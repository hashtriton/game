using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        // m5/XR20439, Vjv/XA30336, eC19853. Source clocks are integral
        // one-second trigger ticks; phase of the private native timer is not
        // exposed. Host uses its world-clock epoch and a separate RNG stream.
        int ordinaryRuneTicks,megaRuneTicks;
        double nextRuneTick=1;
        bool megaRuneTimerEnabled;
        uint runeRandom;
        static readonly string[] RunePool={"rsps","rres","rhe2","rman","rspd","vamp","tdex","tint","tstr","rspl","rdis"};
        static readonly int[] RuneWeights={10,10,10,10,10,10,8,8,8,10,10};

        void InitializeRuneAndBarrelWorld(OriginalWorld candidate)
        {
            if(candidate==null)return;
            nextRuneTick=candidate.Clock+1;
            foreach(var d in candidate.Snapshot().doodads)
                if(d.rawcode=="LTex"&&!options.explosiveBarrels&&!candidate.RemoveAuthoredDoodad(d.editorId))
                    throw new InvalidOperationException("explosive-barrel-removal-navigation-rejected");
            RefreshExplosiveBarrelProtection(candidate);
        }
        void RefreshExplosiveBarrelProtection(OriginalWorld candidate)
        {
            if(candidate==null||!options.explosiveBarrels)return;
            int alive=0;
            foreach(var p in players)
            {
                var u=candidate.UnitState(OriginalWorld.HeroEntityId(p.slot));
                if(u!=null&&u.health>.405)alive++;
            }
            // Source C0 ba[] count15993..16019; host death/restore latency
            // is one fixed tick rather than the source's .5s delayed C0.
            foreach(var d in candidate.Snapshot().doodads)
                if(d.rawcode=="LTex")candidate.SetDoodadInvulnerability(d.editorId,alive>2);
        }
        void OnRuneMatchEvent(OriginalMatchEvent item)
        {
            if(item.kind!=OriginalMatchEventKind.PhaseChanged)return;
            var phase=(OriginalMatchPhase)item.amount;
            if(item.round%5==0&&phase==OriginalMatchPhase.Combat)megaRuneTimerEnabled=true;
            else if(phase==OriginalMatchPhase.Preparation||phase==OriginalMatchPhase.RoundTransition||
                phase==OriginalMatchPhase.DuelPreparation||phase==OriginalMatchPhase.Duel||
                phase==OriginalMatchPhase.Won||phase==OriginalMatchPhase.Lost)megaRuneTimerEnabled=false;
            // VLv30461 enables XA regardless of Tc. RuneOff disables the
            // ordinary timer, but preserves source mega runes every25 ticks.
        }
        void AdvanceRuneAndBarrelWorld()
        {
            if(world==null||match==null)return;
            RefreshExplosiveBarrelProtection(world);
            while(world.Clock+1e-9>=nextRuneTick)
            {
                nextRuneTick+=1;
                if(match.Phase==OriginalMatchPhase.Won||match.Phase==OriginalMatchPhase.Lost||DuelActive)continue;
                if(options.runes&&match.Round%5!=0&&match.Phase==OriginalMatchPhase.Combat)
                {
                    ordinaryRuneTicks++;
                    if(ordinaryRuneTicks==45)
                    {
                        ordinaryRuneTicks=0;bool left=RuneDraw(2)==1;
                        SpawnRune(left?new OriginalPoint(-1156,1156):new OriginalPoint(1156,400),
                            left?-1216:1088,left?1088:320,left?-1088:1216,left?1216:448);
                    }
                }
                if(megaRuneTimerEnabled)
                {
                    megaRuneTicks++;
                    if(megaRuneTicks==25){megaRuneTicks=0;SpawnRune(new OriginalPoint(-2,-1920),-64,-1984,64,-1856);}
                }
            }
        }
        int RuneDraw(int size)
        {
            if(runeRandom==0){runeRandom=unchecked((uint)seed)^0x52554e45u;if(runeRandom==0)runeRandom=0x6D2B79F5u;}
            uint threshold=unchecked(0u-(uint)size)%(uint)size;
            do{runeRandom^=runeRandom<<13;runeRandom^=runeRandom>>17;runeRandom^=runeRandom<<5;}while(runeRandom<threshold);
            return 1+(int)(runeRandom%(uint)size);
        }
        void SpawnRune(OriginalPoint point,double minX,double minY,double maxX,double maxY)
        {
            // EnumItemsInRect removes any old item in this tiny source region,
            // including a dropped player item; cleanup is not rune-only.
            var old=new List<long>();
            foreach(var pair in groundItems)
                if(pair.Value.position.x>=minX&&pair.Value.position.x<=maxX&&pair.Value.position.y>=minY&&pair.Value.position.y<=maxY)old.Add(pair.Key);
            foreach(long id in old)groundItems.Remove(id);
            if(groundItems.Count>=8192)throw new InvalidOperationException("rune-ground-item-limit");
            int draw=RuneDraw(104),index=0;
            while(draw>RuneWeights[index]){draw-=RuneWeights[index];index++;}
            var item=players[0].inventory.CreateInstance(RunePool[index],0);
            groundItems.Add(item.instanceId,new OriginalGroundItemView{item=item,position=point});
        }
    }
}
