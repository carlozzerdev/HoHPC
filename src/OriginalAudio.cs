namespace HoH26;

// Native translation of the CPC effect sequencer (0B60–104D).
// This is a data sequencer, not a Z80 interpreter. All addresses refer to the
// relocated image recovered by tools/recover.py.
internal sealed class OriginalAudio
{
    readonly byte[] m = new byte[65536];
    readonly int[] start = new int[3], next = new int[3];
    readonly byte[][] state = [new byte[19],new byte[19],new byte[19],new byte[19]];
    readonly bool[] active = new bool[3], fresh = new bool[3];
    readonly int[] ids = [-1,-1,-1];
    public readonly byte[] Registers = new byte[11];
    readonly double[] phase = new double[3];
    double noisePhase;
    int lfsr = 0x1ffff;
    static readonly double[] Levels = [0,.00999,.01445,.02106,.03070,.04555,.06450,.10736,.12659,.20499,.29221,.37284,.49253,.63532,.80558,1];
    public OriginalAudio() { Array.Copy(OriginalAudioData.Bytes,0,m,0xB8B,OriginalAudioData.Bytes.Length); Registers[7]=63; }
    int Word(int p)=>m[p]|m[p+1]<<8;
    internal void StopIfPlaying(int command)
    {
        int channel=command>>6;
        if(channel<3&&ids[channel]==(command&63))Trigger((channel<<6)|63);
    }
    static int W(byte[] s,int p)=>s[p]|s[p+1]<<8;
    static void W(byte[] s,int p,int n) {s[p]=(byte)n;s[p+1]=(byte)(n>>8);}
    public void Trigger(int command)
    {
        int channel=command>>6,id=command&63;
        if(channel==3) { for(int c=0;c<3;c++){ids[c]=128;Begin(c,Word(0x1111+id*6+c*2));} return; }
        if(ids[channel]==128)return;
        if(ids[channel]==id)return;
        ids[channel]=id;
        if(id==63) {active[channel]=false;return;}
        Begin(channel,Word(Word(0x124C+channel*2)+id*2));
    }
    void Begin(int c,int address) {start[c]=next[c]=address;active[c]=fresh[c]=true;state[c][0]|=2;}
    int Envelope(byte[] s)
    {
        int p=W(s,1)+s[3],b=m[p];
        if((b&128)!=0) { s[4]=(byte)b;return b; }
        if((b&64)!=0) { if((s[0]&4)!=0){s[0]&=251;s[3]=0;}else s[0]|=4; }
        else s[3]++;
        s[4]=s[5];return s[5];
    }
    void Pattern(byte[] s,int index) {int p=0x10CE+index;W(s,1,p+m[p]);}
    void Configuration(int c,byte[] s,ref int p)
    {
        int b=m[p],hi=b>>4,lo=b&15;
        s[0]&=2;
        if((hi&4)!=0)s[0]|=64;
        if((hi&3)!=0) {
            int v=m[0x10AD+(hi&3)-1],e=v&15,d=v>>4;
            s[12]=(byte)e;int flags=0;
            if(d!=e) {
                if(d<e){(d,e)=(e,d);flags=128;}
                // 0FCB: shift the smaller power of two through carry.
                while((e&1)==0&&e!=0){e>>=1;d=(d>>1)|((d&1)<<7);}
                e>>=1;s[16]=(byte)(flags|d);
            }else s[16]=0;
            s[0]|=128;
        }
        int v2=m[0x10BE+lo];s[4]=s[5]=(byte)(v2>>4);Pattern(s,v2&15);
        if(c==0)s[0]&=247;
        if((b&128)==0)return;
        p++;
        if(c!=0)return;
        // 1012: RLCA, not SLA. Bit 7 rotates into bit 0 and disables
        // tone A for noise-only effects; bit 6 becomes the noise slide sign.
        var n=state[3];int noise=m[p];n[0]=(byte)(((noise&192)<<1)|(noise>>7));
        int a=0x10B8+(noise&15)*2,v3=m[a];n[4]=n[5]=(byte)(v3>>4);n[6]=m[a+1];
        Pattern(n,v3&15);n[7]=(byte)(n[6]+m[W(n,1)]);n[3]=0;s[0]|=8;
    }
    void Note(int c,byte[] s,ref int p)
    {
        s[0]&=239;
        for(int guard=0;guard<64;guard++) {
            if(m[p]!=255)break;
            p++;
            if(m[p]==255){active[c]=false;ids[c]=-1;return;}
            // 0E61/0E5A: FF 00 repeats from the initial configuration;
            // FF FF ends. Treating 00 as a configuration falls through into
            // the adjacent effect data (audible during sustained footsteps).
            if(m[p]==0){p=start[c]+1;}
            Configuration(c,s,ref p);p++;
        }
        int b=m[p],note=b>>3;s[13]=m[0x10B0+(b&7)];
        if(note==0){s[0]|=32;s[0]&=253;next[c]=p+1;return;}
        s[0]&=223;
        int pitch=s[10]+note,octave=pitch/12,index=pitch%12;
        int period=Word(0xB8B+index*2)>>octave,following=Word(0xB8D+index*2)>>octave;
        if((s[0]&66)==64)W(s,6,period);else W(s,8,period);
        if((s[0]&128)!=0) {
            int step=((period-following)&255)>>2,a=s[16];
            if(a!=0) {
                if((a&128)==0){while((a&1)==0){a=(a>>1)|((a&1)<<7);step=(step*2)&255;}}
                else {while((a&1)==0){step>>=1;a=(a>>1)|((a&1)<<7);}}
            }
            s[14]=(byte)step;s[15]=0;
        }
        if((s[0]&64)!=0) {
            if((s[0]&2)!=0)s[0]|=16;
            else {int delta=(short)(W(s,6)-W(s,8));s[0]=(byte)((s[0]&254)|(delta<0?1:0));int duration=s[13];while(duration>1){delta>>=1;duration>>=1;}W(s,17,delta);s[0]&=239;}
        }
        s[3]=0;s[4]=s[5];s[0]&=253;next[c]=p+1;
    }
    void Tick(int c)
    {
        if(!active[c])return;
        var s=state[c];int p=next[c];
        if(fresh[c]) {fresh[c]=false;s[10]=(byte)(m[p]>>2);s[11]=(byte)(m[p]&3);p++;Configuration(c,s,ref p);p++;Note(c,s,ref p);return;}
        s[13]--;if(s[13]==0){Note(c,s,ref p);if((s[0]&8)!=0){state[3][3]=0;Noise(true);}return;}
        s[4]--;if(s[4]==0)Envelope(s);
        int period=W(s,8);
        if((s[0]&128)!=0) {
            int d=(s[12]&128)!=0?-1:1;s[15]=(byte)(s[15]+d);
            int a=s[12];if(s[15]==a)s[12]=(byte)-a;
            period=(period+((a&128)!=0?s[14]:-s[14]))&65535;
        }
        if((s[0]&80)==64) {
            period=(period+(short)W(s,17))&65535;
            bool borrow=period<W(s,6);
            if((borrow?1:0)==(s[0]&1)){s[0]|=16;s[15]=0;period=W(s,6);}
        }
        W(s,8,period);
        if((s[0]&8)!=0)Noise(false);
    }
    void Noise(bool force)
    {
        var n=state[3];
        if(!force){n[4]--;if(n[4]!=0)return;}
        int result=Envelope(n);
        if(result==0&&n[3]==0)return;
        int v=m[W(n,1)+n[3]]&15;if((n[0]&128)!=0)v=-v;n[7]=(byte)(n[6]+v);
    }
    public void Frame()
    {
        for(int c=0;c<3;c++)Tick(c);
        Registers[7]=63;
        for(int c=0;c<3;c++) {
            var s=state[c];Registers[8+c]=0;
            if(!active[c]||(s[0]&32)!=0)continue;
            Registers[c*2]=s[8];Registers[c*2+1]=s[9];int v=m[W(s,1)+s[3]]&15;
            Registers[8+c]=(byte)(v==0?0:Math.Min(15,v+s[11]));Registers[7]&=(byte)~(1<<c);
        }
        if((state[0][0]&40)==8&&active[0]){Registers[6]=state[3][7];Registers[7]=(byte)((Registers[7]|(state[3][0]&1))&247);}
    }
    public short Sample(int rate)
    {
        noisePhase+=1_000_000.0/(16*Math.Max(1,(int)(Registers[6]&31))*rate);
        while(noisePhase>=1){noisePhase--;int bit=(lfsr^(lfsr>>3))&1;lfsr=(lfsr>>1)|(bit<<16);}
        double mix=0;
        for(int c=0;c<3;c++){
            int period=(Registers[c*2]|(Registers[c*2+1]&15)<<8);
            phase[c]=(phase[c]+1_000_000.0/(16*Math.Max(1,period)*rate))%1;
            bool gate=((Registers[7]&(1<<c))!=0||phase[c]<.5)&&((Registers[7]&(8<<c))!=0||(lfsr&1)!=0);
            mix+=(gate?1:-1)*Levels[Registers[8+c]&15]*.18;
        }
        return (short)(mix*short.MaxValue);
    }
}
