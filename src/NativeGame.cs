namespace HoH26;

// Direct rules in native C#, using original Z80 coordinate increments.
// A logic tick is four PAL frames (01C4/04DB). Rendering is independent.
public sealed partial class NativeGame
{
    public const float TickSeconds=.08f;
    public event Action<SoundCue>? Sound;
    public Dictionary<int,RoomState> Rooms { get; }
    public Character[] Characters = [new(0x8a40,true),new(0x8940,false)];
    public int Active;
    public Character Player => Joined?Characters[1]:Characters[Active];
    public bool Joined { get; private set; }
    public long Ticks { get; private set; }
    public int VisitedCount => visited.Count;
    public string Notice { get; private set; }="";
    public bool HasBag { get; private set; }
    public bool HasHooter { get; private set; }
    public int Ammunition { get; private set; }
    public int CrownMask { get; private set; }
    public bool Won=>CrownMask==31;
    public bool GameOver=>Characters.All(c=>c.Lives==0);
    public Entity? Carried { get; private set; }
    public int TeleportTicks { get; private set; }
    public bool TeleportArriving { get; private set; }
    readonly int[][] teleportPairs;
    int teleportDestination;
    (int room,float x,float y,float height)? checkpoint;
    readonly HashSet<int> visited=[];
    float accumulator;
    bool jumpRequested;
    bool walkingSound;
    void SetWalkingSound(bool walking)
    {
        if(walkingSound&&!walking)Sound?.Invoke(SoundCue.StopStep);
        walkingSound=walking;
    }
    int damageMask;
    static readonly (int x,int y)[] vectors=[(-1,0),(-1,-1),(0,-1),(1,-1),(1,0),(1,1),(0,1),(-1,1)];
    public NativeGame(WorldData data)
    {
        teleportPairs=data.TeleportPairs;
        Rooms=data.Rooms.ToDictionary(r=>r.Id,r=>new RoomState(r));
        foreach(var pickup in data.Pickups)Rooms[pickup.RoomId].Pickups.Add(new(pickup));
        foreach(var p in Characters)p.Lives=data.InitialLives;
        foreach(var p in Characters)visited.Add(p.Room);
    }
    public void Switch()
    {
        if(TeleportTicks>0||Player.DamageTicks>0)return;
        Sound?.Invoke(SoundCue.Switch);
        var h=Characters[0];var f=Characters[1];
        if(Joined){Joined=false;Active=0;Notice="Head y Heels separados";return;}
        if(h.Room==f.Room&&Math.Abs(h.X-f.X)<=.1251f&&Math.Abs(h.Y-f.Y)<=.1251f&&Math.Abs(h.Height-f.Height-1)<.001f)
        {Joined=true;Active=1;SyncJoined();Notice="Head sobre Heels";return;}
        Active^=1;Notice="";
    }
    void SyncJoined()
    {
        var head=Characters[0];var heels=Characters[1];
        head.Room=heels.Room;head.X=heels.X;head.Y=heels.Y;head.Height=heels.Height+1;
        head.Direction=heels.Direction;head.Flip=heels.Flip;head.Frame=heels.Frame;
    }
    void RememberEntry(Character p)
    {
        p.Entry=(p.Room,p.X,p.Y,p.Height);
        if(Joined)Characters[0].Entry=(p.Room,p.X,p.Y,p.Height+1);
    }
    void SyncTeleportVisual(Character p)
    {
        p.TeleportTicks=TeleportTicks;p.TeleportArriving=TeleportArriving;
        if(Joined){Characters[0].TeleportTicks=TeleportTicks;Characters[0].TeleportArriving=TeleportArriving;}
    }
    public void Jump()=>jumpRequested=true;
    public void Fire()
    {
        var p=Player;
        if(!p.IsHead||Joined){Notice="La bocina la usa Head por separado";return;}
        if(TeleportTicks>0)return;
        if(!HasHooter||Ammunition==0){Notice="Head necesita bocina y munición";return;}
        if(p.DamageTicks>0||Rooms.Values.Any(r=>r.Projectiles.Count>0))return;
        Rooms[p.Room].Projectiles.Add(new(p.X,p.Y,p.Height,p.Direction));
        Ammunition--;Notice="";Sound?.Invoke(SoundCue.Fire);
    }
    public void Step(float dt,float dx,float dy)
    {
        accumulator+=Math.Clamp(dt,0,.25f);
        while(accumulator+0.000001f>=TickSeconds)
        {accumulator-=TickSeconds;Tick(dx,dy);}
    }
    void Tick(float dx,float dy)
    {
        pushedThisTick.Clear();
        Ticks++;var p=Player;
        foreach(var r in Rooms.Values)
        {
            foreach(var effect in r.Effects)effect.Frame++;
            r.Effects.RemoveAll(e=>e.Finished);
        }
        if(Ticks%3==0)
        {
            if(p.Protection>0)p.Protection--;
            if(Joined&&Characters[0].Protection>0)Characters[0].Protection--;
        }
        if(p.DamageTicks>0)
        {
            SetWalkingSound(false);
            p.DamageTicks--;
            if(Joined)Characters[0].DamageTicks=p.DamageTicks;
            if(p.DamageTicks==0)Respawn(p);
            jumpRequested=false;return;
        }
        if(TeleportTicks>0)
        {
            SetWalkingSound(false);
            if(--TeleportTicks==0&&!TeleportArriving)
            {
                p.Room=teleportDestination;visited.Add(p.Room);TeleportArriving=true;TeleportTicks=8;
                if(Joined)SyncJoined();
                RememberEntry(p);
                Sound?.Invoke(SoundCue.TeleportArrive);
            }
            SyncTeleportVisual(p);
            jumpRequested=false;return;
        }
        foreach(var r in Rooms.Values)
        {
            foreach(var e in r.Entities)
                if(!e.Removed&&!e.Frozen&&e.Source.Animation.Length>1)e.AnimationFrame++;
            foreach(var pickup in r.Pickups)if(!pickup.Collected)pickup.Frame++;
        }
        if(p.Lives==0){SetWalkingSound(false);jumpRequested=false;return;}
        var room=Rooms[p.Room];
        StepObjects(room);
        StepPickups(room);
        StepProjectiles(room);
        bool launched=false;
        if(jumpRequested&&p.Grounded)
        {
            if(TryTeleport(room,p)){SetWalkingSound(false);jumpRequested=false;return;}
            Sound?.Invoke(SoundCue.Jump);
            launched=true;
            p.JumpFlight=true;
            // 2A41–2A6F: spring adds one level; Head/bonus adds another.
            // The combined 12-tick result is explicitly capped at ten.
            int boost=room.Entities.Any(e=>IsSolid(e)&&(e.Sprite&127) is 43 or 44&&
                OverlapXY(e,p.X,p.Y)&&Math.Abs(p.Height-e.Height-e.Source.Collider.Height)<.001f)?1:0;
            if(p.IsHead||Joined)boost++;
            else if(p.LongJumps>0){boost++;p.LongJumps--;}
            p.JumpTicks=Math.Min(10,4+4*boost);
            p.Grounded=false;
        }
        jumpRequested=false;
        int direction=Array.FindIndex(vectors,v=>v.x==Math.Sign(dx)&&v.y==Math.Sign(dy));
        bool walked=false;
        if(direction>=0)
        {
            if(direction!=p.PreviousDirection)p.TurnDelay=2;
            p.Direction=direction;p.Flip=(direction&2)==0;
            int steps=1;
            if(p.TurnDelay>0)p.TurnDelay--;else if(!p.IsHead||Joined||p.FastSteps>0)steps=2;
            var v=vectors[direction];
            for(int s=0;s<steps;s++)
                if(TryMove(room,p,v.x/8f,v.y/8f)) {
                    p.Frame=(p.Frame+1)%4;
                    if(s==0&&p.Grounded){Sound?.Invoke(SoundCue.Step);walked=true;}
                }
            p.PreviousDirection=direction;
            if(p.IsHead&&p.FastSteps>0)p.FastSteps--;
        }
        else {p.PreviousDirection=-1;p.TurnDelay=2;}
        SetWalkingSound(walked);
        Vertical(room,p,launched);
        Collect(room,p);
        if(room.Entities.Any(e=>!e.Removed&&e.Dangerous&&OverlapXY(e,p.X,p.Y)&&p.Height<e.Height+e.Source.Collider.Height&&p.Height+BodyHeight>e.Height))
            Damage(p);
        CheckBoundary(p);
        CheckVerticalBoundary(p);
        if(Joined)SyncJoined();
    }
    bool TryTeleport(RoomState room,Character p)
    {
        if(!room.Entities.Any(e=>!e.Removed&&(e.Sprite&127)==87&&OverlapXY(e,p.X,p.Y)&&
            Math.Abs(p.Height-e.Height-e.Source.Collider.Height)<.001f))return false;
        // 3479 searches in order. Repeated rooms intentionally route to the
        // first matching pair, so a dictionary would change original routing.
        foreach(var pair in teleportPairs)
        {
            int target=pair[0]==p.Room?pair[1]:pair[1]==p.Room?pair[0]:-1;
            if(target<0)continue;
            if(!Rooms.ContainsKey(target))throw new InvalidDataException("Unknown teleport target");
            teleportDestination=target;TeleportArriving=false;TeleportTicks=12;
            SyncTeleportVisual(p);Sound?.Invoke(SoundCue.TeleportDepart);
            Notice="Teletransporte";return true;
        }
        return false;
    }
    void StepProjectiles(RoomState room)
    {
        foreach(var shot in room.Projectiles.ToArray())
        {
            if(shot.BurstFrame>=0)
            {
                if(++shot.BurstFrame==4)room.Projectiles.Remove(shot);
                continue;
            }
            shot.Frame++;
            var v=vectors[shot.Direction];
            // 4C76 calls 4CA7 twice; each call attempts two 4452 steps.
            for(int step=0;step<4;step++)
            {
                float x=shot.X+v.x/8f,y=shot.Y+v.y/8f;
                var hit=room.Entities.FirstOrDefault(e=>!e.Removed&&OverlapXY(e,x,y,.375f)&&
                    shot.Height<e.Height+e.Source.Collider.Height&&shot.Height+1>e.Height);
                var bounds=room.Data.Bounds;
                bool wall=x<(bounds[0]-12)/8f||x>(bounds[2]-12)/8f||y<(bounds[1]-12)/8f||y>(bounds[3]-12)/8f;
                if(hit is not null||wall)
                {
                    if(hit is not null&&(hit.Source.Flags&0x60)==0x60)hit.Frozen=true;
                    shot.BurstFrame=0;break;
                }
                shot.X=x;shot.Y=y;
            }
        }
    }
    void Damage(Character p)
    {
        if(p.DamageTicks>0)return;
        damageMask=Joined?3:p.IsHead?2:1;
        if(Characters[1].Protection>0)damageMask&=~1;
        if(Characters[0].Protection>0)damageMask&=~2;
        if(damageMask==0)return;
        Sound?.Invoke(SoundCue.Damage);p.DamageTicks=12;Notice="Has perdido una vida";
        if(Joined)Characters[0].DamageTicks=12;
    }
    void Respawn(Character p)
    {
        bool wasJoined=Joined;
        if((damageMask&1)!=0)Characters[1].Lives=Math.Max(0,Characters[1].Lives-1);
        if((damageMask&2)!=0)Characters[0].Lives=Math.Max(0,Characters[0].Lives-1);
        damageMask=0;p.JumpTicks=0;p.Height=0;p.Grounded=true;p.JumpFlight=false;
        if(Joined)Joined=false;
        if(checkpoint is {} saved){p.Room=saved.room;p.X=saved.x;p.Y=saved.y;p.Height=saved.height;}
        else {var entry=p.Entry;p.Room=entry.room;p.X=entry.x;p.Y=entry.y;p.Height=entry.height;}
        p.Grounded=HasFloor(Rooms[p.Room].Data)&&p.Height==0;
        p.Protection=12;
        if(wasJoined)
        {
            var head=Characters[0];
            var entry=head.Entry;
            head.Room=p.Room;head.X=p.X;head.Y=p.Y;head.Height=p.Height+1;
            if(checkpoint is null){head.Room=entry.room;head.X=entry.x;head.Y=entry.y;head.Height=entry.height;}
            head.JumpTicks=0;head.DamageTicks=0;head.Grounded=false;head.Protection=12;
        }
        if(p.Lives==0){Active=Characters[0].Lives>0?0:1;Notice=GameOver?"Fin de la partida":"Cambia al otro personaje";}
    }
    public void Carry()
    {
        var p=Player;
        if(TeleportTicks>0||p.DamageTicks>0)return;
        if(!HasBag){Notice="Heels necesita la bolsa";return;}
        if(p.IsHead&&!Joined){Notice="La bolsa pertenece a Heels";return;}
        var room=Rooms[p.Room];
        if(Carried is {} carried)
        {
            float height=p.Height-1;
            if(height<0||Blocked(room,p.X,p.Y,height,carried)){Notice="No hay espacio para dejar el objeto";return;}
            carried.X=p.X;carried.Y=p.Y;carried.Height=height;carried.Removed=false;
            if(!room.Entities.Contains(carried))room.Entities.Add(carried);
            Sound?.Invoke(SoundCue.Carry);Carried=null;Notice="Objeto dejado";return;
        }
        var item=room.Entities.FirstOrDefault(e=>!e.Removed&&e.Carryable&&OverlapXY(e,p.X,p.Y)&&
            p.Height-e.Height>=1-.0001f&&p.Height-e.Height<=7/6f+.0001f);
        if(item is null){Notice="Sitúate sobre un objeto transportable";return;}
        Sound?.Invoke(SoundCue.Carry);item.Removed=true;room.Entities.Remove(item);Carried=item;Notice="Objeto en la bolsa";
    }
    static void AddPickupEffect(RoomState room,float x,float y,float height,int[] animation,int sprite,int start,bool flip=false)
    {
        // 4773/478C: pickup disappearance retains its normal animation for
        // eight updates. No synthetic burst or replacement artwork is used.
        int[] frames=Enumerable.Range(0,8).Select(i=>
            (animation.Length==0?sprite:animation[(start+i)%animation.Length])^(flip?128:0)).ToArray();
        room.Effects.Add(new(x,y,height,frames));
    }
    void Collect(RoomState room,Character p)
    {
        foreach(var pickup in room.Pickups)
        {
            var d=pickup.Data;
            if(pickup.Collected||!d.Collectible||Math.Abs(p.X-d.X)>=.75f||Math.Abs(p.Y-d.Y)>=.75f||
                p.Height>pickup.Height+1+.0001f||p.Height+BodyHeight<pickup.Height-.0001f)continue;
            // 3FA5 grants the shared bag/hooter flags without a character check.
            // Ammunition and speed bonus have the separate 3FB4/3FC3 restriction.
            if((d.Kind==2||d.Kind==3)&&!p.IsHead&&!Joined)continue;
            if(d.Kind==4&&p.IsHead&&!Joined)continue;
            switch(d.Kind)
            {
                case 0:HasBag=true;Notice="Bolsa recogida: la usa Heels con E";break;
                case 1:HasHooter=true;Notice="Bocina recogida: Head dispara con F y munición";break;
                case 2:Ammunition=Math.Min(99,Ammunition+6);Notice="6 disparos";break;
                case 3:Characters[0].FastSteps=99;Notice="Pasos rápidos";break;
                case 4:Characters[1].LongJumps=Math.Min(99,Characters[1].LongJumps+10);Notice="Saltos largos";break;
                case 5:p.Protection=99;if(Joined)Characters[0].Protection=99;Notice="Protección";break;
                case 6:p.Lives=Math.Min(99,p.Lives+2);if(Joined)Characters[0].Lives=Math.Min(99,Characters[0].Lives+2);Notice="Dos vidas adicionales";break;
                case 8:checkpoint=(p.Room,p.X,p.Y,p.Height);Notice="Punto de retorno";break;
                case >=9 and <=13:CrownMask|=1<<(d.Kind-9);Notice=Won?"Las cinco coronas recuperadas":"Corona recuperada";break;
                default:continue;
            }
            AddPickupEffect(room,d.X,d.Y,pickup.Height,d.Animation,pickup.Sprite,pickup.Frame);
            pickup.Collected=true;Sound?.Invoke(d.Kind is 0 or 1 or 2?SoundCue.PickupEquipment:SoundCue.Pickup);
        }
    }
    bool IsSolid(Entity e)=>!e.Removed;
    static bool OverlapXY(Entity e,float x,float y,float radius=.375f)
    {
        var c=e.Source.Collider;
        return x+radius>e.X+c.MinX+.0001f&&x-radius<e.X+c.MaxX-.0001f&&
               y+radius>e.Y+c.MinY+.0001f&&y-radius<e.Y+c.MaxY-.0001f;
    }
    float BodyHeight=>Joined?2:1;
    bool Blocked(RoomState room,float x,float y,float z,Entity? ignore=null)
    {
        return room.Entities.Any(e=>e!=ignore&&IsSolid(e)&&OverlapXY(e,x,y)&&
            z<e.Height+e.Source.Collider.Height-.0001f&&z+BodyHeight>e.Height+.0001f);
    }
    bool TryMove(RoomState room,Character p,float dx,float dy)
    {
        float x=p.X+dx,y=p.Y+dy;
        // Wall contacts allow movement only through an actual doorway aperture.
        var b=room.Data.Bounds;
        float minX=(b[0]-9)/8f,maxX=(b[2]-15)/8f,minY=(b[1]-9)/8f,maxY=(b[3]-15)/8f;
        if(x<minX&&!Doorway(room.Data,p,3,y))return false;
        if(x>maxX&&!Doorway(room.Data,p,1,y))return false;
        if(y<minY&&!Doorway(room.Data,p,2,x))return false;
        if(y>maxY&&!Doorway(room.Data,p,0,x))return false;
        if(Blocked(room,x,y,p.Height))
        {
            if(room.Entities.Any(e=>!e.Removed&&e.Dangerous&&OverlapXY(e,x,y)&&p.Height<e.Height+e.Source.Collider.Height&&p.Height+BodyHeight>e.Height))Damage(p);
            if(!TryPush(room,p,dx,dy))return false;
        }
        p.X=x;p.Y=y;return true;
    }
    static bool Doorway(RoomData r,Character p,int side,float along)
    {
        int code=r.Exits[side];
        if(code==1)return true;
        return code>=2&&Math.Abs(along-3.5f)<=.25f&&Math.Abs(p.Height-(code-2))<=1/6f;
    }
    void Vertical(RoomState room,Character p,bool launched)
    {
        if(p.JumpTicks>0)
        {
            // 27BB requests 84 during ascent. The launch tick keeps 85,
            // whose original per-tick priority is higher (2B21).
            if(!launched)
                Sound?.Invoke(SoundCue.Rise);
            p.JumpTicks--;
            for(int i=0;i<2;i++)
                if(!Blocked(room,p.X,p.Y,p.Height+1/6f))p.Height+=1/6f;
                else {p.JumpTicks=0;break;}
            p.Grounded=false;return;
        }
        bool wasGrounded=p.Grounded;
        float support=HasFloor(room.Data)?0:float.NegativeInfinity;
        foreach(var e in room.Entities.Where(IsSolid))
            if(OverlapXY(e,p.X,p.Y)&&e.Height+e.Source.Collider.Height<=p.Height+.0001f)
                support=Math.Max(support,e.Height+e.Source.Collider.Height);
        var other=Characters[Active^1];
        if(!Joined&&other.Room==p.Room&&Math.Abs(p.X-other.X)<.75f&&Math.Abs(p.Y-other.Y)<.75f&&other.Height+1<=p.Height+.0001f)
            support=Math.Max(support,other.Height+1);
        p.Height=Math.Max(support,p.Height-((p.IsHead||Joined)?1:2)/6f);
        p.Grounded=Math.Abs(p.Height-support)<.0001f;
        if(!p.Grounded)
            Sound?.Invoke(!p.IsHead&&!Joined&&p.JumpFlight?SoundCue.FallHeels:SoundCue.Fall);
        if(p.Grounded&&!wasGrounded)Sound?.Invoke(SoundCue.Land);
        if(p.Grounded)p.JumpFlight=false;
        // 2CAA/2CB7: floor 6 supports the player at Z=C0 and kills on contact.
        // Floor 7 is the opening that continues into the room below.
        if(p.Grounded&&support==0&&room.Data.Floor==6)Damage(p);
        if(p.Grounded&&room.Entities.Any(e=>e.Dangerous&&!e.Removed&&OverlapXY(e,p.X,p.Y)&&Math.Abs(e.Height+e.Source.Collider.Height-support)<.001f))Damage(p);
    }
    void CheckBoundary(Character p)
    {
        var r=Rooms[p.Room].Data;var b=r.Bounds;
        if(p.X<(b[0]-12)/8f)Transition(p,-1,0,3);
        else if(p.X>(b[2]-12)/8f)Transition(p,1,0,1);
        else if(p.Y<(b[1]-12)/8f)Transition(p,0,-1,2);
        else if(p.Y>(b[3]-12)/8f)Transition(p,0,1,0);
    }
    void Transition(Character p,int dx,int dy,int side)
    {
        var r=Rooms[p.Room].Data;
        // 018D changes the high XY nibble for physical X, low nibble for Y.
        int id=((((r.GridY+dx)&15)*16+((r.GridX+dy)&15))<<8)|(r.Level<<4);
        if(!Rooms.TryGetValue(id,out var dest)||r.Exits[side]==0)return;
        p.Room=id;var b=dest.Data.Bounds;
        if(r.Exits[side]>=2)
        {
            p.Height=dest.Data.Exits[(side+2)%4]-2;
            p.JumpTicks=0;p.Grounded=true;p.JumpFlight=false;
        }
        if(dx<0)p.X=(b[2]-15)/8f;if(dx>0)p.X=(b[0]-9)/8f;
        if(dy<0)p.Y=(b[3]-15)/8f;if(dy>0)p.Y=(b[1]-9)/8f;
        visited.Add(id);Notice="";
        RememberEntry(p);
    }
}
