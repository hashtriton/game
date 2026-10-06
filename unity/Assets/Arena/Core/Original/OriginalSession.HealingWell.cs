using System;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalWellView
    {
        public bool present;
        public OriginalPoint position;
        public double mana,maxMana;
    }

    public sealed partial class OriginalSession
    {
        static readonly OriginalPoint HealingWellPosition=new OriginalPoint(-60,-380);
        bool healingWellPresent;
        double healingWellMana,healingWellAt=double.PositiveInfinity;
        long healingWellEvent;

        OriginalWellView SnapshotHealingWellView()=>healingWellPresent?
            new OriginalWellView{present=true,position=HealingWellPosition,mana=healingWellMana,maxMana=2000}:new OriginalWellView();
        double HealingWellRange=>combatCatalog.Ability("Ambt").Number("Area1");
        double HealingWellActorRange(OriginalWorldUnitView actor)
        {
            // WELL5(db59aa4adf10): unchanged e00M/Ambt and all three
            // authored heroes with body24 admit425, reject426. This is an
            // observed unit-target reach, not an invented caster-body value.
            if(actor.kind==OriginalWorldUnitKind.Hero&&actor.profile.collisionRadius==24&&
                (actor.rawcode=="H008"||actor.rawcode=="N0A0"||actor.rawcode=="H024"))return 425;
            // Other organic bodies transfer the authored area+recipient
            // radius policy until that native body family is measured.
            return HealingWellRange+actor.profile.collisionRadius;
        }

        void OnHealingWellMatchEvent(OriginalMatchEvent item)
        {
            if(world==null||item.kind!=OriginalMatchEventKind.ShopAccess||!item.enabled||item.sequence<=healingWellEvent)return;
            // xU is OU's delayed callback, not every opening of the shops.
            // D4's duel branches return before OU; oU>=30 skips refills.
            int completed=item.round-1;
            if(match.Phase!=OriginalMatchPhase.Preparation||completed<1||completed>=30)return;
            healingWellEvent=item.sequence;
            bool refill=completed==1||options.difficulty==OriginalDifficulty.Easy||
                (options.difficulty==OriginalDifficulty.Extreme?completed%3==0:completed%2!=0);
            if(refill)healingWellAt=ItemClock+item.time-match.Clock+1;
        }

        void AdvanceHealingWell()
        {
            if(world==null||ItemClock+1e-9<healingWellAt)return;
            var ability=combatCatalog.Ability("Ambt");var definition=combatCatalog.Unit("e00M");
            if(ability.Text("code")!="Ambt"||ability.Number("DataA1")!=1||ability.Number("DataB1")!=1||
                ability.Number("Area1")!=400||definition.Number("manaN")!=2000)
                throw new InvalidOperationException("healing-well-source-changed");
            healingWellPresent=true;healingWellMana=2000;healingWellAt=double.PositiveInfinity;
            // WELL1 original e00M is mana-neutral in day and night controls;
            // the stock emow positive control regenerates1.25MP/s at night.
            // Its stock rate must not be inherited into this optimized unit.
        }

        bool CanInteractHealingWell(Player player,int actorEntityId,out OriginalWorldUnitView actor)
        {
            int id=actorEntityId==0?OriginalWorld.HeroEntityId(player.slot):actorEntityId;
            actor=world?.UnitState(id);
            return Started&&healingWellPresent&&match.Phase!=OriginalMatchPhase.Won&&match.Phase!=OriginalMatchPhase.Lost&&
                actor!=null&&actor.ownerSlot==player.slot&&actor.health>.405&&!actor.paused&&!actor.hidden&&
                (actor.kind==OriginalWorldUnitKind.Hero||actor.kind==OriginalWorldUnitKind.Summon)&&
                !CasterHasType(actor,"mechanical")&&!CasterHasType(actor,"structure");
        }

        OriginalSessionReplyCode UseHealingWell(Player player,int actorEntityId)
        {
            if(!CanInteractHealingWell(player,actorEntityId,out var actor))return OriginalSessionReplyCode.NotReady;
            double range=HealingWellActorRange(actor);
            if(SquaredDistance(actor.position,HealingWellPosition)>range*range||healingWellMana<=0)
                return OriginalSessionReplyCode.NotReady;
            double hpNeed=Math.Max(0,actor.profile.maxHealth-actor.health),mpNeed=Math.Max(0,actor.profile.maxMana-actor.mana);
            if(hpNeed==0&&mpNeed==0)return OriginalSessionReplyCode.NotReady;
            // WELL1/2: budget20 with both deficits gives10+10; deficit2 on
            // either axis redirects its unused half to the other. Full axes
            // cost nothing. Ambt's authored two transfer ratios are both1.
            double half=healingWellMana*.5;
            double hp=Math.Min(hpNeed,half),mp=Math.Min(mpNeed,half);
            double remaining=healingWellMana-hp-mp;
            double extra=Math.Min(hpNeed-hp,remaining);hp+=extra;remaining-=extra;
            mp+=Math.Min(mpNeed-mp,remaining);
            if(!world.UpdateProfile(actor.entityId,actor.profile,actor.health+hp,actor.mana+mp))
                return OriginalSessionReplyCode.RuleUnavailable;
            healingWellMana=Math.Max(0,healingWellMana-hp-mp);
            return OriginalSessionReplyCode.Accepted;
        }
    }
}
