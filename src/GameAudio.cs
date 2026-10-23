using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace HoH26;

public enum SoundCue { Step, Jump, Land, Fire, Pickup, Damage, Teleport, Switch, Carry, Spring, LiftMove, PickupEquipment, StopStep, Rise, Fall, FallHeels, TeleportDepart, TeleportArrive }

// PCM rendering of the original CPC effect data and native AY sequencer.
sealed class GameAudio : IDisposable
{
    const int Rate=22050, Samples=441, BufferCount=3;
    readonly ConcurrentQueue<SoundCue> pending=new();
    readonly CancellationTokenSource stop=new();
    readonly Task worker;
    volatile bool muted;
    public bool Muted { get=>muted; set { muted=value; } }
    public string? Error { get; private set; }
    long playedSamples;
    internal long PlayedSamples=>Interlocked.Read(ref playedSamples);
    public GameAudio()=>worker=Task.Run(Run);
    public void Play(SoundCue cue) { if(!muted&&pending.Count<32)pending.Enqueue(cue); }
    internal static int Command(SoundCue cue)=>cue switch {
        SoundCue.Step=>0x80,
        SoundCue.Jump=>0x85, SoundCue.Land=>0x88,
        SoundCue.Rise=>0x84, SoundCue.Fall=>0x83, SoundCue.FallHeels=>0x82,
        SoundCue.Damage=>0xC6, SoundCue.Teleport=>0x47,
        SoundCue.TeleportDepart=>0xC7, SoundCue.TeleportArrive=>0xC8,
        SoundCue.Spring=>0x44, SoundCue.LiftMove=>0x41,
        SoundCue.Pickup=>0xC5, SoundCue.PickupEquipment=>0xC2, SoundCue.Carry=>0xC4,
        SoundCue.Fire=>0x48,
        // No invented substitute for events whose original call site is unverified.
        _=>-1
    };
    internal static void Submit(OriginalAudio synth,SoundCue cue)
    {
        if(cue==SoundCue.StopStep){synth.StopIfPlaying(0x80);return;}
        int command=Command(cue);if(command>=0)synth.Trigger(command);
    }
    void Run()
    {
        IntPtr device=IntPtr.Zero;
        var buffers=new List<OutputBuffer>();
        using var completed=new AutoResetEvent(false);
        try {
            var format=new WaveFormat { Format=1,Channels=1,Rate=Rate,BytesPerSecond=Rate*2,Align=2,Bits=16 };
            // CALLBACK_EVENT: keep blocks queued while Windows plays earlier ones.
            uint result=waveOutOpen(out device,uint.MaxValue,ref format,completed.SafeWaitHandle.DangerousGetHandle(),IntPtr.Zero,0x50000);
            if(result!=0) { Error=$"No se pudo abrir el audio de Windows ({result})";return; }
            for(int i=0;i<BufferCount;i++)
            {
                var buffer=new OutputBuffer();buffers.Add(buffer);
                Marshal.StructureToPtr(new WaveHeader { Data=buffer.Data,Length=Samples*2 },buffer.Header,false);
                Check(waveOutPrepareHeader(device,buffer.Header,(uint)Marshal.SizeOf<WaveHeader>()));buffer.Prepared=true;
            }
            var synth=new OriginalAudio();var pcm=new short[Samples];int framePhase=Rate;
            while(!stop.IsCancellationRequested) {
                foreach(var buffer in buffers)
                {
                    if(buffer.Queued&&(Marshal.PtrToStructure<WaveHeader>(buffer.Header).Flags&1)==0)continue;
                    while(pending.TryDequeue(out var cue))
                    {
                        Submit(synth,cue);
                    }
                    for(int i=0;i<Samples;i++) {
                        if(framePhase>=Rate){synth.Frame();framePhase-=Rate;}
                        framePhase+=50;
                        short sample=synth.Sample(Rate);pcm[i]=muted?(short)0:sample;
                    }
                    Marshal.Copy(pcm,0,buffer.Data,Samples);
                    Check(waveOutWrite(device,buffer.Header,(uint)Marshal.SizeOf<WaveHeader>()));buffer.Queued=true;
                }
                var position=new WavePosition { Type=2 };
                if(waveOutGetPosition(device,ref position,12)==0&&position.Type==2)Interlocked.Exchange(ref playedSamples,position.Value);
                completed.WaitOne(10);
            }
        }
        catch(Exception e) { Error="Audio: "+e.Message; }
        finally {
            if(device!=IntPtr.Zero) {
                waveOutReset(device);
                foreach(var buffer in buffers)
                    if(buffer.Prepared)waveOutUnprepareHeader(device,buffer.Header,(uint)Marshal.SizeOf<WaveHeader>());
                waveOutClose(device);
            }
            foreach(var buffer in buffers){Marshal.FreeHGlobal(buffer.Header);Marshal.FreeHGlobal(buffer.Data);}
        }
    }
    sealed class OutputBuffer
    {
        public readonly IntPtr Data=Marshal.AllocHGlobal(Samples*2),Header=Marshal.AllocHGlobal(Marshal.SizeOf<WaveHeader>());
        public bool Prepared,Queued;
    }
    static void Check(uint result) { if(result!=0)throw new InvalidOperationException($"Windows waveOut: {result}"); }
    public void Dispose() { stop.Cancel();worker.GetAwaiter().GetResult();stop.Dispose(); }
    [StructLayout(LayoutKind.Sequential,Pack=2)]
    struct WaveFormat { public ushort Format,Channels;public uint Rate,BytesPerSecond;public ushort Align,Bits,Extra; }
    [StructLayout(LayoutKind.Sequential)]
    struct WaveHeader { public IntPtr Data;public uint Length,Recorded;public UIntPtr User;public uint Flags,Loops;public IntPtr Next;public UIntPtr Reserved; }
    [StructLayout(LayoutKind.Explicit,Size=12)]
    struct WavePosition { [FieldOffset(0)] public uint Type;[FieldOffset(4)] public uint Value; }
    [DllImport("winmm.dll")] static extern uint waveOutGetPosition(IntPtr device,ref WavePosition position,uint size);
    [DllImport("winmm.dll")] static extern uint waveOutOpen(out IntPtr device,uint id,ref WaveFormat format,IntPtr callback,IntPtr instance,uint flags);
    [DllImport("winmm.dll")] static extern uint waveOutPrepareHeader(IntPtr device,IntPtr header,uint size);
    [DllImport("winmm.dll")] static extern uint waveOutWrite(IntPtr device,IntPtr header,uint size);
    [DllImport("winmm.dll")] static extern uint waveOutReset(IntPtr device);
    [DllImport("winmm.dll")] static extern uint waveOutUnprepareHeader(IntPtr device,IntPtr header,uint size);
    [DllImport("winmm.dll")] static extern uint waveOutClose(IntPtr device);
}
