using System.Drawing.Drawing2D;

namespace HoH26;

public sealed class Renderer : IDisposable
{
    readonly Dictionary<(int,int,bool),Bitmap> cache=[];
    readonly Dictionary<int,SpriteData> sprites;
    readonly Dictionary<(int,int,bool),Bitmap> wallCache=[];
    readonly WorldData world;
    public static readonly Color[] Tints=[Color.FromArgb(244,209,135),Color.FromArgb(158,215,229),Color.FromArgb(191,217,150),Color.FromArgb(216,174,220),Color.FromArgb(235,173,147),Color.FromArgb(180,190,235),Color.FromArgb(216,211,198),Color.FromArgb(236,206,113)];
    public Renderer(WorldData data) {world=data;sprites=data.Sprites.ToDictionary(s=>s.Id);}
    public Bitmap Sprite(int id,int theme,bool flip=false)
    {
        flip^=(id&128)!=0;id&=127;
        if(!sprites.TryGetValue(id,out var s)) throw new InvalidDataException($"Unknown sprite {id}");
        var key=(id,theme,flip);
        if(cache.TryGetValue(key,out var existing))return existing;
        var b=new Bitmap(s.Width,s.Height);
        for(int y=0;y<s.Height;y++)for(int x=0;x<s.Width;x++)
        {
            int c=s.Pixels[y*s.Width+x];
            b.SetPixel(flip?s.Width-x-1:x,y,c<0?Color.Transparent:Ink(theme,c));
        }
        cache[key]=b;return b;
    }
    Color Ink(int area,int color)=>Color.FromArgb(unchecked((int)0xff000000)|world.Palettes[area%8][color]);
    Bitmap Wall(int theme,int area,bool flip)
    {
        var key=(theme,area,flip);if(wallCache.TryGetValue(key,out var old))return old;
        var s=world.Walls[theme*4];var b=new Bitmap(16,56);
        for(int y=0;y<56;y++)for(int x=0;x<16;x++)b.SetPixel(flip?15-x:x,y,Ink(area,s.Pixels[y*16+x]));
        wallCache[key]=b;return b;
    }
    // 1E77: C=X-Y+128, B=2Z-X-Y+127; native horizontal aspect 2:1.
    static PointF Project(float x,float y,float z) => new(160+(x-y)*16,190-(x+y)*8-z*12);
    public Bitmap Room(RoomState room,IEnumerable<Character>? people=null)
    {
        var b=new Bitmap(320,220);
        using var g=Graphics.FromImage(b);g.Clear(Color.Black);
        g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
        var r=room.Data;var tint=Tints[r.Theme%8];
        int xmin=(r.Bounds[0]-8)/8,xmax=(r.Bounds[2]-8)/8;
        int ymin=(r.Bounds[1]-8)/8,ymax=(r.Bounds[3]-8)/8;
        using var edge=new Pen(Ink(r.Area,1),1);
        using var tile=new SolidBrush(Color.Black);
        for(int x=xmin;x<xmax;x++)for(int y=ymin;y<ymax;y++)
        {
            PointF[] p=[Project(x-.5f,y-.5f,0),Project(x+.5f,y-.5f,0),Project(x+.5f,y+.5f,0),Project(x-.5f,y+.5f,0)];
            g.FillPolygon(tile,p);g.DrawPolygon(edge,p);
        }
        // Original direct wall planes (1A80), composed per room in native code.
        if(world.Walls.Count>0)
        {
            for(int x=xmin;x<xmax;x++)
            {
                var p=Project(x-.5f,ymax-.5f,0);
                g.DrawImageUnscaled(Wall(r.Theme,r.Area,false),(int)p.X,(int)p.Y-56);
            }
            for(int y=ymax-1;y>=ymin;y--)
            {
                var p=Project(xmax-.5f,y+.5f,0);
                g.DrawImageUnscaled(Wall(r.Theme,r.Area,true),(int)p.X,(int)p.Y-48);
            }
        }
        // Ground shadows belong below every solid object.
        foreach(var person in people??[]) {
            var floor=Project(person.X,person.Y,0);
            using var shadow=new SolidBrush(Color.FromArgb(110,0,0,0));
            g.FillEllipse(shadow,floor.X-7,floor.Y-1,14,4);
        }
        var draws=new List<DrawItem>();
        foreach(var projectile in room.Projectiles)
        {
            var shot=projectile;
            draws.Add(new(Volume.Body(shot.X,shot.Y,shot.Height,.375f,1),()=>{
                var image=Sprite(shot.Sprite,r.Area,(shot.Direction&2)==0);
                var point=Project(shot.X,shot.Y,shot.Height);
                g.DrawImageUnscaled(image,(int)point.X-image.Width/2,(int)point.Y-image.Height+8);
            }));
        }
        foreach(var pickup in room.Pickups.Where(p=>!p.Collected))
        {
            var item=pickup;
            draws.Add(new(Volume.Body(item.Data.X,item.Data.Y,item.Height,.3f,.5f),()=>{
                var image=Sprite(item.Sprite,r.Area);
                var point=Project(item.Data.X,item.Data.Y,item.Height);
                g.DrawImageUnscaled(image,(int)point.X-image.Width/2,(int)point.Y-image.Height+8);
            }));
        }
        foreach(var e in room.Entities.Where(e=>!e.Removed))
        {
            var entity=e;
            draws.Add(new(Volume.Entity(e),()=>{
                bool facing=entity.Source.Flip^(entity.Moved&&(entity.Direction&2)==0);
                var s=Sprite(entity.Sprite,r.Area,facing);
                var p=Project(entity.X,entity.Y,entity.Height);
                g.DrawImageUnscaled(s,(int)p.X-s.Width/2,(int)p.Y-s.Height+8);
                if(entity.Source.SecondarySprite>=0)
                {
                    var extra=Sprite(entity.Source.SecondarySprite,r.Area,entity.Source.Flip);
                    g.DrawImageUnscaled(extra,(int)p.X-extra.Width/2,(int)p.Y-extra.Height+2);
                }
            }));
        }
        foreach(var p in people??[])
        {
            var person=p;
            draws.Add(new(Volume.Body(p.X,p.Y,p.Height,.375f,1),()=>{
                var s=Sprite(person.Sprite,r.Area,person.Flip);
                var pt=Project(person.X,person.Y,person.Height);
                g.DrawImageUnscaled(s,(int)pt.X-s.Width/2,(int)pt.Y-s.Height+7);
            }));
        }
        for(int side=0;side<4;side++)
        {
            int code=r.Exits[side];if(code<2)continue;
            int style=r.DoorStyle>=3?r.DoorStyle-1:r.DoorStyle;
            for(int half=0;half<2;half++)
            {
                float x=3+half,y=3+half;
                if(side==0)y=ymax;else if(side==2)y=ymin-1;
                if(side==1)x=xmax;else if(side==3)x=xmin-1;
                var point=Project(x,y,code-2);
                int id=style*2+half;
                bool flip=side==1||side==3;
                draws.Add(new(Volume.Body(x,y,code-2,.5f,2),()=>{
                    var door=Sprite(id,r.Area,flip);
                    g.DrawImageUnscaled(door,(int)point.X-door.Width/2,(int)point.Y-door.Height+8);
                }));
            }
        }
        foreach(int index in DepthOrder.Sort(draws.Select(d=>d.Bounds).ToArray()))draws[index].Draw();
        return b;
    }
    sealed record DrawItem(Volume Bounds,Action Draw);
    public Bitmap Atlas(WorldData data)
    {
        var b=new Bitmap(800,700);using var g=Graphics.FromImage(b);g.Clear(Color.FromArgb(19,24,32));
        using var font=new Font("Consolas",9);using var brush=new SolidBrush(Color.FromArgb(155,169,184));
        foreach(var s in data.Sprites)
        {
            int x=(s.Id%10)*80,y=(s.Id/10)*70;
            g.DrawString(s.Id.ToString("D2"),font,brush,x+3,y+1);
            g.DrawImageUnscaled(Sprite(s.Id,0),x+25,y+12);
        }
        return b;
    }
    public void Dispose(){foreach(var b in cache.Values)b.Dispose();foreach(var b in wallCache.Values)b.Dispose();}
}
