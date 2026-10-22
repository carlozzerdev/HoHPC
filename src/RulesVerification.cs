namespace HoH26;

// Golden values below come from Z80 tables documented in CHARACTERS-Z80.md,
// rather than duplicating implementation formulas in expected results.
static class RulesVerification
{
    static int checks;
    static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    static void Near(float actual,float expected,string message)=>Check(Math.Abs(actual-expected)<.001f,$"{message}: {actual} != {expected}");
    static WorldData EmptyWorld()=>new(){Rooms=[
        new(){Id=0x8a40,GridX=10,GridY=8,Level=4,Bounds=[8,8,72,72],Exits=[0,0,0,0]},
        new(){Id=0x8940,GridX=9,GridY=8,Level=4,Bounds=[8,8,72,72],Exits=[0,0,0,0]}]};
    static void Ticks(NativeGame g,int n,float x=0,float y=0){for(int i=0;i<n;i++)g.Step(.08f,x,y);}
    public static int Run()
    {
        checks=0;
        foreach(var input in new[]{(h:0f,v:-1f,sx:2f,sy:-1f),(h:0f,v:1f,sx:-2f,sy:1f),
                                   (h:-1f,v:0f,sx:-2f,sy:-1f),(h:1f,v:0f,sx:2f,sy:1f)}) {
            var walking=new NativeGame(EmptyWorld());var motion=GameWindow.WorldMotion(input.h,input.v);
            float x=walking.Player.X,y=walking.Player.Y;
            Ticks(walking,1,motion.dx,motion.dy);
            float dx=walking.Player.X-x,dy=walking.Player.Y-y;
            Near((dx-dy)*16,input.sx,"Keyboard: projected horizontal displacement");
            Near(-(dx+dy)*8,input.sy,"Keyboard: projected vertical displacement");
        }
        // Regressions from the stacked steps and teleporter screenshots.
        var lower=Volume.Body(0,6,0,.5f,1);
        var dog=Volume.Body(0,6,1,.375f,1);
        var upper=Volume.Body(0,6,2,.5f,1);
        var rabbit=Volume.Body(0,6,7,.375f,1);
        var stack=DepthOrder.Sort([rabbit,upper,dog,lower]);
        Check(stack.SequenceEqual(new[]{3,2,1,0}),"Stack: lower step, dog, upper step, rabbit");
        var platform=Volume.Body(7,6,0,.5f,1);
        var rider=Volume.Body(6.8f,5.8f,1,.375f,1);
        Check(DepthOrder.Sort([rider,platform]).SequenceEqual(new[]{1,0}),"Offset teleporter rider drawn after supporting surface");
        var front=Volume.Body(2,2,0,.5f,1);
        var back=Volume.Body(3,2,0,.5f,1);
        Check(DepthOrder.Sort([front,back]).SequenceEqual(new[]{1,0}),"Ground bodies retain back to front order");
        Check(DepthOrder.Sort([upper,lower]).SequenceEqual(new[]{1,0}),"Stack independent of insertion order");
        var audible=new NativeGame(EmptyWorld());var cues=new List<SoundCue>();
        audible.Sound+=cues.Add;
        audible.Jump();Ticks(audible,1);
        Check(cues.Count(c=>c==SoundCue.Jump)==1,"Accepted jump emits one sound");
        audible.Jump();Ticks(audible,1);
        Check(cues.Count(c=>c==SoundCue.Jump)==1,"Airborne jump request emits no sound");
        Ticks(audible,30);
        Check(cues.Count(c=>c==SoundCue.Land)==1,"Landing emits one sound");
        Ticks(audible,4);
        Check(cues.Count(c=>c==SoundCue.Land)==1,"Standing emits no repeated landing");
        audible.Fire();Check(!cues.Contains(SoundCue.Fire),"Denied shot emits no firing sound");
        Ticks(audible,4,1,0);Check(cues.Contains(SoundCue.Step),"Successful ground movement emits footsteps");
        audible.Switch();Check(cues.Contains(SoundCue.Switch),"Character switch emits sound");
        var head=new NativeGame(EmptyWorld());Ticks(head,10,1,0);
        Near(head.Player.X,4.75f,"Head: ten original steps");
        var heels=new NativeGame(EmptyWorld());heels.Switch();Ticks(heels,10,1,0);
        Near(heels.Player.X,5.75f,"Heels: double steps after direction delay");
        var jump=new NativeGame(EmptyWorld());jump.Jump();Ticks(jump,8);
        Near(jump.Player.Height,16/6f,"Head: eight ascent ticks");Ticks(jump,16);
        Near(jump.Player.Height,0,"Head: landing");
        jump.Switch();jump.Jump();Ticks(jump,4);Near(jump.Player.Height,8/6f,"Heels: four ascent ticks");
        Ticks(jump,4);Near(jump.Player.Height,0,"Heels: landing");
        var split=new NativeGame(EmptyWorld());var whole=new NativeGame(EmptyWorld());
        for(int i=0;i<20;i++)split.Step(.04f,1,0);Ticks(whole,10,1,0);
        Near(split.Player.X,whole.Player.X,"Logic independent of rendering frequency");
        var joined=new NativeGame(EmptyWorld());
        joined.Characters[0].Room=joined.Characters[1].Room;joined.Characters[0].Height=1;joined.Switch();
        Check(joined.Joined,"Exact Head-on-Heels join");joined.Jump();Ticks(joined,8);
        Near(joined.Characters[1].Height,16/6f,"Combined high jump");
        Near(joined.Characters[0].Height,22/6f,"Joined upper body stays one unit higher");
        joined.Switch();Check(!joined.Joined,"Separate combined characters");
        var itemWorld=EmptyWorld();
        itemWorld.Pickups.Add(new(){Id=0,RoomId=0x8a40,X=3.5f,Y=3.5f,Z=7,Kind=6,Collectible=true});
        var items=new NativeGame(itemWorld);Ticks(items,1);Check(items.Player.Lives==6,"Original life pickup adds two");
        Ticks(items,5);Check(items.Player.Lives==6,"Collected item is not awarded twice");
        var dangerous=new Entity(new(){Flags=0x6c});Check(dangerous.Dangerous&&!dangerous.Carryable,"Raw 6C means hazard after runtime flag conversion");
        Check(new Entity(new(){Flags=0x40}).Carryable,"Raw 40 remains carryable");
        var world=EmptyWorld();var bag=new PickupData{RoomId=0x8940,X=3.5f,Y=3.5f,Z=7,Kind=0,Collectible=true};world.Pickups.Add(bag);
        var carrier=new NativeGame(world);carrier.Switch();Ticks(carrier,1);Check(carrier.HasBag,"Heels acquires bag");
        var entity=new Entity(new(){Flags=0x40,X=3.5f,Y=3.5f,Z=7});
        carrier.Rooms[0x8940].Entities.Add(entity);carrier.Player.Height=1;carrier.Carry();
        Check(carrier.Carried==entity&&!carrier.Rooms[0x8940].Entities.Contains(entity),"Carry removes exactly one room body");
        carrier.Carry();Check(carrier.Carried==null&&carrier.Rooms[0x8940].Entities.Count==1,"Drop restores exactly one body");
        var random=new OriginalRandom();
        ushort[] expected=[0x4dcf,0xe9c9,0xbc2c,0x35ce,0xa14e,0xe478,0xac9f,0x05dd,0x1082,0x3157];
        foreach(var value in expected)Check(random.NextUInt16()==value,"Original PRNG register vector");
        var shotWorld=EmptyWorld();
        shotWorld.Pickups.Add(new(){RoomId=0x8a40,X=3.5f,Y=3.5f,Z=7,Kind=1,Collectible=true});
        shotWorld.Pickups.Add(new(){RoomId=0x8a40,X=3.5f,Y=3.5f,Z=7,Kind=2,Collectible=true});
        var shooting=new NativeGame(shotWorld);Ticks(shooting,1);shooting.Player.Direction=4;
        Check(shooting.HasHooter&&shooting.Ammunition==6,"Head obtains hooter and six shots");
        shooting.Fire();shooting.Fire();
        Check(shooting.Ammunition==5&&shooting.Rooms[0x8a40].Projectiles.Count==1,"Single projectile template consumes one ammunition");
        Ticks(shooting,1);
        Near(shooting.Rooms[0x8a40].Projectiles[0].X,4,"Projectile: four raw X steps per tick");
        var target=new Entity(new(){X=5,Y=3.5f,Z=7,Flags=0x60});
        shooting.Rooms[0x8a40].Entities.Add(target);Ticks(shooting,2);
        Check(target.Frozen&&!target.Removed,"Hooter freezes eligible enemy rather than deleting it");
        Ticks(shooting,4);Check(shooting.Rooms[0x8a40].Projectiles.Count==0,"Impact animation releases projectile template");
        shooting.Switch();shooting.Fire();Check(shooting.Ammunition==5,"Heels cannot use Head's hooter");
        var hazardWorld=EmptyWorld();
        hazardWorld.Rooms[0].Objects.Add(new(){X=4.5f,Y=3.5f,Z=7,Flags=0x60});
        var harm=new NativeGame(hazardWorld);Ticks(harm,20,1,0);
        Check(harm.Characters[0].Lives==3,"Solid hazard contact resolves exactly one life deduction");
        var actorWorld=EmptyWorld();actorWorld.Rooms[0].Objects.Add(new(){X=1,Y=1,Z=7,Behavior=6});
        var actors=new NativeGame(actorWorld);Ticks(actors,1);
        Near(actors.Rooms[0x8a40].Entities[0].X,.875f,"Reverse-direction family moves one original unit");
        actors.Rooms[0x8a40].Entities[0].Frozen=true;Ticks(actors,3);
        Near(actors.Rooms[0x8a40].Entities[0].X,.875f,"Frozen behavior does not update position");
        var doorWorld=EmptyWorld();doorWorld.Rooms[0].Exits[2]=2;doorWorld.Rooms[1].Exits[0]=2;
        var doorway=new NativeGame(doorWorld);doorway.Player.Y=0;Ticks(doorway,6,0,-1);
        Check(doorway.Player.Room==0x8940,"Original physical Y exit changes low XY room nibble");
        Check(doorway.Characters[1].Room==0x8940,"Room transition preserves other character context");
        var teleportWorld=EmptyWorld();teleportWorld.TeleportPairs=[[0x8a40,0x8940]];
        teleportWorld.Rooms[0].Objects.Add(new(){X=3.5f,Y=3.5f,Z=7,Sprite=87});
        var teleport=new NativeGame(teleportWorld);teleport.Player.Height=1;teleport.Jump();Ticks(teleport,1);
        Check(teleport.TeleportTicks==12,"Jump on teleporter starts original departure counter");
        Ticks(teleport,12);Check(teleport.Player.Room==0x8940&&teleport.TeleportTicks==8,"Teleporter uses ordered room pair and arrival counter");
        Near(teleport.Player.X,3.5f,"Teleport preserves original X coordinate");
        Near(teleport.Player.Height,1,"Teleport preserves original Z coordinate");
        return checks;
    }
}
