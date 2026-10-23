namespace HoH26;

public sealed partial class NativeGame
{
    internal static bool HasFloor(RoomData room)=>room.Floor!=7;
    bool StandingOn(Character p,Entity e,int room)=>p.Room==room&&p.JumpTicks==0&&
        OverlapXY(e,p.X,p.Y)&&Math.Abs(p.Height-e.Height-e.Source.Collider.Height)<.001f;

    void StepSpring(RoomState room,Entity e)
    {
        bool loaded=Characters.Any(p=>!(Joined&&p.IsHead)&&StandingOn(p,e,room.Data.Id))||
            room.Entities.Any(o=>o!=e&&IsSolid(o)&&Math.Abs(o.Height-e.Height-e.Source.Collider.Height)<.001f&&
                BodiesOverlap(e,e.X,e.Y,e.Height+.001f,o));
        if(loaded){e.SpriteOverride=44;e.SpringFrame=-1;return;}
        // 4D98/3847: released spring oscillates 43,44,43,44,43.
        int[] release=[43,44,43,44,43];
        if(e.SpringFrame<0)
        {
            if((e.Sprite&127)!=44)return;
            Sound?.Invoke(SoundCue.Spring);
            e.SpringFrame=0;
        }
        else if(++e.SpringFrame==release.Length){e.SpringFrame=-1;e.SpriteOverride=43;return;}
        e.SpriteOverride=release[e.SpringFrame];
    }

    void StepLift(RoomState room,Entity e)
    {
        Sound?.Invoke(SoundCue.LiftMove);
        // 4E82–4EF1: packed lower/upper limits and bit 3 direction.
        int state=e.BehaviorState;
        bool up=(state&8)!=0;
        float low=(state&7)+1/6f;
        float high=(state>>4)&15; // Six original Z units per tile.
        bool atEnd=up?e.Height>=high-.0001f:e.Height<low-.0001f;
        if(atEnd)
        {
            // Original 4E81 holds a two-update dwell at the endpoints.
            if(++e.BehaviorTimer>=2){e.BehaviorState^=8;e.BehaviorTimer=0;}
            return;
        }
        float dz=up?1/6f:-1/6f;
        var riders=Characters.Where(p=>!(Joined&&p.IsHead)&&StandingOn(p,e,room.Data.Id)).ToArray();
        float height=e.Height+dz;
        if(room.Entities.Any(o=>o!=e&&IsSolid(o)&&BodiesOverlap(e,e.X,e.Y,height,o)))return;
        if(Characters.Any(p=>p.Room==room.Data.Id&&!riders.Contains(p)&&ObjectHitsCharacter(e,e.X,e.Y,height,p)))return;
        foreach(var p in riders)
        {
            // Prevent the lift from forcing a rider through a solid ceiling/object.
            float body=Joined&&p==Player?2:1;
            if(room.Entities.Any(o=>o!=e&&IsSolid(o)&&OverlapXY(o,p.X,p.Y)&&
                p.Height+dz<o.Height+o.Source.Collider.Height-.0001f&&p.Height+dz+body>o.Height+.0001f))return;
        }
        e.Height=height;
        foreach(var p in riders){p.Height+=dz;p.Grounded=true;}
    }

    void CheckVerticalBoundary(Character p)
    {
        var room=Rooms[p.Room].Data;
        if(p.Height>10.0001f)
        {
            int id=(p.Room&0xff00)|(((room.Level-1)&15)<<4);
            // 21D8 examines the actual room above; only floor 6/7 is open.
            if(Rooms.TryGetValue(id,out var above)&&above.Data.Floor is 6 or 7)
            {
                p.Room=id;p.Height=Joined?2:1;p.JumpTicks=0;p.Grounded=false;
                visited.Add(id);Notice="";
                RememberEntry(p);
            }
            else {p.Height=10;p.JumpTicks=0;p.Grounded=false;}
        }
        else if(!HasFloor(room)&&p.Height<-.0001f)
        {
            int id=(p.Room&0xff00)|(((room.Level+1)&15)<<4);
            if(Rooms.ContainsKey(id))
            {
                p.Room=id;p.Height=10;p.JumpTicks=0;p.Grounded=false;
                visited.Add(id);Notice="";
                RememberEntry(p);
            }
            // No fabricated destination if the recovered map has no room below.
            else {p.Height=0;Damage(p);}
        }
    }
}
