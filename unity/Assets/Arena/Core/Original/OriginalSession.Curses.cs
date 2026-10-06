using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public static class OriginalCurseRules
    {
        public static bool IsCurse(string id)=>id!=null&&id.Length==4&&string.CompareOrdinal(id,"A19L")>=0&&string.CompareOrdinal(id,"A19T")<=0;
        public static string Name(string id)
        {
            switch(id){case "A19L":return "Тяжесть души";case "A19M":return "Холодное оцепенение";case "A19N":return "Заразный яд";
                case "A19O":return "Огненный шрам";case "A19P":return "Зеркало";case "A19Q":return "Кровяной яд";
                case "A19R":return "Каменная кожа";case "A19S":return "Печать тьмы";case "A19T":return "Узы смерти";default:return "";}
        }
    }

    public sealed partial class OriginalSession
    {
        uint curseRandom;
        double nextCurseSecond=1;
        readonly HashSet<int> heavyCurseCapped=new HashSet<int>();
        sealed class ColdManaReturn {internal int actor,remaining=5;internal double next;internal bool helperDied;}
        readonly List<ColdManaReturn> coldManaReturns=new List<ColdManaReturn>();

        int CurseDraw(int maximum)
        {
            if(curseRandom==0){curseRandom=unchecked((uint)seed)^0x43555253u;if(curseRandom==0)curseRandom=0x6D2B79F5u;}
            uint size=(uint)maximum,threshold=unchecked(0u-size)%size;
            do{curseRandom^=curseRandom<<13;curseRandom^=curseRandom>>17;curseRandom^=curseRandom<<5;}while(curseRandom<threshold);
            return (int)(curseRandom%size);
        }

        void InitializeWorldCurses()
        {
            if(!options.curse||world==null)return;
            var pool=new List<string>{"A19L","A19M","A19N","A19O","A19P","A19Q","A19R","A19S"};
            if(players.Count>=6)pool.Add("A19T");
            var assignments=new string[players.Count];int bonds=0;
            for(int i=0;i<assignments.Length;i++)
            {
                var candidates=new List<string>(pool);
                // iSv excludes9 at the penultimate slot of even large parties.
                if(i>0&&players.Count%2==0&&i==players.Count-2)candidates.Remove("A19T");
                string id=candidates[CurseDraw(candidates.Count)];assignments[i]=id;pool.Remove(id);if(id=="A19T")bonds++;
            }
            if(players.Count>=6)
                while(bonds<2)
                {
                    var available=new List<int>();for(int i=0;i<assignments.Length;i++)if(assignments[i]!="A19T")available.Add(i);
                    assignments[available[CurseDraw(available.Count)]]="A19T";bonds++;
                }
            for(int i=0;i<players.Count;i++)GrantWorldCurse(players[i],assignments[i]);
        }

        bool HasWorldCurse(int owner,string id)
        {
            var player=players.Find(p=>p.slot==owner);return player!=null&&player.auxiliaryAbilities.TryGetValue(id,out int level)&&level>0;
        }
        void GrantWorldCurse(Player player,string id)
        {
            if(!OriginalCurseRules.IsCurse(id))throw new ArgumentException("Unknown source curse.",nameof(id));
            if(player.auxiliaryAbilities.ContainsKey(id))return;
            var actor=world.UnitState(OriginalWorld.HeroEntityId(player.slot));if(actor==null)throw new InvalidOperationException("curse-hero-missing");
            string[] granted=id=="A19L"?new[]{id,"A1AO","A1AN"}:id=="A19R"?new[]{id,"A19V","A0V2"}:new[]{id};
            if(id=="A19R")for(int slot=2;slot<6;slot++)player.inventory.InstallCurseSlotBlock(slot);
            ApplyUnitAbilityOverlay(actor.entityId,granted,null);player.auxiliaryAbilities[id]=1;
        }
        void ApplyDuelCurse(Player player,OriginalDuelEvent item)
        {
            // The source duel emits only the blood-poison grant. A trusted
            // zero-level removal must not publish rank0 or keep a native overlay.
            if(item.amount>0){GrantWorldCurse(player,item.code);return;}
            if(!OriginalCurseRules.IsCurse(item.code))throw new ArgumentException("Unknown duel curse.");
            player.auxiliaryAbilities.Remove(item.code);
            var actor=world?.UnitState(OriginalWorld.HeroEntityId(player.slot));if(actor==null)return;
            string[] removed=item.code=="A19L"?new[]{item.code,"A1AO","A1AN"}:item.code=="A19R"?new[]{item.code,"A19V","A0V2"}:new[]{item.code};
            ApplyUnitAbilityOverlay(actor.entityId,null,removed);
            if(item.code=="A19L"){heavyCurseCapped.Remove(actor.entityId);RefreshAbilityMovement(actor.entityId);}
            if(item.code=="A19R")
                for(int slot=0;slot<6;slot++)
                {
                    var stone=player.inventory.HeroSlots[slot];
                    if(stone?.itemId=="I09D")player.inventory.RemoveForScript(OriginalInventoryBag.Hero,slot,stone.instanceId);
                }
            // Previously delivered native net/silence and iIv mana timers
            // retain their own duration after the curse ability is removed.
        }
        double CurseArmorFraction(int actorId)
        {
            var actor=world?.UnitState(actorId);return actor!=null&&HasWorldCurse(actor.ownerSlot,"A19L")?combatCatalog.Ability("A1AN").Number("DataA1"):0;
        }
        double CurseMoveSpeed(int actorId,double speed)=>heavyCurseCapped.Contains(actorId)?Math.Min(250,speed):speed;

        static bool InColdCurseArea(OriginalPoint p)
        {
            double[,] rects={{-416,2112,-32,2496},{320,1376,704,1760},{1088,1344,1472,1728},{-1312,2176,-928,2560},
                {-1568,1472,-1184,1856},{-1856,-320,-1472,64},{-512,-992,-128,-608},{832,-800,1216,-416},
                {-640,-2432,-256,-2048},{256,-2432,640,-2048},{-640,-3328,-256,-2944},{256,-3328,640,-2944},{128,544,512,928}};
            for(int i=0;i<rects.GetLength(0);i++)if(p.x>=rects[i,0]&&p.x<=rects[i,2]&&p.y>=rects[i,1]&&p.y<=rects[i,3])return false;
            return true;
        }
        void ObserveCurseSpellEffect(int actorId,string abilityId)
        {
            var actor=world?.UnitState(actorId);
            if(actor==null||actor.kind!=OriginalWorldUnitKind.Hero||actor.entityId!=OriginalWorld.HeroEntityId(actor.ownerSlot)||
                !HasWorldCurse(actor.ownerSlot,"A19M")||!InColdCurseArea(actor.position)||
                !OriginalCasterFieldRules.SpellTriggersCurse(abilityId,actor.rawcode))return;
            // ibv/iIv: native A19U is the measured Aens movement-only family;
            // source mana timer decrements5 before granting, hence four ticks.
            // A19U targs excludes invulnerable recipients. Its native Aens
            // order may reject while ibv still creates the independent iIv timer.
            if(!actor.invulnerable)
                SetActorControl(actorId,"curse-cold:A19U",OriginalActorControlMask.Move,5,true,true);
            coldManaReturns.Add(new ColdManaReturn{actor=actorId,next=world.Clock+1});
        }

        void ObserveCurseMegaDeath(int sourceUserData)
        {
            if(sourceUserData!=2)return;
            foreach(var player in players)
                if(HasWorldCurse(player.slot,"A19R"))
                    for(int slot=0;slot<6;slot++)
                    {
                        var item=player.inventory.HeroSlots[slot];if(item?.itemId!="I09D")continue;
                        var removed=player.inventory.RemoveForScript(OriginalInventoryBag.Hero,slot,item.instanceId);
                        if(!removed.Applied)throw new InvalidOperationException("curse-slot-release-rejected");break;
                    }
        }
        void ObserveDeathBonds(int slot)
        {
            var dying=world?.UnitState(OriginalWorld.HeroEntityId(slot));
            if(dying==null||dying.health>.405||DuelActive||!HasWorldCurse(slot,"A19T"))return;
            foreach(var player in players)
            {
                if(player.slot==slot||!HasWorldCurse(player.slot,"A19T"))continue;
                var actor=world.UnitState(OriginalWorld.HeroEntityId(player.slot));
                if(actor!=null&&actor.health>.405&&world.ForceUnitDeath(actor.entityId))ReportHeroDied(player.slot);
            }
        }
    }
}
