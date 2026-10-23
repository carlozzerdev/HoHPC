"""Recover data from the supplied CPC disk. No Z80 execution or emulation."""
from pathlib import Path
import hashlib, json, struct

ROOT = Path(__file__).resolve().parents[1]

def game_data_image(files):
    """Static copies described by HEAD's loader; no CPU or hardware runtime.

    C4 exposes expansion RAM at 4000. The loader stages BI2 there, restores
    C0, loads BI1 at 0600, then copies both payloads into the main 64K.
    """
    main=bytearray(65536)
    staged_bi2=files['HEAD.BI2'][0]
    staged_bi1=files['HEAD.BI1'][0]
    if len(staged_bi1)!=0x9f00 or len(staged_bi2)!=0xdc0:
        raise ValueError('Unexpected payload sizes for this loader')
    main[0x600:0xa500]=staged_bi1
    # LDIR 0600 -> 0100, 9F00 bytes, then expansion 4000 -> main A000.
    main[0x100:0xa000]=bytes(main[0x600:0xa500])
    main[0xa000:0xadc0]=staged_bi2
    # CALL 0462: LDDR ADBF -> B897, 47C0 bytes.
    main[0x70d8:0xb898]=bytes(main[0x6600:0xadc0])
    return main

def disk_files(path):
    b = path.read_bytes()
    if not b.startswith(b'EXTENDED CPC DSK File') or b[49] != 1:
        raise ValueError('Expected single-sided Extended CPC DSK')
    sectors, p = [], 256
    for t in range(b[48]):
        size = b[52+t] * 256
        if not size: continue
        h = b[p:p+256]
        if not h.startswith(b'Track-Info'): raise ValueError('Invalid track')
        q, track = p+256, {}
        for s in range(h[21]):
            d = h[24+s*8:32+s*8]
            n = int.from_bytes(d[6:8], 'little') or (128 << d[3])
            if n != 512 or d[4] or d[5]: raise ValueError('Unsupported sector')
            if d[2] in track: raise ValueError('Duplicate sector')
            track[d[2]] = b[q:q+n]; q += n
        if sorted(track) != list(range(0xc1,0xca)): raise ValueError('Expected DATA format')
        sectors.extend(track[k] for k in sorted(track)); p += size
    data = b''.join(sectors)
    files = {}
    for i in range(0,2048,32):
        e = data[i:i+32]
        if e[0] != 0: continue
        name = bytes(x&127 for x in e[1:9]).decode('ascii').rstrip()
        ext = bytes(x&127 for x in e[9:12]).decode('ascii').rstrip()
        name += ('.'+ext) if ext else ''
        chunk = b''.join(data[n*1024:(n+1)*1024] for n in e[16:32] if n)
        files.setdefault(name,[]).append((e[12]+32*e[14],chunk[:e[15]*128]))
    result = {}
    for name,parts in files.items():
        raw = b''.join(x[1] for x in sorted(parts))
        if sum(raw[:67]) & 65535 != int.from_bytes(raw[67:69],'little'):
            raise ValueError('Invalid AMSDOS checksum: '+name)
        length = int.from_bytes(raw[64:67],'little')
        result[name] = (raw[128:128+length], {
            'load':int.from_bytes(raw[21:23],'little'),
            'entry':int.from_bytes(raw[26:28],'little'), 'size':length})
    return result

