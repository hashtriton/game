using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemSummonBonuses { internal double attack,armor; }
        sealed class ItemOrbSource { internal int actor,owner; internal OriginalPoint point; internal double due; }
        readonly Dictionary<int,ItemSummonBonuses> itemSummonBonuses=new Dictionary<int,ItemSummonBonuses>();
        readonly List<ItemOrbSource> itemOrbSources=new List<ItemOrbSource>();
        double nextAssimilatedOrderScan;
        static readonly string[] AssimilationAbilities={"A0YP","A08U","A08T","ACmi"};
        double SummonScriptAttackBonus(int id)=>itemSummonBonuses.TryGetValue(id,out var b)?b.attack:0;
        double SummonScriptArmorBonus(int id)=>itemSummonBonuses.TryGetValue(id,out var b)?b.armor:0;

        bool ItemAssimilationEligible(OriginalWorldUnitView target)
        {
            // H7/j7 at21239: ConvertUnitType(9) is GIANT in common.j,
            // not MECHANICAL(15). Native ANfd's target mask is a separate gate.
            if(target==null||CasterHasType(target,"giant")||CasterHasType(target,"structure")||
                IsNativeHeroPredicate(target)||target.kind==OriginalWorldUnitKind.Illusion||
                SourceUnitUserData(target.entityId)==1||SourceUnitUserData(target.entityId)==2||
                HasEffectiveUnitAbility(target,"A0K4"))return false;
            return true;
        }
        OriginalWorldUnitProfile AssimilationProfile(string rawcode,int level)
        {
            var nativeUnit=observed?.Unit(rawcode);
            var profile=nativeUnit!=null&&nativeUnit.known
                ?new OriginalWorldUnitProfile{maxHealth=nativeUnit.RequireMaxHP(),maxMana=nativeUnit.RequireMaxMP(),
                    moveSpeed=nativeUnit.RequireMoveSpeed(),collisionRadius=OriginalUnitCollisionRules.Resolve(combatCatalog,rawcode).radius}
                :ScriptedEnemyProfile(rawcode);
            // Fresh CreateUnit, followed by source zl(life,50*level). The
            // victim's f3 additions, current resources and buffs are not copied.
            profile.maxHealth+=50*level;return profile;
        }
        OriginalWorldSummonSpawn PrepareAssimilation(int actorId,OriginalWorldUnitView target)
        {
            var actor=world.UnitState(actorId);
            if(actor==null||actor.kind!=OriginalWorldUnitKind.Hero||actor.entityId!=actor.ownerSlot)
                throw new InvalidOperationException("assimilation-source-identity-unavailable");
            if(nextItemSummonId>OriginalWorld.LastSummonEntityId)
                throw new InvalidOperationException("assimilation-identity-budget-exhausted");
            ValidateAbilityChanges(AssimilationAbilities);
            int level=PlayerAt(actor.ownerSlot).progression.Level;
            var profile=AssimilationProfile(target.rawcode,level);
            if(!FindItemSummonPoint(target.position,profile.collisionRadius,Array.Empty<OriginalWorldSummonSpawn>(),0,out var point))
                throw new InvalidOperationException("assimilation-placement-unavailable");
            return new OriginalWorldSummonSpawn{entityId=nextItemSummonId,ownerSlot=actor.ownerSlot,sourceHeroEntityId=actorId,
                rawcode=target.rawcode,profile=profile,position=point,health=profile.maxHealth,
                mana=profile.maxMana==0?0:Math.Min(profile.maxMana,combatCatalog.Unit(target.rawcode).Number("mana0")),
                invulnerable=Array.IndexOf(combatCatalog.Unit(target.rawcode).Text("abilList").Split(','),"Avul")>=0};
        }
        int BeginItemAssimilation(int actorId,int targetId)
        {
            var target=world.UnitState(targetId);if(!ItemAssimilationEligible(target))return 0;
            // Preflight unknown profile/ability/body/identity before hL can
            // mutate the target. Recheck placement after arbitrary callbacks.
            var row=PrepareAssimilation(actorId,target);int level=PlayerAt(row.ownerSlot).progression.Level;
            var originalPoint=target.position;double damage=target.profile.maxHealth+1;
            ApplyTriggeredHit(actorId,row.ownerSlot,target,damage,OriginalTriggeredDamageMode.ChaosUniversal);
            // H7 does not test whether hL killed the victim. Fresh creation is
            // unconditional, including a survivor or dead source. Bounded host
            // placement is derived; native displacement is not reproduced.
            if(nextItemSummonId>OriginalWorld.LastSummonEntityId||
                !FindItemSummonPoint(originalPoint,row.profile.collisionRadius,Array.Empty<OriginalWorldSummonSpawn>(),0,out row.position))return 0;
            row.entityId=nextItemSummonId;
            if(!world.TryPublishSourceSummons(new[]{row}))return 0;
            itemSummons.Add(row.entityId,new ItemSummonState{remaining=double.PositiveInfinity});
            itemSummonBonuses.Add(row.entityId,new ItemSummonBonuses{attack=Math.Min(8191,5*level),armor=Math.Min(8191,level)});
            ApplyUnitAbilityOverlay(row.entityId,AssimilationAbilities,null);nextItemSummonId++;
            // A0YP's sparse native slow-aura values remain separate. The
            // granted A08U/A08T orders use the guarded host scan below.
            return row.entityId;
        }
        void BeginItemOrbSource(int actorId,OriginalPoint point)
        {
            var actor=world.UnitState(actorId);if(actor==null)return;
            // W7/w7/U7 at21615: the authored delayed300magic sweep is separate
            // from native AUin's summon, impact, stun and timed life.
            itemOrbSources.Add(new ItemOrbSource{actor=actorId,owner=actor.ownerSlot,point=point,due=world.Clock+1});
        }
        void AdvanceItemSummonScripts()
        {
            foreach(var cast in itemOrbSources.ToArray())
            {
                if(cast.due>world.Clock+1e-9)continue;itemOrbSources.Remove(cast);
                foreach(var target in CasterUnits())
                    if(target.health>.405&&AreEnemies(cast.owner,target.ownerSlot)&&!CasterHasType(target,"structure")&&
                        !CasterMagicImmune(target)&&SquaredDistance(cast.point,target.position)<=220*220)
                        ApplyTriggeredHit(cast.actor,cast.owner,target,300,OriginalTriggeredDamageMode.SpellMagic);
            }
            foreach(int id in new List<int>(itemSummonBonuses.Keys))if(world.UnitState(id)==null)itemSummonBonuses.Remove(id);
            AdvanceItemOrbLandings();
            AdvanceAssimilatedOrders();
        }
        void AdvanceAssimilatedOrders()
        {
            if(world.Clock+1e-9<nextAssimilatedOrderScan)return;
            nextAssimilatedOrderScan=world.Clock+.2;
            // H7 grants these two native spells, but exports no autonomous AI
            // schedule. This owner-specific .2s nearest-target scan is a host
            // policy. Reuse admission/castpoint/cooldown checks; adding spells
            // never grants mana or invents a missing castpoint for the raw unit.
            foreach(int id in new List<int>(itemSummonBonuses.Keys))
            {
                var actor=world.UnitState(id);
                if(actor==null||actor.kind!=OriginalWorldUnitKind.Summon||actor.ownerSlot<1||actor.ownerSlot>8)continue;
                foreach(string ability in new[]{"A08U","A08T"})
                {
                    if(!HasEffectiveUnitAbility(actor,ability))continue;
                    OriginalWorldUnitView selected=null;double nearest=double.PositiveInfinity;
                    foreach(var target in world.Snapshot().units)
                        if(OrdinarySpellTarget(actor,target,ability)&&CanSeeForCombat(actor.ownerSlot,target))
                        {
                            double distance=SquaredDistance(actor.position,target.position);
                            if(distance<nearest){nearest=distance;selected=target;}
                        }
                    if(selected!=null&&TryStartOrdinaryNativeSpell(id,ability,selected.entityId))break;
                }
            }
        }
    }
}
