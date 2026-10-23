using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Text.Json;

namespace HoH26;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            if(args.Contains("--verify-audio-timing"))
            {
                using var audio=new GameAudio { Muted=true };
                Thread.Sleep(400);
                long initial=audio.PlayedSamples;var stopwatch=Stopwatch.StartNew();
                Thread.Sleep(3000);
                double elapsed=stopwatch.Elapsed.TotalSeconds;
                long samples=audio.PlayedSamples-initial;
                bool passed=audio.Error is null&&Math.Abs(samples/elapsed-22050)/22050<.05;
                var report=JsonSerializer.Serialize(new {passed,samples,elapsedSeconds=elapsed,samplesPerSecond=samples/elapsed,expectedRate=22050,error=audio.Error},new JsonSerializerOptions{WriteIndented=true});
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"audio-timing.json"),report);
                if(audio.Error is not null)throw new Exception(audio.Error);
                if(!passed)throw new Exception("Audio output speed differs from the expected sample rate by more than 5%");
                return;
            }
            if(args.Contains("--verify-audio")) {
                using var audio=new GameAudio();
                Thread.Sleep(150);
                foreach(var cue in Enum.GetValues<SoundCue>().Where(c=>GameAudio.Command(c)>=0)) { audio.Play(cue);Thread.Sleep(750); }
                if(audio.Error is not null)throw new Exception(audio.Error);
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"audio-verification.txt"),"Windows waveOut: opened and submitted the sixteen mapped CPC effects. Auditory equivalence not validated.");
                return;
            }
            var data=WorldData.Load();
            if(args.Contains("--verify")){Verify(data);return;}
            Application.Run(new GameWindow(data));
        }
        catch(Exception e)
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"error.txt"),e.ToString());
            if(!args.Contains("--verify"))MessageBox.Show(e.Message,"Head over Heels PC");
            Environment.ExitCode=1;
        }
    }
    static void Verify(WorldData data)
    {
        int rules=RulesVerification.Run();
        int pushes=PushVerification.Run(data);
        int mechanics=MechanicsVerification.Run(data);
        int pickups=PickupVerification.Run(data);
        int interactions=InteractionVerification.Run();
        int movementAudio=MovementAudioVerification.Run(data);
        int teleportAnimations=TeleportVerification.Run(data);
        int continuousRooms=ConnectedRoomVerification.Run(data);
        int doorLinks=VerifyTopology(data);
        var game=new NativeGame(data);
        if(game.Rooms.Count!=301||data.Sprites.Count!=100)throw new Exception("Unexpected asset inventory");
        if(!game.Rooms.ContainsKey(0x8940)||!game.Rooms.ContainsKey(0x8a40))throw new Exception("Starting rooms absent");
        var other=game.Characters[1];float oldX=other.X;
        game.Jump();for(int i=0;i<180;i++)game.Step(1/60f,0,0);
        if(!game.Player.Grounded||game.Player.Height<0||other.X!=oldX)throw new Exception("Native simulation failed");
        var dir=Path.Combine(AppContext.BaseDirectory,"verification");Directory.CreateDirectory(dir);
        using var renderer=new Renderer(data);
        using(var longRoom=renderer.ConnectedRoom(0x4260,game.Rooms,new RoomConnections(data.Rooms),[]))longRoom.Save(Path.Combine(dir,"room-4260-complete.png"));
        VisualVerification.Run(data,renderer,dir);
        using(var atlas=renderer.Atlas(data))atlas.Save(Path.Combine(dir,"sprites.png"));
        using(var room=renderer.Room(game.Rooms[0x8a40],game.Characters.Where(c=>c.Room==0x8a40)))room.Save(Path.Combine(dir,"head-start.png"));
        using(var room=renderer.Room(game.Rooms[0x8940],game.Characters.Where(c=>c.Room==0x8940)))room.Save(Path.Combine(dir,"heels-start.png"));
        // Reproduce the reported support/occlusion scenes without changing gameplay.
        var stairsPeople=new[]{new Character(0x8a40,false){X=0,Y=6,Height=1},new Character(0x8a40,true){X=0,Y=6,Height=7}};
        using(var room=renderer.Room(game.Rooms[0x8a40],stairsPeople))room.Save(Path.Combine(dir,"depth-stairs.png"));
        var rider=new Character(0x8a40,false){X=7,Y=6,Height=1};
        using(var room=renderer.Room(game.Rooms[0x8a40],[rider]))room.Save(Path.Combine(dir,"depth-teleporter.png"));
        // Actual spring room from the supplied disk (the user's reported scene).
        var pushing=new NativeGame(data);pushing.Player.Room=0x0480;
        pushing.Player.X=3.25f;pushing.Player.Y=4;
        using(var room=renderer.Room(pushing.Rooms[0x0480],[pushing.Player]))room.Save(Path.Combine(dir,"push-spring-before.png"));
        for(int i=0;i<8;i++)pushing.Step(NativeGame.TickSeconds,1,0);
        var spring=pushing.Rooms[0x0480].Entities.Single(e=>e.Source.Type==1);
        if(Math.Abs(spring.X-5)>.001f||spring.Source.X!=4)throw new Exception("Recovered spring room push failed");
        using(var room=renderer.Room(pushing.Rooms[0x0480],[pushing.Player]))room.Save(Path.Combine(dir,"push-spring-after.png"));
        var collecting=new NativeGame(data);collecting.Player.Room=0x3060;
        collecting.Player.X=2;collecting.Player.Y=0;
        for(int i=0;i<42;i++)collecting.Step(NativeGame.TickSeconds,0,0);
        collecting.Player.X=3;collecting.Player.Height=3;
        using(var room=renderer.Room(collecting.Rooms[0x3060],[collecting.Player],tall:true))room.Save(Path.Combine(dir,"hooter-before.png"));
        collecting.Player.X=4;collecting.Step(NativeGame.TickSeconds,0,0);
        using(var room=renderer.Room(collecting.Rooms[0x3060],[collecting.Player],tall:true))room.Save(Path.Combine(dir,"hooter-collected.png"));
        var riding=new NativeGame(data);riding.Switch();riding.Player.Room=0x6160;
        riding.Player.X=6;riding.Player.Y=3;riding.Player.Height=1;
        var lift=riding.Rooms[0x6160].Entities.Single(e=>e.Source.Behavior==23);
        for(int i=0;i<100&&lift.Height<9-.001f;i++)riding.Step(NativeGame.TickSeconds,0,0);
        using(var room=renderer.Room(riding.Rooms[0x6160],[riding.Player],tall:true))room.Save(Path.Combine(dir,"lift-6160-top.png"));
        riding.Jump();riding.Step(NativeGame.TickSeconds,0,0);
        using(var room=renderer.Room(riding.Rooms[0x6150],[riding.Player],tall:true))room.Save(Path.Combine(dir,"lift-6150-arrival.png"));
        // Exercise every decoded room and sprite, not only the starting scene.
        foreach(var r in game.Rooms.Values){using var image=renderer.Room(r);}
        using var window=new GameWindow(data);window.ClientSize=new Size(1200,800);window.CreateControl();
        using var screenshot=new Bitmap(1200,800);window.DrawToBitmap(screenshot,new Rectangle(0,0,1200,800));
        screenshot.Save(Path.Combine(dir,"window.png"));
        window.VerifyCamera();
        window.DrawToBitmap(screenshot,new Rectangle(0,0,1200,800));
        screenshot.Save(Path.Combine(dir,"camera-transition.png"));
        window.ShowOverview();
        window.DrawToBitmap(screenshot,new Rectangle(0,0,1200,800));
        screenshot.Save(Path.Combine(dir,"multi-room.png"));
        window.VerifyContinuousRoom();
        window.DrawToBitmap(screenshot,new Rectangle(0,0,1200,800));
        screenshot.Save(Path.Combine(dir,"room-4260-window.png"));
        File.WriteAllText(Path.Combine(dir,"results.json"),JsonSerializer.Serialize(new{
            passed=true,roomsRendered=game.Rooms.Count,spritesDecoded=data.Sprites.Count,
            spatialCameraValidated=true,nativeJumpAndLanding=true,independentCharacters=true,z80RuleChecks=rules,pushChecks=pushes,mechanicsChecks=mechanics,pickupChecks=pickups,interactionChecks=interactions,
            pickupsRecovered=data.Pickups.Count,nativeRuntime=true,movementAudioChecks=movementAudio,teleportAnimationChecks=teleportAnimations,continuousRoomChecks=continuousRooms,
            doorLinksValidated=doorLinks,teleportPairsRecovered=data.TeleportPairs.Length,
            fidelityValidated=false,completeGame=false},new JsonSerializerOptions{WriteIndented=true}));
    }
    static int VerifyTopology(WorldData data)
    {
        var rooms=data.Rooms.ToDictionary(r=>r.Id);int count=0;
        (int x,int y)[] delta=[(0,1),(1,0),(0,-1),(-1,0)];
        foreach(var room in data.Rooms)for(int side=0;side<4;side++)
        {
            if(room.Exits[side]<2)continue;
            var (x,y)=delta[side];
            int id=((((room.GridY+x)&15)*16+((room.GridX+y)&15))<<8)|(room.Level<<4);
            if(!rooms.TryGetValue(id,out var target)||target.Exits[(side+2)%4]<2)
                throw new InvalidDataException($"Door {room.Id:X4}/{side} has no return");
            count++;
        }
        if(count!=482)throw new InvalidDataException("Unexpected original door graph");
        foreach(var pair in data.TeleportPairs)if(!rooms.ContainsKey(pair[0])||!rooms.ContainsKey(pair[1]))
            throw new InvalidDataException("Unknown room in original teleport table");
        return count;
    }
}

