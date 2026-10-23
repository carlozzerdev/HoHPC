namespace HoH26;

static class InteractionVerification
{
    public static int Run()
    {
        int checks=0;
        void Check(bool ok,string message){checks++;if(!ok)throw new Exception("Interaction: "+message);}
        void Tick(NativeGame g)=>g.Step(NativeGame.TickSeconds,0,0);
        WorldData World(int floor=1)=>new(){Rooms=[
            new(){Id=0x8a40,Bounds=[8,8,72,72],Exits=[0,0,0,0],Floor=floor},
            new(){Id=0x8940,Bounds=[8,8,72,72],Exits=[0,0,0,0],Floor=1}]};
        var death=new NativeGame(World(6));Tick(death);
        Check(death.Player.DamageTicks==12&&death.Player.Sprite==36,"spike floor begins original death sequence at floor level");
        int[] sequence=[36,36,37,37,37,38,38,38,38,38,38,38];
        for(int i=0;i<12;i++)
        {
            Check(death.Player.Sprite==sequence[i],"death follows original sprite table");
            Tick(death);
        }
        Check(death.Player.Lives==3&&death.Player.DamageTicks==0,"death deducts one life after twelve updates");
        Check(death.Player.Protection>0,"respawn protects against immediately repeating spike death");
        var safe=new NativeGame(World(6));safe.Player.Height=1;safe.Player.Grounded=false;
        safe.Rooms[safe.Player.Room].Entities.Add(new(new(){X=3.5f,Y=3.5f,Z=7,Sprite=43}));
        Tick(safe);Check(safe.Player.DamageTicks==0,"solid platform protects against spike floor below");
        var itemWorld=World();itemWorld.Pickups.Add(new(){RoomId=0x8a40,X=3.5f,Y=3.5f,Z=7,Kind=0,Sprite=52,InitialSprite=52,Animation=[52,53],Collectible=true});
        var pickup=new NativeGame(itemWorld);Tick(pickup);var room=pickup.Rooms[pickup.Player.Room];
        Check(pickup.HasBag&&room.Pickups[0].Collected&&room.Effects.Count==1,"collecting grants item and retains transient visual");
        for(int i=0;i<8;i++)Tick(pickup);
        Check(room.Effects.Count==0,"collection animation expires after eight updates");
        var entry=new NativeGame(World(6));entry.Player.Entry=(entry.Player.Room,2,1,1);
        Tick(entry);for(int i=0;i<12;i++)Tick(entry);
        Check(entry.Player.X==2&&entry.Player.Y==1&&entry.Player.Height==1,"respawn restores individual entry position and height");
        return checks;
    }
}
