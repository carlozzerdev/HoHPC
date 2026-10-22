namespace HoH26;

public sealed partial class NativeGame
{
    // Original 4455 changes an axis by one raw unit (1/8 tile). Keep that
    // limit even when Heels gets two walking attempts during a logic tick.
    readonly HashSet<Entity> pushedThisTick=[];

    bool TryPush(RoomState room,Character p,float dx,float dy)
    {
        var plan=new HashSet<Entity>();
        float x=p.X+dx,y=p.Y+dy;
        foreach(var e in room.Entities)
            if(IsSolid(e)&&OverlapXY(e,x,y)&&p.Height<e.Height+e.Source.Collider.Height-.0001f&&
                p.Height+BodyHeight>e.Height+.0001f&&!Plan(e))return false;

        // Commit only after checking every body: a blocked chain must not
        // partly move, depend on entity order or mutate the recovered data.
        foreach(var e in plan){e.X+=dx;e.Y+=dy;pushedThisTick.Add(e);}
        return plan.Count>0;

        bool Plan(Entity e)
        {
            if(plan.Contains(e))return true;
            if(!e.Pushable||pushedThisTick.Contains(e))return false;
            var c=e.Source.Collider;var b=room.Data.Bounds;
            float ex=e.X+dx,ey=e.Y+dy;
            if(ex+c.MinX<(b[0]-12)/8f-.0001f||ex+c.MaxX>(b[2]-12)/8f+.0001f||
                ey+c.MinY<(b[1]-12)/8f-.0001f||ey+c.MaxY>(b[3]-12)/8f+.0001f)return false;
            foreach(var other in Characters)
            {
                if(other==p||other.Room!=room.Data.Id)continue;
                if(ObjectHitsCharacter(e,ex,ey,e.Height,other))return false;
            }
            plan.Add(e);
            foreach(var obstacle in room.Entities)
                if(obstacle!=e&&IsSolid(obstacle)&&BodiesOverlap(e,ex,ey,e.Height,obstacle)&&!Plan(obstacle))return false;
            return true;
        }
    }
}
