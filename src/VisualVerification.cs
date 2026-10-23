namespace HoH26;

static class VisualVerification
{
    public static void Run(WorldData data,Renderer renderer,string directory)
    {
        void Sheet(string name,int columns,IEnumerable<(string label,RoomState room,Character[] people)> scenes)
        {
            var items=scenes.ToArray();
            using var result=new Bitmap(columns*320,((items.Length+columns-1)/columns)*240);
            using var graphics=Graphics.FromImage(result);graphics.Clear(Color.Black);
            using var font=new Font("Consolas",10);
            for(int i=0;i<items.Length;i++)
            {
                using var room=renderer.Room(items[i].room,items[i].people);
                int x=i%columns*320,y=i/columns*240;
                graphics.DrawImageUnscaled(room,x,y);
                graphics.DrawString(items[i].label,font,Brushes.White,x+8,y+220);
            }
            result.Save(Path.Combine(directory,name+".png"));
        }
        RoomState Empty(int floor,int style=0,int side=-1)
        {
            var room=new RoomData{Id=0x8a40,Floor=floor,DoorStyle=style,Bounds=[8,8,72,72],Exits=[0,0,0,0]};
            if(side>=0)room.Exits[side]=4;
            return new(room);
        }
        Sheet("original-floors",4,Enumerable.Range(0,8).Select(i=>($"Suelo {i}"+(i==6?" - pinchos":i==7?" - hueco":""),Empty(i),Array.Empty<Character>())));
        Sheet("door-details",4,Enumerable.Range(0,16).Select(i=>($"Estilo {i/4} / lado {i%4}",Empty(1,i/4,i%4),Array.Empty<Character>())));
        Sheet("death-animation",4,Enumerable.Range(0,12).Select(i=>($"Muerte / tick {i}",Empty(6),new[]{new Character(0x8a40,true){DamageTicks=12-i}})));
        var springScenes=new List<(string,RoomState,Character[])>();
        foreach(int sprite in new[]{43,44,43,44,43})
        {
            var room=Empty(1);room.Entities.Add(new(new(){X=3.5f,Y=3.5f,Z=7,Sprite=sprite}));
            springScenes.Add(($"Muelle / sprite {sprite}",room,[]));
        }
        Sheet("spring-animation",5,springScenes);
        var collection=new NativeGame(data);collection.Player.Room=0x3060;
        collection.Player.Protection=999;
        collection.Player.X=2;collection.Player.Y=0;
        for(int i=0;i<42;i++)collection.Step(NativeGame.TickSeconds,0,0);
        collection.Player.X=4;collection.Player.Height=3;
        // Save each state before advancing: the room owns mutable effects.
        using var strip=new Bitmap(320*5,240);using var g=Graphics.FromImage(strip);g.Clear(Color.Black);
        for(int i=0;i<5;i++)
        {
            collection.Step(NativeGame.TickSeconds,0,0);
            using var frame=renderer.Room(collection.Rooms[0x3060],[collection.Player]);g.DrawImageUnscaled(frame,320*i,0);
        }
        strip.Save(Path.Combine(directory,"pickup-animation.png"));
        var teleport=new NativeGame(data);teleport.Player.X=7;teleport.Player.Y=6;teleport.Player.Height=1;
        teleport.Jump();teleport.Step(NativeGame.TickSeconds,0,0);
        using var transportStrip=new Bitmap(320*5,240*4);using var transportGraphics=Graphics.FromImage(transportStrip);
        using var caption=new Font("Consolas",10);transportGraphics.Clear(Color.Black);
        for(int tick=0;tick<20;tick++)
        {
            int x=tick%5*320,y=tick/5*240;
            using var frame=renderer.Room(teleport.Rooms[teleport.Player.Room],teleport.Characters.Where(p=>p.Room==teleport.Player.Room));
            transportGraphics.DrawImageUnscaled(frame,x,y);
            transportGraphics.DrawString($"{(tick<12?"Salida":"Llegada")} {tick} / sprite {teleport.Player.Sprite}",caption,Brushes.White,x+8,y+220);
            teleport.Step(NativeGame.TickSeconds,0,0);
        }
        transportStrip.Save(Path.Combine(directory,"teleport-animation.png"));
    }
}
