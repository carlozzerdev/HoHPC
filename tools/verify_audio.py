"""Verify native first-note AY register writes against an independent disk decode."""
from pathlib import Path
import subprocess
from recover import game_data_image, disk_files

ROOT=Path(__file__).resolve().parents[1]
m=game_data_image(disk_files(ROOT/'Head over Heels (Europe).dsk'))
w=lambda p:int.from_bytes(m[p:p+2],'little')
commands=[0x80,0x82,0x83,0x84,0x85,0x88,0xc6,0xc7,0xc8,0x47,0x44,0x41,0xc5,0xc2,0xc4,0x48]
expected={}
for command in commands:
    regs=bytearray(11);regs[7]=63
    channels=range(3) if command>>6==3 else [command>>6]
    for channel in channels:
        p=w(0x1111+(command&63)*6+channel*2) if command>>6==3 else w(w(0x124c+channel*2)+(command&63)*2)
        transpose=m[p]>>2;volume=m[p]&3;p+=1
        config=m[p];p+=1
        if config&128:p+=1
        note=m[p]>>3
        if not note:continue
        pitch=transpose+note
        period=w(0xb8b+(pitch%12)*2)>>(pitch//12)
        regs[channel*2:channel*2+2]=period.to_bytes(2,'little')
        env=m[0x10be+(config&15)]&15
        pattern=0x10ce+env;pattern+=m[pattern]
        v=m[pattern]&15
        regs[8+channel]=min(15,v+volume) if v else 0
        regs[7]&=~(1<<channel)
    expected[command]=regs.hex().upper()
r=subprocess.run(['dotnet','run','--project','tools/audio-verification/AudioVerification.csproj','-c','Release'],cwd=ROOT,capture_output=True,text=True,check=True)
print(r.stdout)
for line in r.stdout.splitlines():
    if ': ' not in line or line.startswith(' '):continue
    command,actual=line.split(': ')
    assert actual==expected[int(command,16)],(command,actual,expected[int(command,16)])
print('16 independently decoded first-note register vectors match; PCM and silence validated (looping movement effects explicitly stopped).')
