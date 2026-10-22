using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace HoH26;

public enum SoundCue { Step, Jump, Land, Fire, Pickup, Damage, Teleport, Switch, Carry }

// Original synthesized effects, not a reproduction of the CPC sound driver.
sealed class GameAudio : IDisposable
{
    const int Rate=22050, Samples=512;
    readonly ConcurrentQueue<SoundCue> pending=new();
    readonly CancellationTokenSource stop=new();
    readonly Task worker;
    volatile bool muted;
    public bool Muted { get=>muted; set { muted=value; } }
    public string? Error { get; private set; }
    public GameAudio()=>worker=Task.Run(Run);
    public void Play(SoundCue cue) { if(!muted&&pending.Count<32)pending.Enqueue(cue); }
    sealed class Voice(SoundCue cue)
    {
        public readonly SoundCue Cue=cue;
        public int Position;
        public double Phase;
    }
    internal static (double duration,double start,double end) Tone(SoundCue cue)=>cue switch {
        SoundCue.Step=>(.035,110,60), SoundCue.Jump=>(.20,220,850),
        SoundCue.Land=>(.07,140,45), SoundCue.Fire=>(.16,950,160),
        SoundCue.Pickup=>(.32,520,1400), SoundCue.Damage=>(.45,450,55),
        SoundCue.Teleport=>(.65,180,1800), SoundCue.Switch=>(.10,440,660),
        _=>(.09,260,420)
    };
    void Run()
    {
        IntPtr device=IntPtr.Zero, data=IntPtr.Zero, header=IntPtr.Zero;
        bool prepared=false;
        try {
            var format=new WaveFormat { Format=1,Channels=1,Rate=Rate,BytesPerSecond=Rate*2,Align=2,Bits=16 };
            uint result=waveOutOpen(out device,uint.MaxValue,ref format,IntPtr.Zero,IntPtr.Zero,0);
            if(result!=0) { Error=$"No se pudo abrir el audio de Windows ({result})";return; }
            data=Marshal.AllocHGlobal(Samples*2);header=Marshal.AllocHGlobal(Marshal.SizeOf<WaveHeader>());
            Marshal.StructureToPtr(new WaveHeader { Data=data,Length=Samples*2 },header,false);
            Check(waveOutPrepareHeader(device,header,(uint)Marshal.SizeOf<WaveHeader>()));prepared=true;
            var voices=new List<Voice>();var pcm=new short[Samples];
            while(!stop.IsCancellationRequested) {
                while(pending.TryDequeue(out var cue)) { if(voices.Count>=8)voices.RemoveAt(0);voices.Add(new(cue)); }
                if(muted)voices.Clear();
                for(int i=0;i<Samples;i++) {
                    double mix=0;
                    foreach(var v in voices) {
                        var (duration,start,end)=Tone(v.Cue);double t=v.Position/(double)Rate;
                        if(t>=duration)continue;
                        double fraction=t/duration;
                        double frequency=start+(end-start)*fraction;
                        if(v.Cue==SoundCue.Teleport)frequency+=120*Math.Sin(t*70);
                        v.Phase+=frequency/Rate;
                        double envelope=Math.Min(1,t/.004)*Math.Min(1,(duration-t)/.015)*(1-fraction*.65);
                        mix+=(v.Phase%1<.5?1:-1)*envelope*.12;
                        v.Position++;
                    }
                    pcm[i]=(short)(Math.Clamp(mix,-.85,.85)*short.MaxValue);
                }
                voices.RemoveAll(v=>v.Position>=Tone(v.Cue).duration*Rate);
                Marshal.Copy(pcm,0,data,Samples);
                Check(waveOutWrite(device,header,(uint)Marshal.SizeOf<WaveHeader>()));
                while((Marshal.PtrToStructure<WaveHeader>(header).Flags&1)==0&&!stop.IsCancellationRequested)Thread.Sleep(2);
            }
        }
        catch(Exception e) { Error="Audio: "+e.Message; }
        finally {
            if(device!=IntPtr.Zero) {
                waveOutReset(device);
                if(prepared)waveOutUnprepareHeader(device,header,(uint)Marshal.SizeOf<WaveHeader>());
                waveOutClose(device);
            }
            if(header!=IntPtr.Zero)Marshal.FreeHGlobal(header);
            if(data!=IntPtr.Zero)Marshal.FreeHGlobal(data);
        }
    }
    static void Check(uint result) { if(result!=0)throw new InvalidOperationException($"Windows waveOut: {result}"); }
    public void Dispose() { stop.Cancel();worker.GetAwaiter().GetResult();stop.Dispose(); }
    [StructLayout(LayoutKind.Sequential,Pack=2)]
    struct WaveFormat { public ushort Format,Channels;public uint Rate,BytesPerSecond;public ushort Align,Bits,Extra; }
    [StructLayout(LayoutKind.Sequential)]
    struct WaveHeader { public IntPtr Data;public uint Length,Recorded;public UIntPtr User;public uint Flags,Loops;public IntPtr Next;public UIntPtr Reserved; }
    [DllImport("winmm.dll")] static extern uint waveOutOpen(out IntPtr device,uint id,ref WaveFormat format,IntPtr callback,IntPtr instance,uint flags);
    [DllImport("winmm.dll")] static extern uint waveOutPrepareHeader(IntPtr device,IntPtr header,uint size);
    [DllImport("winmm.dll")] static extern uint waveOutWrite(IntPtr device,IntPtr header,uint size);
    [DllImport("winmm.dll")] static extern uint waveOutReset(IntPtr device);
    [DllImport("winmm.dll")] static extern uint waveOutUnprepareHeader(IntPtr device,IntPtr header,uint size);
    [DllImport("winmm.dll")] static extern uint waveOutClose(IntPtr device);
}