sealed class GameWindow : Form
{
    readonly WorldData data; NativeGame game; readonly Renderer renderer;
    readonly RoomConnections connections;
    readonly GameAudio audio=new();
    readonly HashSet<Keys> keys=[];
    readonly System.Windows.Forms.Timer timer=new(){Interval=16};
    readonly Stopwatch clock=Stopwatch.StartNew();double last;
    bool overview,paused;
    int selectedRoom;
    readonly Dictionary<int,PointF> roomPositions=[];
    PointF camera;
    int cameraRoom;
    // GridY is physical X; GridX is physical Y, matching the original exits.
    internal static PointF RoomOffset(int dx,int dy,int dz=0)=>new((dx-dy)*160,-(dx+dy)*80+dz*320);
    static int GridDelta(int to,int from)=>(to-from+24)%16-8;
    void UpdateCamera(float dt,bool snap=false)
    {
        var next=game.Rooms[game.Player.Room].Data;
        if(roomPositions.Count==0){roomPositions[next.Id]=PointF.Empty;cameraRoom=next.Id;}
        if(!roomPositions.ContainsKey(next.Id)) {
            var previous=game.Rooms[cameraRoom].Data;
            var delta=RoomOffset(GridDelta(next.GridY,previous.GridY),GridDelta(next.GridX,previous.GridX),next.Level-previous.Level);
            var origin=roomPositions[cameraRoom];
            roomPositions[next.Id]=new(origin.X+delta.X,origin.Y+delta.Y);
        }
        foreach(int member in connections.Group(next.Id).Keys)
        {
            var delta=RoomConnections.ProjectOffset(connections.Offset(next.Id,member));
            var origin=roomPositions[next.Id];
            roomPositions[member]=new(origin.X+delta.X,origin.Y+delta.Y);
        }
        cameraRoom=next.Id;
        var group=connections.Group(next.Id).Keys.Select(id=>roomPositions[id]).ToArray();
        var target=new PointF(group.Average(p=>p.X),group.Average(p=>p.Y));
        float blend=snap?1:1-MathF.Exp(-dt*7);
        camera=new(camera.X+(target.X-camera.X)*blend,camera.Y+(target.Y-camera.Y)*blend);
    }
    readonly Font title=new("Segoe UI",20,FontStyle.Bold);
    readonly Font small=new("Segoe UI",10);
    readonly Font mono=new("Consolas",10);
    readonly Font label=new("Segoe UI",12,FontStyle.Bold);
    public GameWindow(WorldData data)
    {
        this.data=data;connections=new(data.Rooms);game=new(data);game.Sound+=audio.Play;renderer=new(data);selectedRoom=game.Player.Room;
        Text="Head over Heels · PC nativo · prototipo de reconstrucción";
        ClientSize=new Size(1200,800);MinimumSize=new Size(960,680);
        BackColor=Color.FromArgb(14,18,25);ForeColor=Color.White;DoubleBuffered=true;KeyPreview=true;
        timer.Tick+=(_,_)=>{
            double now=clock.Elapsed.TotalSeconds;float dt=(float)Math.Clamp(now-last,0,.04);last=now;
            if(!paused&&!overview){
                if(keys.Contains(Keys.Space))game.Jump();
                float horizontal=(keys.Contains(Keys.D)||keys.Contains(Keys.Right)?1:0)-(keys.Contains(Keys.A)||keys.Contains(Keys.Left)?1:0);
                float vertical=(keys.Contains(Keys.S)||keys.Contains(Keys.Down)?1:0)-(keys.Contains(Keys.W)||keys.Contains(Keys.Up)?1:0);
                var motion=WorldMotion(horizontal,vertical);
                game.Step(dt,motion.dx,motion.dy);selectedRoom=game.Player.Room;
            }
            UpdateCamera(dt);
            Invalidate();};
        UpdateCamera(0,true);
        timer.Start();
        KeyDown+=(_,e)=>{
            bool first=keys.Add(e.KeyCode);e.Handled=true;
            if(!first)return;
            if(e.KeyCode==Keys.Space)game.Jump();
            if(e.KeyCode==Keys.Tab){game.Switch();selectedRoom=game.Player.Room;}
            if(e.KeyCode==Keys.E)game.Carry();
            if(e.KeyCode==Keys.F||e.KeyCode==Keys.ControlKey)game.Fire();
            if(e.KeyCode==Keys.R){audio.Play(SoundCue.StopStep);game.Sound-=audio.Play;game=new(data);game.Sound+=audio.Play;selectedRoom=game.Player.Room;paused=false;overview=false;roomPositions.Clear();UpdateCamera(0,true);}
            if(e.KeyCode==Keys.M)audio.Muted=!audio.Muted;
            if(e.KeyCode==Keys.F2){overview=!overview;if(overview)audio.Play(SoundCue.StopStep);}
            if(e.KeyCode==Keys.Escape){paused=!paused;if(paused)audio.Play(SoundCue.StopStep);}
            if(e.KeyCode==Keys.F11){FormBorderStyle=FormBorderStyle==FormBorderStyle.None?FormBorderStyle.Sizable:FormBorderStyle.None;WindowState=WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized;}
            if(e.KeyCode==Keys.PageDown||e.KeyCode==Keys.PageUp){
                var ids=data.Rooms.Select(r=>r.Id).Order().ToArray();int i=Array.IndexOf(ids,selectedRoom);
                selectedRoom=ids[(i+(e.KeyCode==Keys.PageDown?1:ids.Length-1))%ids.Length];overview=true;audio.Play(SoundCue.StopStep);}
        };
        KeyUp+=(_,e)=>keys.Remove(e.KeyCode);Deactivate+=(_,_)=>keys.Clear();
    }
    // Each key follows one floor axis, parallel to the isometric walls.
    internal static (float dx,float dy) WorldMotion(float horizontal,float vertical)=>(-vertical,-horizontal);
    protected override bool IsInputKey(Keys keyData)=>true;
    internal void ShowOverview() => overview=true;
    internal void VerifyCamera()
    {
        int start=game.Player.Room;
        var origin=game.Rooms[start].Data;
        var adjacent=game.Rooms.Values.Select(r=>r.Data).First(r=>r.Level==origin.Level&&r.GridY==origin.GridY&&r.GridX==origin.GridX-1);
        game.Player.Room=adjacent.Id;selectedRoom=adjacent.Id;UpdateCamera(.08f);
        var before=roomPositions[start];var after=roomPositions[adjacent.Id];
        if(before!=PointF.Empty||after!=new PointF(160,80)||camera.X<=0||camera.X>=after.X)
            throw new Exception("Adjacent room placement or camera easing failed");
        game.Player.Room=start;UpdateCamera(.08f);
        if(roomPositions[start]!=before||roomPositions[adjacent.Id]!=after)
            throw new Exception("Room position changed on return");
        var destination=data.TeleportPairs.First(p=>p[0]==start||p[1]==start);
        game.Player.Room=destination[0]==start?destination[1]:destination[0];
        UpdateCamera(.08f);
        var target=roomPositions[game.Player.Room];
        UpdateCamera(2);
        if(Math.Abs(camera.X-target.X)>.1f||Math.Abs(camera.Y-target.Y)>.1f)
            throw new Exception("Teleport camera did not reach destination");
        game.Player.Room=adjacent.Id;selectedRoom=adjacent.Id;UpdateCamera(0,true);
    }
    internal void VerifyContinuousRoom()
    {
        overview=false;roomPositions.Clear();game.Player.Room=0x4260;selectedRoom=0x4260;
        game.Player.X=2;game.Player.Y=2;game.Player.Height=0;UpdateCamera(0,true);
        if(!roomPositions.ContainsKey(0x4360)||roomPositions[0x4360]!=new PointF(-96,-48))
            throw new Exception("Long room missing its second half");
        var before=camera;game.Player.Room=0x4360;UpdateCamera(0,true);
        if(camera!=before)throw new Exception("Camera moves across a continuous room's internal boundary");
        game.Player.Room=0x4260;UpdateCamera(0,true);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(BackColor);
        g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        using var muted=new SolidBrush(Color.FromArgb(144,159,179));
        using var bright=new SolidBrush(Color.FromArgb(239,231,207));
        using var accent=new SolidBrush(Color.FromArgb(240,195,105));
        g.DrawString("HEAD OVER HEELS",label,bright,12,8);
        g.DrawString(audio.Error is not null?"AUDIO NO DISPONIBLE":audio.Muted?"SONIDO: OFF (M)":"SONIDO: ON (M)",mono,muted,230,10);
        var viewport=new Rectangle(8,36,ClientSize.Width-280,ClientSize.Height-100);
        using var panel=new SolidBrush(Color.FromArgb(10,13,18));g.FillRectangle(panel,viewport);
        if(overview)DrawOverview(g,viewport);else DrawWorld(g,viewport);
        int sx=ClientSize.Width-258;
        g.DrawString(game.Player.IsHead?"HEAD":"HEELS",label,accent,sx,44);
        g.DrawString("Habitación "+selectedRoom.ToString("X4"),mono,bright,sx,151);
        var room=game.Rooms[selectedRoom].Data;
        g.DrawString($"Planta {room.Level} · Sector {room.GridX},{room.GridY}",small,muted,sx,176);
        int objectCount=connections.Group(selectedRoom).Keys.Sum(id=>game.Rooms[id].Data.Objects.Count);
        g.DrawString($"{objectCount} objetos originales",small,muted,sx,200);
        g.DrawString($"Vidas {game.Player.Lives} · Coronas {System.Numerics.BitOperations.PopCount((uint)game.CrownMask)}/5",small,bright,sx,225);
        g.DrawString("EL OTRO PERSONAJE",mono,muted,sx,248);
        var other=game.Characters[game.Active^1];
        DrawRoom(g,new Rectangle(sx,276,250,172),other.Room,true);
        g.DrawString(other.IsHead?"Head":"Heels",small,bright,sx,456);
        g.DrawString($"Bolsa {(game.HasBag?"sí":"no")} · Bocina {(game.HasHooter?"sí":"no")} · Tiros {game.Ammunition}",small,bright,sx,479);
        g.DrawString("DATOS RECUPERADOS",mono,muted,sx,501);
        g.DrawString("301 habitaciones · 100 sprites",small,bright,sx,529);
        g.DrawString("Vista F2: habitaciones simultáneas",small,muted,sx,555);
        g.DrawString("WASD / Flechas  Mover    Espacio  Saltar    Tab  Cambiar / unir    E  Bolsa    F  Disparar",small,bright,34,ClientSize.Height-59);
        g.DrawString("F2  Explorar mapa     RePág / AvPág  Habitación     Esc  Pausa     F11  Pantalla completa",small,muted,34,ClientSize.Height-40);
        g.DrawString(game.Notice.Length>0?game.Notice:"Reconstrucción en curso · reglas Z80 portadas parcialmente · sonido y fidelidad pendientes",small,muted,34,ClientSize.Height-21);
        if(paused){using var shade=new SolidBrush(Color.FromArgb(180,BackColor));g.FillRectangle(shade,viewport);g.DrawString("PAUSA",title,bright,viewport.X+40,viewport.Y+40);}
    }
    void DrawWorld(Graphics g,Rectangle bounds)
    {
        var state=g.Save();g.SetClip(bounds);
        var group=connections.Group(game.Player.Room).Keys.Select(id=>roomPositions[id]).ToArray();
        float width=440+group.Max(p=>p.X)-group.Min(p=>p.X),height=285+group.Max(p=>p.Y)-group.Min(p=>p.Y);
        float scale=Math.Min(bounds.Width/width,bounds.Height/height);
        g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
        foreach(var entry in roomPositions.OrderBy(p=>p.Value.Y))
        {
            float x=bounds.X+bounds.Width/2f+(entry.Value.X-camera.X-160)*scale;
            float y=bounds.Y+bounds.Height/2f+(entry.Value.Y-camera.Y-174)*scale;
            var dest=new RectangleF(x,y,320*scale,284*scale);
            if(!dest.IntersectsWith(bounds))continue;
            using var image=renderer.Room(game.Rooms[entry.Key],game.Characters.Where(p=>p.Room==entry.Key),true,true);
            g.DrawImage(image,dest);
        }
        g.Restore(state);
    }
    void DrawRoom(Graphics g,Rectangle bounds,int id,bool people)
    {
        using var b=renderer.ConnectedRoom(id,game.Rooms,connections,people?game.Characters:[]);
        float scale=Math.Min(bounds.Width/(float)b.Width,bounds.Height/(float)b.Height);
        int w=(int)(b.Width*scale),h=(int)(b.Height*scale);
        g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
        g.DrawImage(b,new Rectangle(bounds.X+(bounds.Width-w)/2,bounds.Y+(bounds.Height-h)/2,w,h));
    }
    void DrawOverview(Graphics g,Rectangle bounds)
    {
        var center=game.Rooms[selectedRoom].Data;
        int w=bounds.Width/3,h=bounds.Height/3;
        for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)
        {
            int id=((((center.GridY+y)&15)*16+((center.GridX+x)&15))<<8)|(center.Level<<4);
            var box=new Rectangle(bounds.X+(x+1)*w,bounds.Y+(y+1)*h,w,h);
            if(game.Rooms.ContainsKey(id))DrawRoom(g,box,id,true);
            using var pen=new Pen(x==0&&y==0?Color.FromArgb(240,195,105):Color.FromArgb(40,49,64));g.DrawRectangle(pen,box);
            if(game.Rooms.ContainsKey(id)){using var brush=new SolidBrush(Color.FromArgb(150,169,188));g.DrawString(id.ToString("X4"),mono,brush,box.X+6,box.Y+5);}
        }
    }
    protected override void Dispose(bool disposing)
    {
        if(disposing){timer.Dispose();audio.Dispose();renderer.Dispose();title.Dispose();small.Dispose();mono.Dispose();label.Dispose();}
        base.Dispose(disposing);
    }
}
