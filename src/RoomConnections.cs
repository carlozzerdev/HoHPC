namespace HoH26;

// Exit code 1 joins adjacent records into one continuous room (1F55–1FA2).
public sealed class RoomConnections
{
    readonly Dictionary<int,Dictionary<int,PointF>> groups=[];
    public RoomConnections(IEnumerable<RoomData> source)
    {
        var rooms=source.ToDictionary(r=>r.Id);
        foreach(var root in rooms.Values.OrderBy(r=>r.Id))
        {
            if(groups.ContainsKey(root.Id))continue;
            var group=new Dictionary<int,PointF>{{root.Id,PointF.Empty}};
            var pending=new Queue<int>();pending.Enqueue(root.Id);
            while(pending.TryDequeue(out int id))
            {
                var room=rooms[id];
                for(int side=0;side<4;side++)
                {
                    if(room.Exits[side]!=1)continue;
                    int next=Neighbor(room,side);
                    if(!rooms.TryGetValue(next,out var dest)||dest.Exits[(side+2)%4]!=1)continue;
                    float dx=side==1?(room.Bounds[2]-dest.Bounds[0])/8f:side==3?(room.Bounds[0]-dest.Bounds[2])/8f:0;
                    float dy=side==0?(room.Bounds[3]-dest.Bounds[1])/8f:side==2?(room.Bounds[1]-dest.Bounds[3])/8f:0;
                    var p=group[id];var target=new PointF(p.X+dx,p.Y+dy);
                    if(group.TryAdd(next,target))pending.Enqueue(next);
                }
            }
            foreach(int id in group.Keys)groups[id]=group;
        }
    }
    public IReadOnlyDictionary<int,PointF> Group(int room)=>groups[room];
    public PointF Offset(int from,int to)
    {
        var g=groups[from];return new(g[to].X-g[from].X,g[to].Y-g[from].Y);
    }
    public static PointF ProjectOffset(PointF offset)=>new((offset.X-offset.Y)*16,-(offset.X+offset.Y)*8);
    public static int Neighbor(RoomData r,int side)
    {
        (int dx,int dy)=side switch{0=>(0,1),1=>(1,0),2=>(0,-1),_=>(-1,0)};
        return ((((r.GridY+dx)&15)*16+((r.GridX+dy)&15))<<8)|(r.Level<<4);
    }
}
