namespace HoH26;

public sealed partial class NativeGame
{
    readonly OriginalRandom random=new();

    // Known dispatch families from 387F. Unported entries retain their state
    // rather than receiving an invented generic enemy behavior.
    void StepObjects(RoomState room)
    {
        foreach(var e in room.Entities.ToArray())
        {
            if(e.Removed||e.Frozen)continue;
            int behavior=e.Behavior;
            if(behavior is 18 or 23){e.BehaviorState=behavior==18?0x52:0x90;e.Behavior=16;continue;}
            if(behavior==16){StepLift(room,e);continue;}
            if(behavior==28)StepSpring(room,e);
            if(behavior is >=2 and <=5){e.Behavior=0;continue;}
            bool mover=behavior is >=6 and <=9 or >=11 and <=13 or 15 or 21 or 22 or 33 or 37;
            if(behavior is not (1 or 10 or 28)&&!mover)continue;
            // 4E65 draws the periodic random value before its gravity path.
            bool periodic=behavior is 9 or 12 or 13;
            if(periodic&&(random.NextUInt16()&15)==0)ChooseDirection(e,behavior);
            if(behavior is not (21 or 22)&&FallObject(room,e))continue;
            if(!mover)continue;
            bool near=Player.Room==room.Data.Id;
            if(behavior==33)near&=Math.Abs(e.X-Player.X)<=3&&Math.Abs(e.Y-Player.Y)<=3;
            if(behavior is 15 or 33 or 37)
            {
                if(!near)continue;
                int dx=Math.Sign(Player.X-e.X),dy=Math.Sign(Player.Y-e.Y);
                if(behavior==15)
                {
                    // 4F55 selects the shorter nonzero axis; ties prefer X.
                    bool moveY=Math.Abs(Player.Y-e.Y)<Math.Abs(Player.X-e.X)?dy!=0:dx==0;
                    if(moveY)dx=0;else dy=0;
                }
                if(behavior==37&&(CrownMask&15)==15){dx=-dx;dy=-dy;}
                int dir=Array.FindIndex(vectors,v=>v.x==dx&&v.y==dy);
                if(dir<0)continue;e.Direction=dir;
            }
            var vector=vectors[e.Direction];
            if(!MoveObject(room,e,vector.x/8f,vector.y/8f))ChooseDirection(e,behavior);
        }
    }
    void ChooseDirection(Entity e,int behavior)
    {
        switch(behavior)
        {
            case 6:case 22:e.Direction=(e.Direction+4)&7;break;
            case 7:case 21:e.Direction=(e.Direction-2)&7;break;
            case 8:e.Direction=(e.Direction+2)&7;break;
            case 9:case 11:case 12:case 13:
                int dir;
                do
                {
                    int value=random.NextUInt16()&7;
                    dir=behavior==9?(value&6)|1:behavior==13?value&6:value;
                }while(dir==e.Direction);
                e.Direction=dir;break;
        }
    }
    static bool BodiesOverlap(Entity a,float x,float y,float z,Entity b)
    {
        var ac=a.Source.Collider;var bc=b.Source.Collider;
        return x+ac.MaxX>b.X+bc.MinX+.0001f&&x+ac.MinX<b.X+bc.MaxX-.0001f&&
            y+ac.MaxY>b.Y+bc.MinY+.0001f&&y+ac.MinY<b.Y+bc.MaxY-.0001f&&
            z+ac.Height>b.Height+.0001f&&z<b.Height+bc.Height-.0001f;
    }
    bool ObjectHitsCharacter(Entity e,float x,float y,float z,Character p)
    {
        if(Joined&&p.IsHead)return false; // Included in the joined player's height.
        var c=e.Source.Collider;
        float height=Joined&&p==Player?2:1;
        return x+c.MaxX>p.X-.375f+.0001f&&x+c.MinX<p.X+.375f-.0001f&&
            y+c.MaxY>p.Y-.375f+.0001f&&y+c.MinY<p.Y+.375f-.0001f&&
            z+c.Height>p.Height+.0001f&&z<p.Height+height-.0001f;
    }
    bool FallObject(RoomState room,Entity e)
    {
        if(e.Height<=0)return false;
        float height=Math.Max(0,e.Height-1/6f);
        if(room.Entities.Any(o=>o!=e&&!o.Removed&&BodiesOverlap(e,e.X,e.Y,height,o)))return false;
        if(Characters.Any(p=>p.Room==room.Data.Id&&ObjectHitsCharacter(e,e.X,e.Y,height,p)))return false;
        e.Height=height;return true;
    }
    bool MoveObject(RoomState room,Entity e,float dx,float dy)
    {
        float x=e.X+dx,y=e.Y+dy;var c=e.Source.Collider;var b=room.Data.Bounds;
        if(x+c.MinX<(b[0]-12)/8f||x+c.MaxX>(b[2]-12)/8f||
            y+c.MinY<(b[1]-12)/8f||y+c.MaxY>(b[3]-12)/8f)return false;
        if(room.Entities.Any(o=>o!=e&&!o.Removed&&BodiesOverlap(e,x,y,e.Height,o)))return false;
        var p=Player;
        if(p.Room==room.Data.Id&&x+c.MaxX>p.X-.375f&&x+c.MinX<p.X+.375f&&
            y+c.MaxY>p.Y-.375f&&y+c.MinY<p.Y+.375f&&e.Height+c.Height>p.Height&&e.Height<p.Height+BodyHeight)
        {if(e.Dangerous)Damage(p);return false;}
        e.X=x;e.Y=y;e.Moved=true;return true;
    }
}
