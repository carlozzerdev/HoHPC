using System.Text.Json;

namespace HoH26;

public sealed class WorldData
{
    public List<RoomData> Rooms { get; set; } = [];
    public List<SpriteData> Sprites { get; set; } = [];
    public List<PickupData> Pickups { get; set; } = [];
    public int InitialLives { get; set; } = 4;
    public List<SpriteData> Walls { get; set; }=[];
    public List<SpriteData> FloorTiles { get; set; }=[];
    public int[] DeathAnimation { get; set; }=[];
    public int[] TeleportInAnimation { get; set; }=[];
    public int[] DoorFlags { get; set; }=[];
    public int[][] Palettes { get; set; }=[];
    public int[][] TeleportPairs { get; set; }=[];
    public static WorldData Load() => JsonSerializer.Deserialize<WorldData>(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"assets","world.json")),
        new JsonSerializerOptions { PropertyNameCaseInsensitive=true })!;
}
public sealed class PickupData
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public int Kind { get; set; }
    public int Sprite { get; set; }
    public int InitialSprite { get; set; }
    public int[] Animation { get; set; } = [];
    public bool Collectible { get; set; }
    public bool CollectedBit { get; set; }
}
public sealed class PickupState(PickupData data)
{
    public PickupData Data { get; }=data;
    public bool Collected=data.CollectedBit;
    public int Frame;
    public int Sprite=>Data.Animation.Length==0?Data.InitialSprite:Data.Animation[Frame%Data.Animation.Length];
    public float Height=7-data.Z;
}
public sealed class RoomData
{
    public int Id { get; set; }
    public int Address { get; set; }
    public int GridX { get; set; }
    public int GridY { get; set; }
    public int Level { get; set; }
    public int Theme { get; set; }
    public int DoorStyle { get; set; }
    public int Area { get; set; }
    public int Shape { get; set; }
    public int Floor { get; set; }
    public int[] Bounds { get; set; } = [];
    public int[] Exits { get; set; } = [];
    public List<ObjectData> Objects { get; set; } = [];
}
public sealed class ObjectData
{
    public int Type { get; set; }
    public int Sprite { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public bool Flip { get; set; }
    public int Behavior { get; set; }
    public int Flags { get; set; }
    public int[] Descriptor { get; set; } = [];
    public int[] Animation { get; set; } = [];
    public int[] SecondaryAnimation { get; set; } = [];
    public int SecondarySprite { get; set; } = -1;
    public int BehaviorAddress { get; set; }
    public ColliderData Collider { get; set; } = new();
}
public sealed class ColliderData
{
    public float MinX { get; set; }=-.375f;
    public float MaxX { get; set; }=.375f;
    public float MinY { get; set; }=-.375f;
    public float MaxY { get; set; }=.375f;
    public float Height { get; set; }=1;
}
public sealed class SpriteData
{
    public int Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Address { get; set; }
    public int[] Pixels { get; set; } = [];
}

// Each room owns its state. Simulation never depends on the current camera.
// Original assets remain immutable; native entities carry mutable positions.
public sealed class RoomState(RoomData data)
{
    public RoomData Data { get; } = data;
    public List<Entity> Entities { get; } = data.Objects.Select(o => new Entity(o)).ToList();
    public List<PickupState> Pickups { get; }=[];
    public List<Projectile> Projectiles { get; }=[];
    public List<TransientEffect> Effects { get; }=[];
}
public sealed class TransientEffect(float x,float y,float height,int[] frames)
{
    public float X=x,Y=y,Height=height;
    public int Frame;
    public bool Finished=>Frame>=frames.Length;
    public int Sprite=>frames[Math.Min(Frame,frames.Length-1)]&127;
    public bool Flip=>(frames[Math.Min(Frame,frames.Length-1)]&128)!=0;
}
public sealed class Projectile(float x,float y,float height,int direction)
{
    public float X=x,Y=y,Height=height;
    public int Direction=direction,Frame,BurstFrame=-1;
    public int Sprite => BurstFrame<0?new[]{36,37,37,36}[Frame%4]:new[]{164,36,37,38}[Math.Min(BurstFrame,3)];
}
public sealed class Entity(ObjectData source)
{
    public ObjectData Source { get; } = source;
    public float X=source.X, Y=source.Y, Height=7-source.Z;
    public bool Removed;
    public int AnimationFrame=1;
    public bool Frozen;
    public int Direction,Behavior=source.Behavior;
    public int BehaviorState,BehaviorTimer;
    public int? SpriteOverride;
    public int SpringFrame=-1;
    public bool Moved;
    public int RuntimeFlags => ((Source.Flags&0x60)==0x60?Source.Flags&~0x40:Source.Flags)&~0x84;
    public bool Dangerous => (RuntimeFlags&0x20)!=0&&(RuntimeFlags&0x40)==0;
    public bool Carryable => (RuntimeFlags&0x40)!=0;
    // 4DDC/4FEF consume contact direction; 4D98 (spring) and idle 4C86
    // reach that same path. This is independent of the bag's flag 40.
    public bool Pushable => !Removed&&!Frozen&&Behavior is 1 or 10 or 28;
    public int Sprite => SpriteOverride??(Source.Animation.Length==0?Source.Sprite:Source.Animation[AnimationFrame%Source.Animation.Length]);
}
public sealed class Character(int room, bool head)
{
    public int Room=room;
    public bool IsHead=head;
    public float X=3.5f,Y=3.5f,Height;
    public bool Grounded=true,Flip;
    public int Frame;
    public int Direction=2,PreviousDirection=-1,TurnDelay,JumpTicks,Lives=4;
    public int FastSteps,LongJumps,Protection;
    public int DamageTicks;
    public int TeleportTicks;
    public bool TeleportArriving;
    public bool JumpFlight;
    public (int room,float x,float y,float height) Entry=(room,3.5f,3.5f,0);
    static readonly int[] deathFrames=[0x24,0xa4,0xa5,0x25,0xa5,0xa6,0x26,0x26,0xa6,0xa6,0x26,0x26];
    static readonly int[] arrivalFrames=[0x26,0xa6,0x26,0xa6,0xa5,0x25,0x24,0xa5];
    int? SpecialFrame=>DamageTicks>0?deathFrames[Math.Clamp(12-DamageTicks,0,11)]:
        TeleportTicks>0?(TeleportArriving?arrivalFrames[Math.Clamp(8-TeleportTicks,0,7)]:deathFrames[Math.Clamp(12-TeleportTicks,0,11)]):null;
    public bool Visible=>Lives>0;
    public bool RenderFlip=>SpecialFrame is int frame?((frame&128)!=0)^!IsHead:Flip;
    public int Sprite => SpecialFrame is int frame?frame&127:(IsHead?
        ((Direction&4)==0?new[]{30,31,30,32}:new[]{33,34,33,35}):
        ((Direction&4)==0?new[]{24,25,24,26}:new[]{27,28,27,29}))[Frame%4];
}
