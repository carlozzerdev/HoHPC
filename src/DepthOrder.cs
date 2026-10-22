namespace HoH26;

internal readonly record struct Volume(float MinX,float MaxX,float MinY,float MaxY,float Bottom,float Top)
{
    public static Volume Body(float x,float y,float z,float radius,float height)=>new(x-radius,x+radius,y-radius,y+radius,z,z+height);
    public static Volume Entity(Entity e) {
        var c=e.Source.Collider;
        return new(e.X+c.MinX,e.X+c.MaxX,e.Y+c.MinY,e.Y+c.MaxY,e.Height,e.Height+c.Height);
    }
    public float Depth=>(MinX+MaxX+MinY+MaxY)/2-(Bottom+Top)*.75f;
}

internal static class DepthOrder
{
    // Larger X/Y are farther from the camera; higher Z is nearer.
    // A supporting surface must be painted before the body standing on it.
    internal static bool Behind(Volume a,Volume b)
    {
        const float epsilon=.0001f;
        bool overlapX=a.MinX<b.MaxX-epsilon&&a.MaxX>b.MinX+epsilon;
        bool overlapY=a.MinY<b.MaxY-epsilon&&a.MaxY>b.MinY+epsilon;
        if(overlapX&&overlapY) return a.Top<=b.Bottom+epsilon;
        bool farther=a.MinX>=b.MaxX-epsilon||a.MinY>=b.MaxY-epsilon;
        bool nearer=b.MinX>=a.MaxX-epsilon||b.MinY>=a.MaxY-epsilon;
        return farther&&!nearer;
    }
    internal static int[] Sort(IReadOnlyList<Volume> volumes)
    {
        int n=volumes.Count;
        var edges=new List<int>[n];var incoming=new int[n];var done=new bool[n];
        for(int i=0;i<n;i++)edges[i]=[];
        for(int i=0;i<n;i++)for(int j=i+1;j<n;j++) {
            if(Behind(volumes[i],volumes[j])) { edges[i].Add(j);incoming[j]++; }
            else if(Behind(volumes[j],volumes[i])) { edges[j].Add(i);incoming[i]++; }
        }
        var result=new int[n];
        for(int k=0;k<n;k++) {
            int next=-1;
            for(int i=0;i<n;i++)if(!done[i]&&incoming[i]==0&&(next<0||volumes[i].Depth>volumes[next].Depth))next=i;
            // Intersecting or cyclic composite bodies need a stable fallback.
            if(next<0)for(int i=0;i<n;i++)if(!done[i]&&(next<0||volumes[i].Depth>volumes[next].Depth))next=i;
            done[next]=true;result[k]=next;
            foreach(int target in edges[next])incoming[target]--;
        }
        return result;
    }
}
