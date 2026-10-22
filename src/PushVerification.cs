namespace HoH26;

static class PushVerification
{
    public static int Run(WorldData data)
    {
        int checks=0;
        void Check(bool ok,string message){checks++;if(!ok)throw new Exception("Push: "+message);}
        void Near(float value,float expected,string message)=>Check(Math.Abs(value-expected)<.001f,message);
        void Tick(NativeGame g,float dx=0,float dy=0)=>g.Step(NativeGame.TickSeconds,dx,dy);
        NativeGame Game(bool heels=false)
        {
            var world=new WorldData {Rooms=[
                new(){Id=0x8a40,Bounds=[8,8,72,72],Exits=[0,0,0,0]},
                new(){Id=0x8940,Bounds=[8,8,72,72],Exits=[0,0,0,0]}]};
            var g=new NativeGame(world);
            if(heels){g.Switch();g.Player.Room=0x8a40;g.Characters[0].Room=0x8940;}
            return g;
        }
        Entity Add(NativeGame g,float x=4.25f,float y=3.5f,int behavior=1,float height=0,int flags=0)
        {
            var e=new Entity(new(){X=x,Y=y,Z=7-height,Behavior=behavior,Flags=flags});
            g.Rooms[g.Player.Room].Entities.Add(e);return e;
        }

        // All four axes, both characters: no bag or sprite-name whitelist.
        foreach(bool heels in new[]{false,true})foreach(var v in new[]{(1,0),(-1,0),(0,1),(0,-1)})
        {
            var g=Game(heels);var p=g.Player;
            var e=Add(g,p.X+v.Item1*.75f,p.Y+v.Item2*.75f,28,flags:0x40);
            float x=e.X,y=e.Y;
            Tick(g,v.Item1,v.Item2);
            Near(e.X,x+v.Item1*.125f,"spring X follows contact direction");
            Near(e.Y,y+v.Item2*.125f,"spring Y follows contact direction");
            Check(!g.HasBag&&g.Carried is null,"push requires neither bag nor pickup");
            // Heels has two attempts after turning, objects still take one raw step.
            Tick(g,v.Item1,v.Item2);x=e.X;y=e.Y;
            Tick(g,v.Item1,v.Item2);
            Near(e.X,x+v.Item1*.125f,"one X step per tick, including fast Heels");
            Near(e.Y,y+v.Item2*.125f,"one Y step per tick, including fast Heels");
        }
        var diagonal=Game();var corner=Add(diagonal,4.25f,4.25f,28);
        Tick(diagonal,1,1);Near(corner.X,4.375f,"diagonal X");Near(corner.Y,4.375f,"diagonal Y");

        // Exercise every recovered passive type using its actual descriptor/collider.
        var types=data.Rooms.SelectMany(r=>r.Objects).GroupBy(o=>o.Type).Select(g=>g.First()).ToArray();
        var passive=types.Where(o=>o.Behavior is 1 or 10 or 28).ToArray();
        Check(passive.Select(o=>o.Type).Order().SequenceEqual(new[]{1,3,10,11,13,21,29,40,48,51,52}),"original passive object inventory");
        foreach(var source in passive)
        {
            var g=Game();var e=new Entity(source){X=4.25f,Y=3.5f,Height=0};
            g.Rooms[g.Player.Room].Entities.Add(e);
            g.Player.X=e.X+source.Collider.MinX-.375f;
            float start=g.Player.X;
            Tick(g,1,0);
            Near(e.X,4.375f,$"recovered type {source.Type} can be pushed");
            Near(g.Player.X,start+.125f,$"player follows recovered type {source.Type}");
        }
        foreach(int behavior in new[]{0,2,3,4,5,14,16,19,20,24,27,30,31})
        {
            var g=Game();var e=Add(g,behavior:behavior);Tick(g,1,0);
            Near(e.X,4.25f,$"non-push behavior {behavior} stays fixed");
            Near(g.Player.X,3.5f,$"non-push behavior {behavior} blocks player");
        }
        var frozen=Game();var ice=Add(frozen);ice.Frozen=true;Tick(frozen,1,0);
        Near(ice.X,4.25f,"frozen body stays fixed");
        var removed=Game();var gone=Add(removed);gone.Removed=true;Tick(removed,1,0);
        Near(gone.X,4.25f,"removed body is untouched");Near(removed.Player.X,3.625f,"removed body does not block");

        var chain=Game();var a=Add(chain);var b=Add(chain,5);
        Tick(chain,1,0);Near(a.X,4.375f,"chain front");Near(b.X,5.125f,"chain second body");
        var blocked=Game();a=Add(blocked);b=Add(blocked,5);Add(blocked,5.75f,behavior:0);
        Tick(blocked,1,0);Near(a.X,4.25f,"blocked chain rolls back front");Near(b.X,5,"blocked chain rolls back rear");
        Near(blocked.Player.X,3.5f,"blocked chain stops player");
        var wall=Game();a=Add(wall,7.125f);wall.Player.X=6.375f;
        Tick(wall,1,0);Near(a.X,7.125f,"whole collider stays within wall");Near(wall.Player.X,6.375f,"wall stops pusher");
        var npc=Game();a=Add(npc);npc.Characters[1].Room=npc.Player.Room;npc.Characters[1].X=5;
        Tick(npc,1,0);Near(a.X,4.25f,"other character blocks push");Near(npc.Characters[1].X,5,"push does not displace other character");

        var above=Game();a=Add(above);above.Player.Height=1;
        Tick(above,1,0);Near(a.X,4.25f,"walking above body does not push it");Near(above.Player.X,3.625f,"walking above body is allowed");
        var underneath=Game();a=Add(underneath,height:2);Add(underneath,4.25f,behavior:0,height:1);
        Tick(underneath,1,0);Near(a.X,4.25f,"body above player is not pushed");Near(underneath.Player.X,3.625f,"player can pass underneath");
        var top=Game();a=Add(top,3.5f,behavior:28);top.Player.Height=1;Tick(top,1,0);
        Near(a.X,3.5f,"standing on spring does not push");Near(top.Player.Height,1,"spring supports player");
        var elevated=Game();a=Add(elevated,behavior:28,height:1);Add(elevated,behavior:0);elevated.Player.Height=1;
        Tick(elevated,1,0);Near(a.X,4.375f,"side contact at elevated height pushes");
        for(int i=0;i<8;i++)Tick(elevated,1,0);
        Tick(elevated);Check(a.Height<1,"spring falls after leaving support");
        Tick(elevated);Check(a.Height>=0,"fall cannot cross floor");

        var stacked=Game();a=Add(stacked,3.5f,behavior:28,height:1);Add(stacked,3.5f,behavior:0);
        Tick(stacked);Near(a.Height,1,"spring resting on solid support does not fall through");
        var supported=Game();a=Add(supported,3.5f,behavior:28,height:1);
        Tick(supported);Near(a.Height,1,"falling spring cannot cross character's head");
        var harmful=Game();a=Add(harmful,flags:0x20);Tick(harmful,1,0);
        Check(harmful.Player.DamageTicks>0,"pushing does not disable contact damage");
        var joined=Game();joined.Characters[0].Room=joined.Characters[1].Room;
        joined.Characters[0].Height=1;joined.Switch();a=Add(joined);
        Tick(joined,1,0);Near(a.X,4.375f,"joined pair pushes");Near(joined.Characters[0].X,joined.Characters[1].X,"joined pair stays synchronized");
        var persistent=Game();a=Add(persistent);Tick(persistent,1,0);persistent.Switch();Tick(persistent);persistent.Switch();
        Near(a.X,4.375f,"pushed position persists across character switch");Near(a.Source.X,4.25f,"recovered source remains immutable");
        float stopped=a.X;for(int i=0;i<4;i++)Tick(persistent);Near(a.X,stopped,"no drift without contact");
        var split=Game();a=Add(split);var whole=Game();b=Add(whole);
        for(int i=0;i<10;i++){split.Step(.04f,1,0);split.Step(.04f,1,0);Tick(whole,1,0);}
        Near(a.X,b.X,"push independent of render frequency");
        return checks;
    }
}
