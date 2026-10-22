namespace HoH26;

public sealed partial class NativeGame
{
    void StepPickups(RoomState room)
    {
        // 3F2B assigns behavior 26; 4DCF falls through to 4DDC/5029.
        // The decoded Z is a spawn position, not an immutable collectible height.
        foreach(var item in room.Pickups)
        {
            if(item.Collected)continue;
            float support=HasFloor(room.Data)?0:float.NegativeInfinity;
            foreach(var e in room.Entities)
                if(IsSolid(e)&&OverlapXY(e,item.Data.X,item.Data.Y)&&
                    e.Height+e.Source.Collider.Height<=item.Height+.0001f)
                    support=Math.Max(support,e.Height+e.Source.Collider.Height);
            foreach(var p in Characters)
            {
                if(p.Room!=room.Data.Id||Joined&&p.IsHead)continue;
                float top=p.Height+(Joined&&p==Player?2:1);
                if(Math.Abs(p.X-item.Data.X)<.75f&&Math.Abs(p.Y-item.Data.Y)<.75f&&top<=item.Height+.0001f)
                    support=Math.Max(support,top);
            }
            item.Height=Math.Max(support,item.Height-1/6f);
        }
    }
}