class Bits:
    def __init__(self,m,p): self.m,self.bit = m,p*8
    def read(self,n):
        v=0
        for _ in range(n):
            v=(v<<1)|((self.m[self.bit//8]>>(7-self.bit%8))&1); self.bit+=1
        return v

def records(m,p):
    for _ in range(1024):
        n=m[p]
        if not n: return
        yield p,n
        p+=n+1
    raise ValueError('Unterminated record table')

def animation_frames(m,s):
        # 374E/37AB: 80 is a special non-sequence code. 81..9F use
        # zero-terminated sequences. Creation advances to frame 1 first.
        if s<128 or not (s&31): return [s]
        p=int.from_bytes(m[0x37f1+(s&31)*2:0x37f3+(s&31)*2],'little')
        result=[]
        for i in range(8):
            if not m[p+i]: break
            result.append(m[p+i])
        if not result: raise ValueError(f'Empty animation {s:02x}')
        return result

def rooms(m):
    macros={m[p+1]:p+2 for p,n in records(m,0x50d0)}
    frames=lambda s: animation_frames(m,s)
    def collider(flags):
        # Relative extents in native grid units; original Z increases down.
        if flags&4:
            x=(-.5,0) if flags&1 else (0,.5)
            y=(-.5,0) if flags&2 else (0,.5)
            h=3
        else:
            hx,hy=((.125,.5) if flags&1 else (.5,.125)) if flags&2 else ((3+(flags&1))/8,)*2
            x=(-hx,hx); y=(-hy,hy); h=1
        return dict(minX=x[0],maxX=x[1],minY=y[0],maxY=y[1],height=h)
    def objects(bits,offset=(0,0,0),depth=0):
        if depth>12: raise ValueError('Recursive room macro')
        out=[]
        for _ in range(1024):
            op=bits.read(8)
            if op==255: return out
            if op>=192:
                xyz=[bits.read(3) for _ in range(3)]
                off=(offset[0]+(xyz[0] if xyz[0]<4 else xyz[0]-8),
                     offset[1]+(xyz[1] if xyz[1]<4 else xyz[1]-8),offset[2]+xyz[2]-7)
                if op not in macros: raise ValueError(f'Unknown macro {op:02x}')
                out.extend(objects(Bits(m,macros[op]),off,depth+1)); continue
            desc=m[0x38c9+op*3:0x38cc+op*3]
            mode=bits.read(2)
            mode=1 if not mode&2 else mode|(bits.read(1)<<2)
            for _ in range(1024):
                flip=(mode>>2)&1 if mode&2 else bits.read(1)
                xyz=[bits.read(3) for _ in range(3)]
                if mode&1 and xyz==[7,7,0]: break
                primary=desc[0]; secondary=None
                if desc[1]>>6:
                    secondary=m[0x36f1+(desc[1]>>6)]
                    if desc[2]&4: primary,secondary=secondary,primary
                sequence=frames(primary)
                secondary_sequence=frames(secondary) if secondary is not None else []
                behavior=desc[1]&63
                out.append(dict(type=op,sprite=sequence[1%len(sequence)],
                    x=xyz[0]+offset[0],y=xyz[1]+offset[1],z=xyz[2]+offset[2],
                    flip=bool(flip),behavior=behavior,flags=desc[2],
                    descriptor=list(desc),animation=sequence,
                    secondarySprite=(secondary_sequence[1%len(secondary_sequence)] if secondary_sequence else -1),
                    secondaryAnimation=secondary_sequence,collider=collider(desc[2]),
                    behaviorAddress=(int.from_bytes(m[0x387f+2*(behavior-1):0x3881+2*(behavior-1)],'little') if behavior else 0)))
                if not mode&1: break
            else: raise ValueError('Unterminated coordinate list')
        raise ValueError('Unterminated object list')
    result=[]
    for table in (0x5241,0x60e6):
        for p,n in records(m,table):
            bits=Bits(m,p+2)
            level=bits.read(4); shape=bits.read(3); area=bits.read(3); theme=bits.read(3)
            door_style=bits.read(3); exits=[bits.read(3) for _ in range(4)]
            floor=bits.read(3)
            bounds=list(m[0x1ece+shape*4:0x1ed2+shape*4])
            room=dict(id=(m[p+1]<<8)|(level<<4),address=p,length=n,
                gridX=m[p+1]&15,gridY=m[p+1]>>4,level=level,shape=shape,
                area=area,theme=theme,doorStyle=door_style,exits=exits,floor=floor,
                bounds=bounds,objects=objects(bits))
            if bits.bit>(p+n+1)*8: raise ValueError(f'Room overrun at {p:04x}')
            result.append(room)
    return result

def bmp(path,w,h,pixels):
    stride=(w*3+3)&~3
    data=b''.join(b''.join(bytes((c&255,(c>>8)&255,(c>>16)&255)) for c in pixels[y*w:(y+1)*w])+bytes(stride-w*3) for y in range(h-1,-1,-1))
    path.write_bytes(b'BM'+struct.pack('<IHHI',54+len(data),0,0,54)+struct.pack('<IiiHHIIiiII',40,w,h,1,24,0,len(data),2835,2835,0,0)+data)

def pickups(m):
    """Independent persistent records spawned by 3EF7, decoded by 3EE3."""
    result=[]
    for i in range(52):
        p=0x3deb+i*4
        room_low,room_high,zk,xy=m[p:p+4]
        kind=zk&15
        animation=animation_frames(m,m[0x3ebb+kind])
        result.append(dict(id=i,address=p,roomId=((room_low&254)|(room_high<<8)),
            x=xy>>4,y=xy&15,z=zk>>4,kind=kind,sprite=m[0x3ebb+kind],
            animation=animation,initialSprite=animation[1%len(animation)],
            effectAddress=(int.from_bytes(m[0x3f89+2*kind:0x3f8b+2*kind],'little') if kind<14 else 0),
            collectedBit=bool(room_low&1),collectible=kind not in (7,14),raw=list(m[p:p+4])))
    return result

def behaviors(m):
    # Names describe demonstrated operations, not guessed visual species.
    names={0:'static',2:'initialize_direction_FE',3:'initialize_direction_FD',
        4:'initialize_direction_F7',5:'initialize_direction_FB',6:'reverse_direction',
        7:'turn_minus_two',8:'turn_plus_two',9:'random_odd_direction',
        11:'random_direction',12:'periodic_random_direction',13:'random_even_direction',
        14:'toggle_entities',15:'pursue_shorter_nonzero_axis',16:'vertical_platform',17:'disappear',
        18:'initialize_vertical_52',21:'turn_minus_two_variant',22:'reverse_direction_variant',
        23:'initialize_vertical_90',26:'pickup',28:'spring',
        29:'consume_shared_direction',30:'publish_shared_direction',
        33:'pursue_within_24',34:'remove_entity',36:'shared_timer',37:'pursue_by_crowns'}
    return [dict(typeId=i,address=(int.from_bytes(m[0x387f+2*(i-1):0x3881+2*(i-1)],'little') if i else 0),
        name=names.get(i,'unclassified')) for i in range(38)]

def sprites(m):
    result=[]
    for i in range(128):
        if i<16: p,w,h=0x8270+i*336,3,28
        elif i<24: p,w,h=0x8a50+(i-16)*192,3,16
        elif i<84: p,w,h=0x8bd0+(i-24)*144,3,12
        else: p,w,h=0xaa30+(i-84)*224,4,14
        if p+w*h*4>0xb898: break
        # Original 1-bit image and mask planes, each widthBytes * height.
        plane=w*h*2
        data=m[p:p+plane]; mask=m[p+plane:p+plane*2]
        colors=[]
        for y in range(h*2):
            for x in range(w*8):
                bit=128>>(x%8)
                d=bool(data[y*w+x//8]&bit); a=bool(mask[y*w+x//8]&bit)
                # 149A: high=(oldHigh&mask)|data; low=(oldLow|data)&mask.
                c=(2 if d else 0)|(1 if d and a else 0)
                transparent=a and not d
                colors.append(-1 if transparent else c)
        result.append(dict(id=i,width=w*8,height=h*2,address=p,pixels=colors))
    return result

def walls(m):
    result=[]
    for theme in range(8):
        base=int.from_bytes(m[0x3d9a+theme*2:0x3d9c+theme*2],'little')
        for variant in range(4):
            p=base+variant*224
            colors=[]
            for y in range(56):
                for x in range(16):
                    bit=128>>(x%8)
                    d=bool(m[p+y*4+(0 if x<8 else 3)]&bit)
                    a=bool(m[p+y*4+(1 if x<8 else 2)]&bit)
                    # 1A80 writes direct colour planes for walls, unlike sprites.
                    colors.append((1 if d else 0)|(2 if a else 0))
            result.append(dict(id=theme*4+variant,width=16,height=56,address=p,pixels=colors))
    return result

def palettes(m):
    hardware=[0xb4b4b4,0xb4b4b4,0x00ffb4,0xffffb4,0x0000b4,0xff00b4,0x00b4b4,0xffb4b4,
        0xff00b4,0xffffb4,0xffff00,0xffffff,0xff0000,0xff00ff,0xffb400,0xffb4ff,
        0x0000b4,0x00ffb4,0x00ff00,0x00ffff,0,0x0000ff,0x00b400,0x00b4ff,
        0xb400b4,0xb4ffb4,0xb4ff00,0xb4ffff,0xb40000,0xb400ff,0xb4b400,0xb4b4ff]
    return [[hardware[v&31] for v in m[0x554+i*4:0x558+i*4]] for i in range(8)]

def floor_tiles(m):
    # 19D6 selects B4B0+48*floor. 19EA and 1A04 consume this as
    # repeating 16-row, two-byte monochrome patterns (remaining rows wrap).
    # 1A04 adds C=10 in alternating columns (bit7 of segment), i.e.
    # shifts the neighbouring 16-pixel column by eight rows. Export both
    # phases so a plain texture repeat reproduces that stagger.
    return [dict(id=i,address=0xb4b0+i*48,width=32,height=16,
        pixels=[1 if m[0xb4b0+i*48+((y+(8 if x>=16 else 0))%16)*2+(x%16)//8]&(128>>(x%8)) else 0
                for y in range(16) for x in range(32)]) for i in range(8)]

def main():
    disk=ROOT/'Head over Heels (Europe).dsk'
    files=disk_files(disk)
    dest=ROOT/'assets'; dest.mkdir(exist_ok=True)
    m=game_data_image(files)
    (ROOT/'analysis'/'relocated.bin').write_bytes(m)
    rs=rooms(m); ss=sprites(m)
    ps=pickups(m)
    (dest/'world.json').write_text(json.dumps(dict(rooms=rs,sprites=ss,pickups=ps,
        behaviors=behaviors(m),initialLives=4,walls=walls(m),palettes=palettes(m),
        floorTiles=floor_tiles(m),
        deathAnimation=list(m[0x24ed:0x24f9]),
        teleportInAnimation=list(m[0x24fb:0x2503]),
        doorFlags=list(m[0x1eb1:0x1eb9]),
        teleportPairs=[list(struct.unpack_from('<HH',m,p)) for p in range(0x3490,0x34e8,4)]),separators=(',',':')))
    manifest=dict(diskSha256=hashlib.sha256(disk.read_bytes()).hexdigest(),files={k:v[1] for k,v in files.items()},rooms=len(rs),sprites=len(ss),
        pickups=len(ps),initialLives=4,
        limitations=['Native gameplay rules not yet fully recovered','Sprite colors, animation, auxiliary patterns and scene fidelity require comparison with the original'])
    (dest/'provenance.json').write_text(json.dumps(manifest,indent=2))
    print(json.dumps(manifest,indent=2))

if __name__=='__main__': main()

