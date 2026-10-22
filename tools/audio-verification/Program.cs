using HoH26;
using System.Security.Cryptography;

// Independent first-note decode is performed by tools/verify_audio.py.
foreach(int command in new[]{0x80,0x85,0x88,0xC6,0x47,0x44,0x41,0xC5,0xC2,0xC4,0x48}) {
    var a=new OriginalAudio();a.Trigger(command);a.Frame();
    Console.WriteLine($"{command:X2}: {Convert.ToHexString(a.Registers)}");
    bool audible=false,ended=false;
    using var stream=new MemoryStream();
    for(int f=0;f<2000;f++) {
        if(f!=0)a.Frame();
        if(a.Registers.Skip(8).All(v=>v==0))ended=true;
        for(int s=0;s<441;s++){short sample=a.Sample(22050);audible|=sample!=0;stream.WriteByte((byte)sample);stream.WriteByte((byte)(sample>>8));}
    }
    if(!audible||!ended||a.Registers.Skip(8).Any(v=>v!=0))throw new Exception($"Effect {command:X2} silent or never ends");
    Console.WriteLine($"  audible={audible}; reaches silence={ended}; PCM SHA256={Convert.ToHexString(SHA256.HashData(stream.ToArray()))}");
}
{
    var uninterrupted=new OriginalAudio();var repeated=new OriginalAudio();
    uninterrupted.Trigger(0x44);repeated.Trigger(0x44);
    for(int f=0;f<8;f++){
        repeated.Trigger(0x44);uninterrupted.Frame();repeated.Frame();
        if(!uninterrupted.Registers.SequenceEqual(repeated.Registers))throw new Exception("Duplicate request restarted effect");
    }
    var reference=new OriginalAudio();var interrupted=new OriginalAudio();
    reference.Trigger(0xC6);interrupted.Trigger(0xC6);
    interrupted.Trigger(0x44);reference.Frame();interrupted.Frame();
    if(!reference.Registers.SequenceEqual(interrupted.Registers))throw new Exception("Single-channel request interrupted locked effect group");
    var stopped=new OriginalAudio();stopped.Trigger(0x44);stopped.Frame();stopped.Trigger(0x7F);stopped.Frame();
    if(stopped.Registers[9]!=0)throw new Exception("Stop command did not mute channel");
    var walking=new OriginalAudio();walking.Trigger(0x80);walking.Frame();walking.StopIfPlaying(0x80);walking.Frame();
    if(walking.Registers[10]!=0)throw new Exception("Footsteps did not stop");
    var jumping=new OriginalAudio();var jumpReference=new OriginalAudio();
    jumping.Trigger(0x85);jumpReference.Trigger(0x85);jumping.Frame();jumpReference.Frame();
    jumping.StopIfPlaying(0x80);jumping.Frame();jumpReference.Frame();
    if(!jumping.Registers.SequenceEqual(jumpReference.Registers))throw new Exception("Footstep stop cut jump sound");
    Console.WriteLine("Duplicate request, group lock and explicit channel stop checks passed.");
}
