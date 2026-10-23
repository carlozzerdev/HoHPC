namespace HoH26;

static class PickupVerification
{
    public static int Run(WorldData data)
    {
        int checks=0;
        void Check(bool ok,string message){checks++;if(!ok)throw new Exception("Pickup: "+message);}
        void Tick(NativeGame g)=>g.Step(NativeGame.TickSeconds,0,0);
        foreach(bool heels in new[]{false,true})
        {
            var g=new NativeGame(data);if(heels)g.Switch();
            // These rooms have spike floors. Isolate pickup physics from death
            // while the test waits below the falling item.
            g.Player.Protection=999;
            g.Player.Room=0x3060;g.Player.X=2;g.Player.Y=1;
            var horn=g.Rooms[0x3060].Pickups.Single(p=>p.Data.Kind==1);
            Tick(g);Check(Math.Abs(horn.Height-(7-1/6f))<.001f,"original horn falls from spawn height");
            for(int i=0;i<40;i++)Tick(g);
            Check(Math.Abs(horn.Height-3)<.001f,"horn lands on actual three-block column");
            g.Player.X=4;g.Player.Y=0;g.Player.Height=0;Tick(g);
            Check(!g.HasHooter,"projected overlap at different physical height does not collect");
            g.Player.Height=3;Tick(g);
            Check(g.HasHooter&&horn.Collected,"both characters can acquire original shared hooter flag");
            Check(horn.Data.Z==0,"pickup fall preserves original spawn data");
            Tick(g);Check(g.HasHooter&&horn.Collected,"collected horn stays collected");
            g.Player.Room=0x5260;g.Player.X=7;g.Player.Y=7;g.Player.Height=1;Tick(g);
            if(heels)Check(g.Ammunition==0,"Heels still cannot collect Head ammunition");
            else
            {
                Check(g.Ammunition==6,"Head obtains ammunition after horn");
                g.Fire();Check(g.Rooms[0x5260].Projectiles.Count==1&&g.Ammunition==5,"collected horn enables actual firing");
            }
        }
        var falling=new NativeGame(data);falling.Player.Protection=999;falling.Player.Room=0x3060;
        falling.Player.X=4;falling.Player.Y=0;falling.Player.Height=3;
        for(int i=0;i<30&&!falling.HasHooter;i++)Tick(falling);
        Check(falling.HasHooter,"falling horn is collected when it contacts the player's head");
        var joined=new NativeGame(data);joined.Characters[0].Room=joined.Characters[1].Room;
        joined.Characters[0].Height=1;joined.Switch();joined.Player.Room=0x3060;
        foreach(var p in joined.Characters)p.Protection=999;
        joined.Player.X=4;joined.Player.Y=0;joined.Player.Height=3;
        for(int i=0;i<30&&!joined.HasHooter;i++)Tick(joined);
        Check(joined.HasHooter,"joined body can collect horn on contact");
        return checks;
    }
}
