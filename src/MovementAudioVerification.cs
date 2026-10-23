namespace HoH26;

static class MovementAudioVerification
{
    public static int Run(WorldData data)
    {
        int checks=0;
        void Check(bool ok,string message){checks++;if(!ok)throw new Exception("Movement audio: "+message);}
        foreach(bool heels in new[]{false,true})
        {
            var game=new NativeGame(new(){Rooms=[
                new(){Id=0x8a40,Bounds=[8,8,72,72],Exits=[0,0,0,0],Floor=1},
                new(){Id=0x8940,Bounds=[8,8,72,72],Exits=[0,0,0,0],Floor=1}]});
            if(heels)game.Switch();
            var synth=new OriginalAudio();var cues=new List<SoundCue>();
            bool audible=false;
            game.Sound+=cues.Add;
            void Tick(float x=0)
            {
                cues.Clear();game.Step(NativeGame.TickSeconds,x,0);
                foreach(var cue in cues)GameAudio.Submit(synth,cue);
                audible=false;
                for(int frame=0;frame<4;frame++){synth.Frame();audible|=synth.Registers[10]>0;}
            }
            for(int i=0;i<48;i++)Tick(i%8<4?1:-1);
            Check(cues.Contains(SoundCue.Step),"walking emits steps");
            game.Jump();Tick(1);
            Check(cues.Contains(SoundCue.Jump)&&!cues.Contains(SoundCue.Step)&&!cues.Contains(SoundCue.Rise),"launch has priority over walking and ascent");
            Check(audible,"jump remains audible after walking stops");
            Tick(1);Check(cues.Contains(SoundCue.Rise)&&audible,"ascent continues audibly after launch");
            bool falling=false,landing=false;
            for(int i=0;i<80&&!landing;i++)
            {
                Tick();falling|=cues.Contains(heels?SoundCue.FallHeels:SoundCue.Fall);
                landing|=cues.Contains(SoundCue.Land);
            }
            Check(falling&&landing,"jump emits descent and landing cues");
            for(int i=0;i<10;i++)Tick();
            Check(synth.Registers.Skip(8).All(v=>v==0),"jump audio finishes after landing");
        }
        var teleport=new NativeGame(data);var heard=new List<SoundCue>();var ay=new OriginalAudio();
        teleport.Sound+=heard.Add;
        void TeleTick()
        {
            heard.Clear();teleport.Step(NativeGame.TickSeconds,0,0);
            foreach(var cue in heard)GameAudio.Submit(ay,cue);
            ay.Frame();
        }
        TeleTick();
        Check(heard.Contains(SoundCue.Teleport)&&ay.Registers[9]>0,"actual initial room teleporter emits audible ambient sound");
        teleport.Player.X=7;teleport.Player.Y=6;teleport.Player.Height=1;teleport.Player.Grounded=true;
        teleport.Jump();TeleTick();
        Check(teleport.TeleportTicks==12&&heard.Contains(SoundCue.TeleportDepart),"activating the teleporter emits departure sound");
        for(int i=0;i<20;i++)
        {
            TeleTick();Check(heard.Contains(SoundCue.TeleportArrive)==(i==11),"arrival effect occurs once at destination transition");
        }
        return checks;
    }
}
