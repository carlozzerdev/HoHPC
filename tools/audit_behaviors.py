"""Reproducible static evidence and implementation inventory for CPC mechanics."""
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
from recover import disk_files, game_data_image, rooms

ROOT = Path(__file__).resolve().parents[1]
disk = ROOT / 'Head over Heels (Europe).dsk'
memory = game_data_image(disk_files(disk))
world = rooms(memory)
counts = Counter(o['behavior'] for r in world for o in r['objects'])
implemented = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 16, 18, 21, 22, 23, 26, 27, 28, 33, 37}
names = {0: 'static', 1: 'gravity/contact push', 2: 'direction mask FE',
         3: 'direction mask FD', 4: 'direction mask F7', 5: 'direction mask FB',
         6: 'reverse direction', 7: 'turn -2', 8: 'turn +2', 9: 'random odd direction',
         10: 'inertial/contact body', 11: 'random direction', 12: 'periodic random direction',
         13: 'random even direction', 14: 'switch', 15: 'pursue shorter nonzero axis',
         16: 'vertical platform', 17: 'disappear', 18: 'initialize vertical 52',
         19: 'conditional gravity', 20: 'conditional disappearance',
         21: 'turn -2 variant', 22: 'reverse variant', 23: 'initialize vertical 90',
         24: 'conditional disappearance C0', 25: 'double interaction/projectile',
         26: 'pickup', 27: 'teleport animation', 28: 'spring',
         29: 'shared direction consumer', 30: 'shared direction publisher',
         31: 'disappear depending on active character', 32: 'relative interaction',
         33: 'nearby diagonal pursuit', 34: 'remove body', 35: 'interaction variant',
         36: 'shared timer', 37: 'pursue/flee by crowns'}
inventory = []
for behavior in range(38):
    address = int.from_bytes(memory[0x387f+2*(behavior-1):0x3881+2*(behavior-1)], 'little') if behavior else 0
    inventory.append(dict(id=behavior, address=f'{address:04X}' if address else None,
                          name=names[behavior], roomInstances=counts[behavior],
                          nativeStatus='partial' if behavior in implemented else 'pending',
                          note='Explicit native pickup and projectile paths also exist outside room dispatch.' if behavior in (25, 26) else ''))

ranges = [(0x21d8, 0x21ef), (0x25bf, 0x25d5), (0x2a10, 0x2a7f),
          (0x2caa, 0x2ce9), (0x4d2e, 0x4d3b), (0x4d98, 0x4dcf),
          (0x4e82, 0x4ef4), (0x4f36, 0x4fb6)]
matched = []
for line in (ROOT/'analysis/original/z80.asm').read_text(encoding='utf-8-sig').splitlines():
    match = re.match(r'^([0-9A-F]{4})  ([0-9A-F]+)\s+(.*?)(?:\s+;.*)?$', line)
    if not match:
        continue
    address = int(match[1], 16)
    if not any(a <= address < b for a, b in ranges):
        continue
    raw = bytes.fromhex(match[2])
    assert memory[address:address+len(raw)] == raw, f'Disassembly mismatch at {address:04X}'
    matched.append(dict(address=f'{address:04X}', bytes=raw.hex(), assembly=match[3]))

by_id = {r['id']: r for r in world}
lower, upper = by_id[0x6160], by_id[0x6150]
assert upper['floor'] == 7
assert any(o['behavior'] == 23 and (o['x'], o['y']) == (6, 3) for o in lower['objects'])
report = dict(evidenceKind='static bytes from supplied CPC disk; not a runtime equivalence test',
              diskSha256=hashlib.sha256(disk.read_bytes()).hexdigest(),
              memorySha256=hashlib.sha256(memory).hexdigest(),
              roomCount=len(world), roomObjects=sum(counts.values()),
              dispatchEntries=37, coverage=inventory, matchedInstructions=matched,
              verticalPuzzle=dict(lower='6160', upper='6150', upperFloor=upper['floor'],
                                  lift=[o for o in lower['objects'] if o['behavior'] == 23]),
              validatedFidelity=False)
output = ROOT/'analysis/mechanics-evidence.json'
output.write_text(json.dumps(report, indent=2), encoding='utf-8')
print(f'{len(matched)} original instructions checked; 37 behavior dispatch entries audited; '
      f'{len(world)} rooms, {sum(counts.values())} objects. Evidence: {output}')
