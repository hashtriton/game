using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalItemSummonScriptRules itemOrbRule,itemFingerRule;
        sealed class ItemOrbLanding { internal int actor,owner;internal OriginalPoint point;internal double due; }
        readonly List<ItemOrbLanding> itemOrbLandings=new List<ItemOrbLanding>();
        OriginalItemSummonScriptRules ItemSummonScriptRule(string id)=>id=="I00Z"
            ?itemOrbRule??(itemOrbRule=new OriginalItemSummonScriptRules(itemCatalog,combatCatalog)):id=="I049"
            ?itemFingerRule??(itemFingerRule=new OriginalItemSummonScriptRules(itemCatalog,combatCatalog,id)):null;
        OriginalSessionReplyCode ValidateItemSummonScriptTarget(Player player,OriginalSessionCommand command,OriginalItemSummonScriptRules rule,bool enforceRange=true)
        {
            if(rule.itemId=="I049")return ValidateItemFingerTarget(player,command,rule,enforceRange);
            if(command.targetKind!=OriginalWorldTargetKind.None||command.targetId!=0)return OriginalSessionReplyCode.InvalidCommand;
            var actor=world.UnitState(player.slot);
            return actor!=null&&ValidPoint(command.x,command.y)&&(!enforceRange||SquaredDistance(actor.position,new OriginalPoint(command.x,command.y))<=rule.range*rule.range)
                ?OriginalSessionReplyCode.Accepted:OriginalSessionReplyCode.NotReady;
        }
        OriginalSessionReplyCode UseItemSummonScript(Player player,OriginalSessionCommand command,OriginalItemSummonScriptRules rule)
        {
            if(rule.itemId=="I049")return UseItemFinger(player,command,rule);
            var valid=ValidateItemSummonScriptTarget(player,command,rule);if(valid!=OriginalSessionReplyCode.Accepted)return valid;
            if(nextItemSummonId>OriginalWorld.LastSummonEntityId||!TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic))
                return OriginalSessionReplyCode.RuleUnavailable;
            var point=new OriginalPoint(command.x,command.y);
            // SUMSCRIPT2 measured birth vitals. Collision/placement and60s life
            // use declared values; the four-second observer did not prove life.
            var profile=new OriginalWorldUnitProfile{maxHealth=1800,maxMana=0,moveSpeed=320,collisionRadius=32};
            if(!FindItemSummonPoint(point,32,Array.Empty<OriginalWorldSummonSpawn>(),0,out var position))return OriginalSessionReplyCode.NotReady;
            if(!world.TrySpendMana(player.slot,rule.manaCost))return OriginalSessionReplyCode.NotReady;
            itemCooldowns[ItemCooldownKey(player.slot,rule.cooldownGroup)]=ItemClock+rule.cooldown;player.lastItemAction=OriginalItemActionCode.Success;
            NotifyNativeSpellEffect(player.slot,rule.abilityId);BeginItemOrbSource(player.slot,point);
            // Nested SPELL_EFFECT may kill the canonical caster. The accepted
            // source effect survives; if identity/body vanished, native creation
            // is represented by an accepted fizzle, without a partial rejection.
            if(nextItemSummonId>OriginalWorld.LastSummonEntityId||
                !FindItemSummonPoint(point,32,Array.Empty<OriginalWorldSummonSpawn>(),0,out position))return OriginalSessionReplyCode.Accepted;
            var row=new OriginalWorldSummonSpawn{entityId=nextItemSummonId,ownerSlot=player.slot,sourceHeroEntityId=player.slot,
                rawcode="n01S",profile=profile,position=position,health=1800,mana=0};
            if(!world.TryPublishSourceSummons(new[]{row}))return OriginalSessionReplyCode.Accepted;
            nextItemSummonId++;itemSummons.Add(row.entityId,new ItemSummonState{remaining=60});world.SetVisibility(row.entityId,false);
            itemOrbLandings.Add(new ItemOrbLanding{actor=row.entityId,owner=player.slot,point=point,due=world.Clock+1});
            return OriginalSessionReplyCode.Accepted;
        }
        void AdvanceItemOrbLandings()
        {
            foreach(var cast in itemOrbLandings.ToArray())
            {
                if(cast.due>world.Clock+1e-9)continue;itemOrbLandings.Remove(cast);
                var summon=world.UnitState(cast.actor);if(summon==null||summon.health<=.405)continue;
                // Revealing at1s uses authored impact time inside the observed
                // (1,1.01] visibility bracket. Native displacement is not copied.
                if(!world.TryFindFreeSpawn(cast.point,summon.profile.collisionRadius,512,out var position)||
                    !world.RelocateStoredPosition(cast.actor,position))continue;
                world.SetVisibility(cast.actor,true);
                foreach(var target in world.Snapshot().units)
                {
                    // Declared ground mask, radius+body host frontier. The
                    // measured row proves the central hero only, not every edge.
                    double radius=220+target.profile.collisionRadius;
                    if(target.health<=.405||target.hidden||target.invulnerable||!AreEnemies(cast.owner,target.ownerSlot)||
                        CasterMagicImmune(target)||combatCatalog.Unit(target.rawcode).Text("movetp")=="fly"||
                        SquaredDistance(cast.point,target.position)>radius*radius)continue;
                    ApplyResolvedUnitHit(cast.actor,cast.owner,target,0);
                    var live=world.UnitState(target.entityId);if(live==null||live.health<=.405)continue;
                    string token="item-orb-stun:"+cast.actor;
                    if(SetActorControl(live.entityId,token,AllActorControls,0,true,true))
                        nativeBashes.Add(new NativeBashState{target=live.entityId,source=cast.actor,owner=cast.owner,token=token,remaining=2.7});
                }
            }
        }
    }
}
