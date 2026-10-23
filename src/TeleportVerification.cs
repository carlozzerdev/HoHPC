namespace HoH26;

static class TeleportVerification
{
    public static int Run(WorldData data)
    {
        int checks=0;
        void Check(bool ok,string message){checks++;if(!ok)throw new Exception("Teleport: "+message);}
        foreach(int mode in new[]{0,1,2})
        {
            var game=new NativeGame(data);
            if(mode==1)game.Switch();
            if(mode==2){game.Characters[0].Room=game.Characters[1].Room;game.Characters[0].Height=1;game.Switch();}
            var p=game.Player;
            var pad=game.Rooms[p.Room].Entities.First(e=>(e.Sprite&127)==87);
            float x=pad.X,y=pad.Y;
            p.X=x;p.Y=y;p.Height=pad.Height+1;p.Grounded=true;
            if(game.Joined){game.Characters[0].X=x;game.Characters[0].Y=y;game.Characters[0].Height=p.Height+1;}
            int origin=p.Room;int[] lives=game.Characters.Select(c=>c.Lives).ToArray();
            int departures=0,arrivals=0;
            var synth=new OriginalAudio();bool audibleOut=false,audibleIn=false;
            game.Sound+=cue=>{
                if(cue==SoundCue.TeleportDepart)departures++;
                if(cue==SoundCue.TeleportArrive)arrivals++;
                GameAudio.Submit(synth,cue);
            };
            game.Jump();game.Step(NativeGame.TickSeconds,0,0);
            for(int tick=0;tick<20;tick++)
            {
                bool incoming=tick>=12;
                int raw=(incoming?data.TeleportInAnimation:data.DeathAnimation)[incoming?tick-12:tick];
                Check(p.TeleportTicks==game.TeleportTicks&&p.TeleportArriving==incoming,"character animation follows transport phase");
                Check(p.Sprite==(raw&127)&&p.RenderFlip==(((raw&128)!=0)^!p.IsHead),$"original frame and reflection rendered: mode={mode}, tick={tick}, ticks={p.TeleportTicks}, sprite={p.Sprite}, flip={p.RenderFlip}, expected={raw}");
                if(game.Joined)Check(game.Characters[0].Sprite==(raw&127)&&game.Characters[0].TeleportTicks==p.TeleportTicks,"both joined bodies animate");
                Check((p.Room!=origin)==incoming,"room changes between disappearance and appearance");
                Check(p.X==x&&p.Y==y&&p.DamageTicks==0,"transport freezes movement without damage");
                for(int frame=0;frame<4;frame++)
                {
                    synth.Frame();bool audible=synth.Registers.Skip(8).Any(v=>v>0);
                    if(incoming)audibleIn|=audible;else audibleOut|=audible;
                }
                game.Step(NativeGame.TickSeconds,1,0);
            }
            Check(game.TeleportTicks==0&&p.TeleportTicks==0,"ordinary character restored on completion");
            Check(departures==1&&arrivals==1&&audibleOut&&audibleIn,"each phase emits its own audible effect once");
            Check(game.Characters.Select(c=>c.Lives).SequenceEqual(lives),"transport animation does not cost lives");
        }
        return checks;
    }
}
