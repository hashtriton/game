using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalItemScriptActRules scriptActRules;
        sealed class ItemScriptAct
        {
            internal string ability;
            internal int actor,owner,target,ticks;
            internal double due,period,amount,second;
            internal OriginalPoint origin,point;
            internal readonly HashSet<int> hit=new HashSet<int>();
        }
        sealed class ItemSourceShield { internal double pool,pending,expires,next,factor; internal string ability; }
        readonly List<ItemScriptAct> itemScriptActs=new List<ItemScriptAct>();
        readonly Dictionary<int,ItemSourceShield> itemSourceShields=new Dictionary<int,ItemSourceShield>();
        readonly Dictionary<string,int> itemScriptActPulses=new Dictionary<string,int>();
        uint itemActRandom;
        int ItemScriptActPulseCount(int actor,string ability)=>itemScriptActPulses.TryGetValue(actor+":"+ability,out var count)?count:0;
        int ItemActRandomIndex(int inclusiveMaximum)
        {
            if(itemActRandom==0)itemActRandom=unchecked((uint)seed)^0x710B1A3u;
            if(itemActRandom==0)itemActRandom=1;
            itemActRandom^=itemActRandom<<13;itemActRandom^=itemActRandom>>17;itemActRandom^=itemActRandom<<5;
            return (int)(itemActRandom/4294967296.0*(inclusiveMaximum+1));
        }
        OriginalSessionReplyCode UseScriptActItem(Player player,OriginalItemScriptActRule rule)
        {
            int actor=OriginalWorld.HeroEntityId(player.slot);
            // Cm and equalization are resolved before the native activation
            // transaction. Unsupported profiles cannot consume mana/cooldown.
            try
            {
                if(rule.abilityId=="A1BM"||rule.abilityId=="A0T5")_ = ScriptedAbilityAttack(player);
                if(rule.abilityId=="A0KQ"&&ItemCrownTargets(world.UnitState(actor)).Count==0)return OriginalSessionReplyCode.RuleUnavailable;
            }
            catch(InvalidOperationException){return OriginalSessionReplyCode.RuleUnavailable;}
            if(!world.TrySpendMana(actor,rule.manaCost))return OriginalSessionReplyCode.NotReady;
            itemCooldowns[ItemCooldownKey(player.slot,rule.cooldownGroup)]=ItemClock+rule.cooldown;
            player.lastItemAction=OriginalItemActionCode.Success;
            NotifyNativeSpellEffect(actor,rule.abilityId);
            BeginItemScriptAct(actor,rule.abilityId);return OriginalSessionReplyCode.Accepted;
        }
        bool ItemActEnemy(OriginalWorldUnitView actor,OriginalWorldUnitView target,double radius,bool magic=false,bool structure=false)
            => target!=null&&target.health>.405&&AreEnemies(actor.ownerSlot,target.ownerSlot)&&
               SquaredDistance(actor.position,target.position)<=radius*radius&&(!magic||!CasterMagicImmune(target))&&
               (!structure||!CasterHasType(target,"structure"));
        void ItemActHit(ItemScriptAct effect,OriginalWorldUnitView target,double amount,OriginalTriggeredDamageMode mode)
            =>ApplyTriggeredHit(effect.actor,effect.owner,target,amount,mode);

        void BeginItemScriptAct(int actorId,string ability)
        {
            var actor=world.UnitState(actorId);if(actor==null)return;
            var effect=new ItemScriptAct{ability=ability,actor=actorId,owner=actor.ownerSlot};
            switch(ability)
            {
                case "A0WK": // Z7/z7: snapshot at EFFECT,32 callbacks, current maxima at completion.
                    effect.period=.03;effect.amount=actor.health/actor.profile.maxHealth;
                    effect.second=actor.profile.maxMana>0?actor.mana/actor.profile.maxMana:0;break;
                case "A09B": // Q7/P7: one shared h011 attempts A03W on every retained recipient.
                    foreach(var target in CasterUnits())if(ItemActEnemy(actor,target,400,true,true))
                    {
                        ItemActHit(effect,target,200,OriginalTriggeredDamageMode.SpellMagic);
                        var current=world.UnitState(target.entityId);
                        // AHtb/B02Q duration3 declared. Shared-caster multiple
                        // orders and arrival phase are a host family transfer;
                        // sparse native DataA is not invented as an extra hit.
                        if(current!=null&&current.health>.405&&!current.invulnerable&&!CasterHasType(current,"mechanical"))
                            AddTimedNativeStun(current.entityId,"B02Q",0,3);
                    }
                    return;
                case "A0BI":
                    double healing=0;
                    foreach(var target in CasterUnits())if(ItemActEnemy(actor,target,525,false,true)&&DuelCleanupEligible(target.rawcode))
                    {double amount=Math.Min(300,target.health*.08);healing+=amount;ItemActHit(effect,target,amount,OriginalTriggeredDamageMode.ChaosUniversal);}
                    ApplySourceHealing(actorId,actorId,Math.Min(1800,healing));return;
                case "A0BM":effect.period=.5;effect.ticks=1;itemScriptActPulses[actorId+":"+ability]=0;break;
                case "A0BP":
                    foreach(var target in CasterUnits())if(ItemActEnemy(actor,target,300,true,true))
                        ItemActHit(effect,target,300,OriginalTriggeredDamageMode.SpellMagic);
                    AddItemSourceShield(actorId,300,8);return;
                case "A16O":effect.period=5;break;
                case "A0DV":case "A0HZ":case "A0JJ":
                    BeginItemWrath(actorId,ability,5);return;
                case "A0FX":effect.period=.5;effect.ticks=14;break;
                case "A1BM":effect.period=.03;effect.ticks=42;effect.amount=ScriptedAbilityAttack(PlayerAt(actor.ownerSlot))*1.5;break;
                case "A0QQ":
                    ApplyItemScriptAreaDebuff(actor,"A0WJ",250);effect.period=.5;effect.ticks=10;
                    itemScriptActPulses[actorId+":"+ability]=1;break;
                case "A0KQ":BeginItemCrown(effect,actor);return;
                case "A0T5":effect.period=4;break;
                case "A1CT":
                    ApplyItemScriptAreaDebuff(actor,"A1CY",400);
                    foreach(var target in CasterUnits())if(target.health>0&&!AreEnemies(actor.ownerSlot,target.ownerSlot)&&SquaredDistance(actor.position,target.position)<=400*400)
                        ApplySourceHealing(actorId,target.entityId,250);
                    effect.period=1;break; // Timed-life helper death still reaches CA.
                case "A1CU":effect.period=1;effect.ticks=5;break;
                case "A1CV":
                    foreach(var target in CasterUnits())if(ItemActEnemy(actor,target,250))
                        itemScriptActs.Add(new ItemScriptAct{ability=ability,actor=actorId,owner=actor.ownerSlot,target=target.entityId,
                            period=.03,due=world.Clock+.03,ticks=15});
                    return;
                default:throw new InvalidOperationException("unimplemented-script-item:"+ability);
            }
            effect.due=world.Clock+effect.period;itemScriptActs.Add(effect);
        }

        void AddItemSourceShield(int actor,double pool,double seconds,double factor=1,string ability=null)
        {
            // Rp replaces the prior shield, including its accumulated event
            // damage. Op records damage; Xp heals only on its later .01 tick.
            if(!OriginalCombatDefinition.IsFinite(pool)||pool<=0||!OriginalCombatDefinition.IsFinite(seconds)||seconds<0||
                !OriginalCombatDefinition.IsFinite(factor)||factor<0)throw new ArgumentException("Invalid source shield.");
            if(ability!=null)ValidateAbilityChanges(new[]{ability});
            RemoveItemSourceShield(actor);
            if(ability!=null)ApplyUnitAbilityOverlay(actor,new[]{ability},null);
            itemSourceShields[actor]=new ItemSourceShield{pool=pool,expires=seconds==0?double.PositiveInfinity:world.Clock+seconds,
                next=world.Clock+.01,factor=factor,ability=ability};
        }
        void RemoveItemSourceShield(int actor)
        {
            if(!itemSourceShields.TryGetValue(actor,out var shield))return;
            itemSourceShields.Remove(actor);
            if(shield.ability!=null&&world.UnitState(actor)!=null)ApplyUnitAbilityOverlay(actor,null,new[]{shield.ability});
        }
        void ObserveItemScriptActDamage(OriginalWorldUnitView actor,double damage)
        {
            if(actor==null)return;
            if(itemSourceShields.TryGetValue(actor.entityId,out var shield))shield.pending+=damage;
            foreach(var effect in itemScriptActs)
                if(effect.actor==actor.entityId&&effect.ability=="A16O")effect.amount+=damage*.2;
        }
        void ObserveItemScriptActDeath(int actor)
        {
            RemoveItemSourceShield(actor); // fp destroys before Xp can revive a corpse.
            foreach(var effect in itemScriptActs.ToArray())
                if(effect.actor==actor&&effect.ability=="A16O")LaunchItemDragonWave(effect);
        }
        void LaunchItemDragonWave(ItemScriptAct effect)
        {effect.ability="A16O_wave";effect.amount+=600;effect.period=.15;effect.due=world.Clock+.15;effect.ticks=1;}

        static bool IsWrathAct(string ability)=>ability=="A0DV"||ability=="A0HZ"||ability=="A0JJ";
        void BeginItemWrath(int actorId,string ability,double seconds)
        {
            if(!IsWrathAct(ability)||!OriginalCombatDefinition.IsFinite(seconds)||seconds<=0)throw new ArgumentException("Invalid wrath effect.");
            var actor=world.UnitState(actorId);if(actor==null||actor.kind!=OriginalWorldUnitKind.Hero)throw new ArgumentException("Missing wrath hero.");
            double bonus=Math.Truncate(ItemScriptBaseAttribute(actor.ownerSlot,ability)*.75);
            itemScriptActs.Add(new ItemScriptAct{ability=ability,actor=actorId,owner=actor.ownerSlot,period=seconds,amount=bonus,due=world.Clock+seconds});
            RefreshItemScriptAttributes(actorId);
        }
        double ItemScriptBaseAttribute(int slot,string ability)
        {
            var player=PlayerAt(slot);var stats=ComposeSoulUpgrades(player.stats,slot);
            double value=ability=="A0HZ"?stats.strength.Require():ability=="A0DV"?stats.agility.Require():stats.intelligence.Require();
            if(itemPermanentAttributes.TryGetValue(slot,out var permanent))
                value+=ability=="A0HZ"?permanent.strength:ability=="A0DV"?permanent.agility:permanent.intelligence;
            foreach(var effect in itemScriptActs)if(effect.actor==slot&&effect.ability==ability)value+=effect.amount;
            return value; // GetHero*(false): exclude green equipment attributes.
        }
        void AddItemScriptAttributes(int actor,OriginalNativeItemProfile profile)
        {
            foreach(var effect in itemScriptActs)if(effect.actor==actor)
            {
                if(effect.ability=="A0HZ")profile.strength+=effect.amount;
                if(effect.ability=="A0DV")profile.agility+=effect.amount;
                if(effect.ability=="A0JJ")profile.intelligence+=effect.amount;
            }
        }
        void RefreshItemScriptAttributes(int actorId)
        {
            var actor=world.UnitState(actorId);if(actor==null)return;
            var stats=HeroCombatStats(actor.ownerSlot);var profile=actor.profile.Copy();
            profile.maxHealth=stats.maxHealth.Require();profile.maxMana=stats.maxMana.Require();
            // Source permanent setters are temporary by a later subtraction.
            // Ratio-nearest is the existing explicitly derived setter policy;
            // native per-attribute intermediate rounding is not remeasured here.
            double hp=UpdatedVitality(actor.health,actor.profile.maxHealth,profile.maxHealth,"ratio-nearest-approximation",true);
            double mp=UpdatedVitality(actor.mana,actor.profile.maxMana,profile.maxMana,"ratio-nearest-approximation",false);
            world.UpdateProfile(actorId,profile,hp,mp);
        }
        int ItemScriptAbilityRank(int actor,string ability,int rank)
        {
            if(ability!="A0FT"||rank<1)return rank;
            foreach(var effect in itemScriptActs)if(effect.actor==actor&&effect.ability=="A0FX")return 2;
            return rank;
        }

        void AdvanceItemScriptActs()
        {
            AdvanceItemScriptDebuffs();
            foreach(var pair in new List<KeyValuePair<int,ItemSourceShield>>(itemSourceShields))
            {
                var actor=world.UnitState(pair.Key);var shield=pair.Value;
                if(actor==null||actor.health<=.405||world.Clock+1e-9>=shield.expires)
                {RemoveItemSourceShield(pair.Key);continue;}
                if(world.Clock+1e-9<shield.next)continue;
                shield.next=world.Clock+.01;
                if(shield.pending>0)
                {
                    // Xp's last-pool branch intentionally restores pool, not
                    // pool/factor. Equality also takes that final branch.
                    double restored;
                    if(shield.pool>shield.pending*shield.factor){restored=shield.pending;shield.pool-=shield.pending*shield.factor;}
                    else {restored=shield.pool;shield.pool=0;}
                    shield.pending=0;
                    world.UpdateProfile(actor.entityId,actor.profile,Math.Min(actor.profile.maxHealth,actor.health+restored),actor.mana);
                    if(shield.pool<=0)RemoveItemSourceShield(pair.Key);
                }
            }
            foreach(var effect in itemScriptActs.ToArray())
                while(itemScriptActs.Contains(effect)&&effect.due<=world.Clock+1e-9)
                {
                    effect.due+=effect.period;
                    if(!TickItemScriptAct(effect))itemScriptActs.Remove(effect);
                }
        }
        bool TickItemScriptAct(ItemScriptAct effect)
        {
            if(TickExtendedItemScriptAct(effect,out bool retained))return retained;
            var actor=world.UnitState(effect.actor);if(actor==null)return false;
            if(IsWrathAct(effect.ability))
            {itemScriptActs.Remove(effect);RefreshItemScriptAttributes(effect.actor);return false;}
            if(effect.ability=="A0FX")
            {
                bool held=Array.Exists(PlayerAt(actor.ownerSlot).inventory.HeroSlots,item=>item?.itemId=="I076");
                return held&&effect.ticks-->0;
            }
            if(effect.ability=="A0WK")
            {
                if(effect.ticks++<=30)return true;
                // SetUnitState does not revive a dead unit. Host health0 stays0.
                world.UpdateProfile(actor.entityId,actor.profile,actor.health<=0?0:actor.profile.maxHealth*Math.Max(.1,effect.second),
                    actor.profile.maxMana*Math.Max(.1,effect.amount));return false;
            }
            if(effect.ability=="A0BM")
            {
                var targets=new List<OriginalWorldUnitView>();
                foreach(var target in CasterUnits())
                    if(ItemActEnemy(actor,target,600,true,true)&&!target.hidden&&DuelCleanupEligible(target.rawcode)&&
                        !(target.health/target.profile.maxHealth*100<=35&&SourceUnitUserData(target.entityId)==0&&!IsNativeHeroPredicate(target)))targets.Add(target);
                // vfv indexes inclusive0..count, intentionally allowing the
                // uninitialized final array slot. Never force a positive hit.
                int selected=ItemActRandomIndex(targets.Count);
                if(selected<targets.Count)ItemActHit(effect,targets[selected],500,OriginalTriggeredDamageMode.SpellMagic);
                itemScriptActPulses[effect.actor+":"+effect.ability]=effect.ticks;
                return effect.ticks++<=16&&actor.health>.405;
            }
            if(effect.ability=="A16O")
            {LaunchItemDragonWave(effect);return true;}
            if(effect.ability=="A16O_wave")
            {
                if(effect.ticks>=10)return false;
                foreach(var target in CasterUnits())
                    if(ItemActEnemy(actor,target,70*effect.ticks,true,true)&&effect.hit.Add(target.entityId))
                        ItemActHit(effect,target,effect.amount,OriginalTriggeredDamageMode.SpellMagic);
                effect.ticks++;return true;
            }
            var pulled=world.UnitState(effect.target);
            if(effect.ticks<=0||pulled==null||pulled.health<=.405||SquaredDistance(actor.position,pulled.position)<=2500)return false;
            double angle=Math.Atan2(actor.position.y-pulled.position.y,actor.position.x-pulled.position.x);
            world.ForcePosition(pulled.entityId,new OriginalPoint(pulled.position.x+15*Math.Cos(angle),pulled.position.y+15*Math.Sin(angle)));
            effect.ticks--;return true;
        }
    }
}
