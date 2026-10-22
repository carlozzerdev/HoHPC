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
            if(args.Contains("--verify-audio")) {
                using var audio=new GameAudio();
                Thread.Sleep(150);
                foreach(var cue in Enum.GetValues<SoundCue>()) { audio.Play(cue);Thread.Sleep(750); }
                if(audio.Error is not null)throw new Exception(audio.Error);
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"audio-verification.txt"),"Windows waveOut: opened and played all nine synthesized effects successfully.");
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
        int doorLinks=VerifyTopology(data);
        var game=new NativeGame(data);
        if(game.Rooms.Count!=301||data.Sprites.Count!=100)throw new Exception("Unexpected asset inventory");
        if(!game.Rooms.ContainsKey(0x8940)||!game.Rooms.ContainsKey(0x8a40))throw new Exception("Starting rooms absent");
        var other=game.Characters[1];float oldX=other.X;
        game.Jump();for(int i=0;i<180;i++)game.Step(1/60f,0,0);
        if(!game.Player.Grounded||game.Player.Height<0||other.X!=oldX)throw new Exception("Native simulation failed");
        var dir=Path.Combine(AppContext.BaseDirectory,"verification");Directory.CreateDirectory(dir);
        using var renderer=new Renderer(data);
        using(var atlas=renderer.Atlas(data))atlas.Save(Path.Combine(dir,"sprites.png"));
        using(var room=renderer.Room(game.Rooms[0x8a40],game.Characters.Where(c=>c.Room==0x8a40)))room.Save(Path.Combine(dir,"head-start.png"));
        using(var room=renderer.Room(game.Rooms[0x8940],game.Characters.Where(c=>c.Room==0x8940)))room.Save(Path.Combine(dir,"heels-start.png"));
        // Reproduce the reported support/occlusion scenes without changing gameplay.
        var stairsPeople=new[]{new Character(0x8a40,false){X=0,Y=6,Height=1},new Character(0x8a40,true){X=0,Y=6,Height=7}};
        using(var room=renderer.Room(game.Rooms[0x8a40],stairsPeople))room.Save(Path.Combine(dir,"depth-stairs.png"));
        var rider=new Character(0x8a40,false){X=7,Y=6,Height=1};
        using(var room=renderer.Room(game.Rooms[0x8a40],[rider]))room.Save(Path.Combine(dir,"depth-teleporter.png"));
        // Exercise every decoded room and sprite, not only the starting scene.
        foreach(var r in game.Rooms.Values){using var image=renderer.Room(r);}
        using var window=new GameWindow(data);window.ClientSize=new Size(1200,800);window.CreateControl();
        using var screenshot=new Bitmap(1200,800);window.DrawToBitmap(screenshot,new Rectangle(0,0,1200,800));
        screenshot.Save(Path.Combine(dir,"window.png"));
        window.ShowOverview();
        window.DrawToBitmap(screenshot,new Rectangle(0,0,1200,800));
        screenshot.Save(Path.Combine(dir,"multi-room.png"));
        File.WriteAllText(Path.Combine(dir,"results.json"),JsonSerializer.Serialize(new{
            passed=true,roomsRendered=game.Rooms.Count,spritesDecoded=data.Sprites.Count,
            nativeJumpAndLanding=true,independentCharacters=true,z80RuleChecks=rules,
            pickupsRecovered=data.Pickups.Count,nativeRuntime=true,
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
    readonly GameAudio audio=new();
    readonly HashSet<Keys> keys=[];
    readonly System.Windows.Forms.Timer timer=new(){Interval=16};
    readonly Stopwatch clock=Stopwatch.StartNew();double last;
    bool overview,paused;
    int selectedRoom;
    readonly Font title=new("Segoe UI",20,FontStyle.Bold);
    readonly Font small=new("Segoe UI",10);
    readonly Font mono=new("Consolas",10);
    readonly Font label=new("Segoe UI",12,FontStyle.Bold);
    public GameWindow(WorldData data)
    {
        this.data=data;game=new(data);game.Sound+=audio.Play;renderer=new(data);selectedRoom=game.Player.Room;
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
            Invalidate();};
        timer.Start();
        KeyDown+=(_,e)=>{
            bool first=keys.Add(e.KeyCode);e.Handled=true;
            if(!first)return;
            if(e.KeyCode==Keys.Space)game.Jump();
            if(e.KeyCode==Keys.Tab){game.Switch();selectedRoom=game.Player.Room;}
            if(e.KeyCode==Keys.E)game.Carry();
            if(e.KeyCode==Keys.F||e.KeyCode==Keys.ControlKey)game.Fire();
            if(e.KeyCode==Keys.R){game.Sound-=audio.Play;game=new(data);game.Sound+=audio.Play;selectedRoom=game.Player.Room;paused=false;overview=false;}
            if(e.KeyCode==Keys.M)audio.Muted=!audio.Muted;
            if(e.KeyCode==Keys.F2)overview=!overview;
            if(e.KeyCode==Keys.Escape)paused=!paused;
            if(e.KeyCode==Keys.F11){FormBorderStyle=FormBorderStyle==FormBorderStyle.None?FormBorderStyle.Sizable:FormBorderStyle.None;WindowState=WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized;}
            if(e.KeyCode==Keys.PageDown||e.KeyCode==Keys.PageUp){
                var ids=data.Rooms.Select(r=>r.Id).Order().ToArray();int i=Array.IndexOf(ids,selectedRoom);
                selectedRoom=ids[(i+(e.KeyCode==Keys.PageDown?1:ids.Length-1))%ids.Length];overview=true;}
        };
        KeyUp+=(_,e)=>keys.Remove(e.KeyCode);Deactivate+=(_,_)=>keys.Clear();
    }
    // Each key follows one floor axis, parallel to the isometric walls.
    internal static (float dx,float dy) WorldMotion(float horizontal,float vertical)=>(-vertical,-horizontal);
    protected override bool IsInputKey(Keys keyData)=>true;
    internal void ShowOverview() => overview=true;
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(BackColor);
        g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        using var muted=new SolidBrush(Color.FromArgb(144,159,179));
        using var bright=new SolidBrush(Color.FromArgb(239,231,207));
        using var accent=new SolidBrush(Color.FromArgb(240,195,105));
        g.DrawString("HEAD OVER HEELS",title,bright,32,22);
        g.DrawString("PC NATIVO  /  RECONSTRUCCIÓN DEL CPC",mono,muted,34,61);
        g.DrawString(audio.Error is not null?"AUDIO NO DISPONIBLE":audio.Muted?"SONIDO: OFF (M)":"SONIDO: ON (M)",mono,muted,34,82);
        g.DrawString("PROTOTIPO",mono,accent,ClientSize.Width-142,39);
        using var line=new Pen(Color.FromArgb(40,49,64));g.DrawLine(line,32,96,ClientSize.Width-32,96);
        var viewport=new Rectangle(32,116,ClientSize.Width-340,ClientSize.Height-230);
        using var panel=new SolidBrush(Color.FromArgb(10,13,18));g.FillRectangle(panel,viewport);
        if(overview)DrawOverview(g,viewport);else DrawRoom(g,viewport,game.Player.Room,true);
        int sx=ClientSize.Width-282;
        g.DrawString(game.Player.IsHead?"HEAD":"HEELS",label,accent,sx,118);
        g.DrawString("Habitación "+selectedRoom.ToString("X4"),mono,bright,sx,151);
        var room=game.Rooms[selectedRoom].Data;
        g.DrawString($"Planta {room.Level} · Sector {room.GridX},{room.GridY}",small,muted,sx,176);
        g.DrawString($"{room.Objects.Count} objetos originales",small,muted,sx,200);
        g.DrawString($"Vidas {game.Player.Lives} · Coronas {System.Numerics.BitOperations.PopCount((uint)game.CrownMask)}/5",small,bright,sx,225);
        g.DrawString("EL OTRO PERSONAJE",mono,muted,sx,248);
        var other=game.Characters[game.Active^1];
        DrawRoom(g,new Rectangle(sx,276,250,172),other.Room,true);
        g.DrawString(other.IsHead?"Head":"Heels",small,bright,sx,456);
        g.DrawString($"Bolsa {(game.HasBag?"sí":"no")} · Bocina {(game.HasHooter?"sí":"no")} · Tiros {game.Ammunition}",small,bright,sx,479);
        g.DrawString("DATOS RECUPERADOS",mono,muted,sx,501);
        g.DrawString("301 habitaciones · 100 sprites",small,bright,sx,529);
        g.DrawString("Vista F2: habitaciones simultáneas",small,muted,sx,555);
        g.DrawString("WASD / Flechas  Mover    Espacio  Saltar    Tab  Cambiar / unir    E  Bolsa    F  Disparar",small,bright,34,ClientSize.Height-88);
        g.DrawString("F2  Explorar mapa     RePág / AvPág  Habitación     Esc  Pausa     F11  Pantalla completa",small,muted,34,ClientSize.Height-61);
        g.DrawString(game.Notice.Length>0?game.Notice:"Reconstrucción en curso · reglas Z80 portadas parcialmente · sonido y fidelidad pendientes",small,muted,34,ClientSize.Height-34);
        if(paused){using var shade=new SolidBrush(Color.FromArgb(180,BackColor));g.FillRectangle(shade,viewport);g.DrawString("PAUSA",title,bright,viewport.X+40,viewport.Y+40);}
    }
    void DrawRoom(Graphics g,Rectangle bounds,int id,bool people)
    {
        using var b=renderer.Room(game.Rooms[id],people?game.Characters.Where(p=>p.Room==id):[]);
        float scale=Math.Min(bounds.Width/320f,bounds.Height/220f);
        int w=(int)(320*scale),h=(int)(220*scale);
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
