namespace HoH26;

static class ConnectedRoomVerification
{
    public static int Run(WorldData data)
    {
        int checks=0;
        void Check(bool ok,string message){checks++;if(!ok)throw new Exception("Connected rooms: "+message);}
        var connections=new RoomConnections(data.Rooms);
        var group=connections.Group(0x4260);
        Check(group.Count==2&&group.ContainsKey(0x4360),"4260 joins 4360 into the original long room");
        Check(connections.Offset(0x4260,0x4360)==new PointF(0,6),"six-tile halves touch at their floor edge");
        Check(group.Keys.Sum(id=>data.Rooms.Single(r=>r.Id==id).Objects.Count)==12,"both original rows of six objects are present");
        var game=new NativeGame(data);var p=game.Player;p.Room=0x4260;p.X=2;p.Y=6.375f;p.Height=0;
        game.Step(NativeGame.TickSeconds,0,1);
        game.Step(NativeGame.TickSeconds,0,1);
        Check(p.Room==0x4360&&p.X==2&&p.Height==0,"open edge can be crossed away from a doorway center");
        for(int i=0;i<10&&p.Room==0x4360;i++)game.Step(NativeGame.TickSeconds,0,-1);
        Check(p.Room==0x4260,"continuous room can be crossed in both directions");
        p.Room=0x4260;p.X=2;p.Y=6.5f;p.Height=2;p.JumpTicks=4;p.Grounded=false;p.JumpFlight=true;
        game.Step(NativeGame.TickSeconds,0,1);
        Check(p.Room==0x4360&&p.JumpTicks==3&&p.Height>2&&!p.Grounded,"jump continues across internal boundary");
        foreach(var room in data.Rooms)for(int side=0;side<4;side++)
        {
            if(room.Exits[side]!=1)continue;
            int id=RoomConnections.Neighbor(room,side);
            var dest=data.Rooms.FirstOrDefault(r=>r.Id==id);
            Check(dest is not null&&dest.Exits[(side+2)%4]==1,"every continuous exit has a matching half");
            Check(connections.Group(room.Id).ContainsKey(id),"every continuous exit is included in the scene");
        }
        return checks;
    }
}
