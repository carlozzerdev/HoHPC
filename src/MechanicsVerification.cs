namespace HoH26;

static class MechanicsVerification
{
    public static int Run(WorldData data)
    {
        int checks=0;
        void Check(bool ok,string message){checks++;if(!ok)throw new Exception("Mechanics: "+message);}
        void Near(float a,float b,string message)=>Check(Math.Abs(a-b)<.001f,message);
        void Tick(NativeGame g,float x=0,float y=0)=>g.Step(NativeGame.TickSeconds,x,y);
        NativeGame Empty(bool heels=false)
        {
            var g=new NativeGame(new(){Rooms=[
                new(){Id=0x8a40,Bounds=[8,8,72,72],Exits=[0,0,0,0],Floor=1},
                new(){Id=0x8940,Bounds=[8,8,72,72],Exits=[0,0,0,0],Floor=1}]});
            if(heels)g.Switch();return g;
        }
        Entity Add(NativeGame g,int behavior,int sprite=0,float height=0)=>AddAt(g,behavior,sprite,g.Player.X,g.Player.Y,height);
        Entity AddAt(NativeGame g,int behavior,int sprite,float x,float y,float height)
        {
            var e=new Entity(new(){X=x,Y=y,Z=7-height,Sprite=sprite,Behavior=behavior});
            g.Rooms[g.Player.Room].Entities.Add(e);return e;
        }
        foreach(bool heels in new[]{false,true})foreach(bool bonus in new[]{false,true})
        {
            var g=Empty(heels);var e=Add(g,28,43);g.Player.Height=1;
            if(bonus)g.Player.LongJumps=1;
            g.Jump();int ticks=heels&&!bonus?8:10;
            for(int i=0;i<ticks;i++)Tick(g);
            Near(g.Player.Height,1+ticks/3f,"spring adds exact original jump ascent");
            Check(g.Player.JumpTicks==0,"spring ascent ends at 8 or 10 ticks");
            if(heels&&bonus)Check(g.Player.LongJumps==0,"spring and jump bonus consume one resource");
            Check(e.X==e.Source.X&&e.Y==e.Source.Y,"jump does not displace spring");
        }
        var side=Empty();AddAt(side,28,43,4.25f,3.5f,0);side.Jump();Tick(side);
        Check(side.Player.JumpTicks==7,"side of spring does not increase jump");
        var spring=Empty();var s=Add(spring,28,43);spring.Player.Height=1;Tick(spring);
        Check(s.Sprite==44,"spring compresses under player");
        spring.Player.X=1;
        foreach(int frame in new[]{43,44,43,44,43,43}){Tick(spring);Check(s.Sprite==frame,"original spring release sequence");}

        var lift=Empty(true);var platform=Add(lift,23);lift.Player.Height=1;
        Tick(lift);Check(platform.Behavior==16&&platform.BehaviorState==0x90,"lift initializes original packed state");
        Tick(lift);Tick(lift);Check(platform.BehaviorState==0x98,"lift changes to rising after endpoint dwell");
        Tick(lift);Near(platform.Height,1/6f,"lift moves one raw Z unit per tick");
        Near(lift.Player.Height,7/6f,"lift carries grounded rider");
        float min=platform.Height,max=min;
        for(int i=0;i<130;i++)
        {
            Tick(lift);min=Math.Min(min,platform.Height);max=Math.Max(max,platform.Height);
            Near(lift.Player.Height,platform.Height+1,"rider follows lift in both directions");
        }
        Near(min,0,"lift lower limit");Near(max,9,"lift upper limit");
        var shortLift=Empty();var small=Add(shortLift,18);shortLift.Player.X=1;
        for(int i=0;i<120;i++)Tick(shortLift);
        Check(small.Height>=2-.001f&&small.Height<=5+.001f,"alternate lift has limits two to five");
        var blocked=Empty();var stopped=Add(blocked,23);blocked.Player.Height=1;Add(blocked,0,height:2);
        for(int i=0;i<8;i++)Tick(blocked);
        Near(stopped.Height,0,"lift cannot crush rider against solid object");
        var jumpOff=Empty();platform=Add(jumpOff,23);jumpOff.Player.Height=1;
        for(int i=0;i<10;i++)Tick(jumpOff);
        float old=jumpOff.Player.Height;jumpOff.Jump();Tick(jumpOff);
        Check(jumpOff.Player.Height>old+1/6f+.01f&&!jumpOff.Player.Grounded,"rider can jump from moving lift");

        // Real two-room puzzle: the platform is in 6160; 6150 has the floor opening.
        var route=new NativeGame(data);route.Switch();route.Player.Room=0x6160;
        route.Player.X=6;route.Player.Y=3;route.Player.Height=1;
        var real=route.Rooms[0x6160].Entities.Single(e=>e.Source.Behavior==23);
        for(int i=0;i<100&&real.Height<9-.001f;i++)Tick(route);
        Near(real.Height,9,"actual 6160 lift reaches upper limit");
        Near(route.Player.Height,10,"dog rides to room ceiling");
        route.Jump();Tick(route);
        Check(route.Player.Room==0x6150,"dog jumps from lift into actual room above");
        Near(route.Player.X,6,"vertical ascent preserves X");Near(route.Player.Y,3,"vertical ascent preserves Y");
        Near(route.Player.Height,1,"original upward entry height");
        Tick(route);Check(route.Player.Grounded,"upper room block supports dog after arrival");
        route.Player.X=5;route.Player.Y=3;
        for(int i=0;i<8&&route.Player.Room==0x6150;i++)Tick(route);
        Check(route.Player.Room==0x6160,"dog falls back through upper floor opening");
        Near(route.Player.Height,10,"downward entry height");
        Check(GameWindow.RoomOffset(0,0,-1)==new System.Drawing.PointF(0,-320),"camera places upper level above lower level");

        var sealedRoom=Empty();sealedRoom.Player.Height=10;sealedRoom.Player.Grounded=true;sealedRoom.Jump();Tick(sealedRoom);
        Check(sealedRoom.Player.Room==0x8a40&&sealedRoom.Player.Height==10,"no invented room through solid ceiling");

        var chase=Empty();var enemy=AddAt(chase,33,0,2,2,0);Tick(chase);
        Near(enemy.X,2.125f,"near enemy pursues in diagonal X");Near(enemy.Y,2.125f,"near enemy pursues in diagonal Y");
        var axis=Empty();enemy=AddAt(axis,15,0,1.5f,2.5f,0);Tick(axis);
        Near(enemy.X,1.5f,"type 15 chooses shorter nonzero axis");Near(enemy.Y,2.625f,"type 15 follows Y when closer");
        var tie=Empty();enemy=AddAt(tie,15,0,2,2,0);Tick(tie);
        Near(enemy.X,2.125f,"type 15 tie prefers X");Near(enemy.Y,2,"type 15 tie leaves Y");
        var aligned=Empty();enemy=AddAt(aligned,15,0,2,3.5f,0);Tick(aligned);
        Near(enemy.X,2.125f,"type 15 follows nonzero axis when aligned");
        return checks;
    }
}
