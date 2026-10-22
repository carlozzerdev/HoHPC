"""Check push evidence against the supplied DSK, without executing Z80 code."""
import hashlib
import json
import re
from pathlib import Path
from recover import disk_files, game_data_image, rooms

ROOT = Path(__file__).resolve().parents[1]
disk = ROOT / 'Head over Heels (Europe).dsk'
memory = game_data_image(disk_files(disk))
assert memory == (ROOT / 'analysis/relocated.bin').read_bytes()
ranges = [(0x2998, 0x29C7), (0x440F, 0x446F), (0x465C, 0x470D),
          (0x4C86, 0x4CA7), (0x4D98, 0x4DE7), (0x4FEF, 0x5029)]
instructions = []
for line in (ROOT / 'analysis/original/z80.asm').read_text(encoding='utf-8-sig').splitlines():
    match = re.match(r'^([0-9A-F]{4})  ([0-9A-F]+)\s+(.*?)(?:\s+;.*)?$', line)
    if not match:
        continue
    address = int(match[1], 16)
    if not any(start <= address < end for start, end in ranges):
        continue
    raw = bytes.fromhex(match[2])
    assert memory[address:address + len(raw)] == raw, f'ASM mismatch at {address:04X}'
    instructions.append(dict(address=f'{address:04X}', hex=raw.hex(), assembly=match[3]))

types = {}
counts = {}
for room in rooms(memory):
    for obj in room['objects']:
        kind = obj['type']
        counts[kind] = counts.get(kind, 0) + 1
        types.setdefault(kind, obj)
selected = []
for kind, obj in sorted(types.items()):
    if obj['behavior'] not in (1, 10, 28):
        continue
    selected.append(dict(type=kind, descriptor=obj['descriptor'],
                         behavior=obj['behavior'], behaviorAddress=f"{obj['behaviorAddress']:04X}",
                         instances=counts[kind], carryable=(obj['flags'] & 0x60) == 0x40))
report = dict(diskSha256=hashlib.sha256(disk.read_bytes()).hexdigest(),
              memorySha256=hashlib.sha256(memory).hexdigest(),
              evidenceKind='static Z80 bytes and recovered descriptors; no runtime observation',
              instructionsChecked=len(instructions), passivePushTypes=selected,
              matchedInstructions=instructions,
              limits=['Native push chain resolves atomically in the same tick; original propagates contact masks.',
                      'Behavior 10 inertia and spring compression timing are not fully ported.',
                      'Autonomous movers, directional bodies and disappearing objects retain their separate rules.'])
output = ROOT / 'analysis/push-evidence.json'
output.write_text(json.dumps(report, indent=2), encoding='utf-8')
print(f'{len(instructions)} instructions match the supplied disk; {len(selected)} passive push types, '
      f"{sum(x['instances'] for x in selected)} instances. Report: {output}")
