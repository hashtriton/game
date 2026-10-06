using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemScriptDebuff { internal string ability;internal double updated,remaining,move,slow,miss,pulse;internal int owner; }
        readonly Dictionary<int,Dictionary<string,ItemScriptDebuff>> itemScriptDebuffs=new Dictionary<int,Dictionary<string,ItemScriptDebuff>>();

        void BeginItemSoulBurn(int actorId,int owner)
        {
            var actor=world.UnitState(actorId);
            if(actor==null||actor.health<=.405||actor.invulnerable||CasterMagicImmune(actor))return;
            // G8's h011 lasts1s via RemoveUnit. ITEMEX2 proves the native
            // B010 effect survives that helper lifetime:8s, +20%MS/+200%IAS,
            // cast-only disable and eight damage0 callbacks starting+.01.
            // Latest refresh, pause freezing and mixed modifier sums are host
            // policies. Item blocking was not measured and is not inferred.
            CaptureAbilityMovementBase(actorId);
            if(!itemScriptDebuffs.TryGetValue(actorId,out var buffs))itemScriptDebuffs[actorId]=buffs=new Dictionary<string,ItemScriptDebuff>();
            buffs["B010"]=new ItemScriptDebuff{ability="A0VG",updated=world.Clock,remaining=8,move=.2,slow=-2,pulse=.01,owner=owner};
            SetActorControl(actorId,"item-soulburn:B010",OriginalActorControlMask.Cast,0,true,true);
            RefreshAbilityMovement(actorId);RescaleWeaponRate(actorId,world.Clock);
        }

        void ApplyItemScriptAreaDebuff(OriginalWorldUnitView actor,string ability,double radius)
        {
            foreach(var target in CasterUnits())if(ItemActEnemy(actor,target,radius,true,true))ApplyItemScriptDebuff(target,ability);
        }
        void ApplyItemScriptDebuff(OriginalWorldUnitView target,string ability)
        {
            target=target==null?null:world.UnitState(target.entityId);
            if(target==null||target.health<=.405||target.invulnerable||CasterMagicImmune(target))return;
            var a=combatCatalog.Ability(ability);bool silence=a.Text("code")=="ANsi";
            if(silence&&(CasterHasType(target,"mechanical")||CasterHasType(target,"structure")))return;
            double slow=0;
            // Missing Aslo DataB1 and ANsi disable-mask are not asserted zero.
            // The playable host profile applies the declared axes only; helper
            // effect arrival, independent buff stacking and latest refresh are
            // derived family policies, not per-rawcode native measurements.
            if(!silence)a.TryNumber("DataB1",out slow,out _);
            var state=new ItemScriptDebuff{ability=ability,updated=world.Clock,
                remaining=a.Number(IsNativeHeroPredicate(target)?"HeroDur1":"Dur1"),
                move=silence?0:-a.Number("DataA1"),slow=slow,miss=silence?a.Number("DataB1"):0};
            CaptureAbilityMovementBase(target.entityId);
            if(!itemScriptDebuffs.TryGetValue(target.entityId,out var buffs))itemScriptDebuffs[target.entityId]=buffs=new Dictionary<string,ItemScriptDebuff>();
            buffs[a.Text("BuffID1")]=state;RefreshAbilityMovement(target.entityId);RescaleWeaponRate(target.entityId,world.Clock);
        }
        double ItemScriptDebuffValue(int actor,int field)
        {
            double value=0;if(itemScriptDebuffs.TryGetValue(actor,out var buffs))foreach(var state in buffs.Values)
                value+=field==0?state.move:field==1?state.slow:state.miss;
            return value;
        }
        double ItemScriptDebuffMovementBonus(int actor)=>ItemScriptDebuffValue(actor,0);
        double ItemScriptDebuffAttackSlow(int actor)=>ItemScriptDebuffValue(actor,1);
        double ItemScriptDebuffMissChance(int actor)=>Math.Min(1,ItemScriptDebuffValue(actor,2));
        void RemoveItemScriptDebuffs(int actor)
        {
            if(!itemScriptDebuffs.Remove(actor))return;
            ClearActorControl(actor,"item-soulburn:B010");
            ClearActorControl(actor,"item-soulburn:B0A1");
            RefreshAbilityMovement(actor);RescaleWeaponRate(actor,world.Clock);ReleaseAbilityMovementBase(actor);
        }
        void AdvanceItemScriptDebuffs()
        {
            foreach(var pair in new List<KeyValuePair<int,Dictionary<string,ItemScriptDebuff>>>(itemScriptDebuffs))
            {
                var actor=world.UnitState(pair.Key);if(actor==null||actor.health<=.405){RemoveItemScriptDebuffs(pair.Key);continue;}
                bool changed=false;
                foreach(var entry in new List<KeyValuePair<string,ItemScriptDebuff>>(pair.Value))
                {
                    var state=entry.Value;double elapsed=Math.Max(0,world.Clock-state.updated);state.updated=world.Clock;
                    if(actor.paused)continue;
                    double activeElapsed=Math.Min(elapsed,state.remaining);
                    state.remaining-=elapsed;
                    if(state.ability=="A0VG"||state.ability=="A159")
                    {
                        state.pulse-=activeElapsed;
                        while(state.pulse<=1e-9)
                        {
                            state.pulse+=1;var current=world.UnitState(pair.Key);
                            if(current==null||current.health<=.405)break;
                            ApplyResolvedUnitHit(0,state.owner,current,0);
                            if(!itemScriptDebuffs.TryGetValue(pair.Key,out var live)||!live.TryGetValue(entry.Key,out var same)||!ReferenceEquals(same,state))break;
                        }
                    }
                    if(state.remaining<=1e-9){pair.Value.Remove(entry.Key);if(state.ability=="A0VG")ClearActorControl(pair.Key,"item-soulburn:B010");if(state.ability=="A159")ClearActorControl(pair.Key,"item-soulburn:B0A1");changed=true;}
                }
                if(changed){if(pair.Value.Count==0)itemScriptDebuffs.Remove(pair.Key);RefreshAbilityMovement(pair.Key);RescaleWeaponRate(pair.Key,world.Clock);ReleaseAbilityMovementBase(pair.Key);}
            }
        }

        List<OriginalWorldUnitView> ItemCrownTargets(OriginalWorldUnitView actor)
        {
            var result=new List<OriginalWorldUnitView>();if(actor==null)return result;
            foreach(var target in CasterUnits())if(target.health>.405&&!CasterHasType(target,"structure")&&
                !HasEffectiveUnitAbility(target,"A0X5")&&!HasEffectiveUnitAbility(target,"A0X6")&&!HasEffectiveUnitAbility(target,"A0K4")&&
                SquaredDistance(actor.position,target.position)<=600*600)result.Add(target);
            return result;
        }
        void BeginItemCrown(ItemScriptAct effect,OriginalWorldUnitView actor)
        {
            var targets=ItemCrownTargets(actor);double sum=0;int count=0;
            foreach(var target in targets)if(SourceUnitUserData(target.entityId)!=2&&target.rawcode!="O006")
            {sum+=target.health/target.profile.maxHealth;count++;}
            if(count==0)return;double mean=sum/count;
            foreach(var previous in targets)
            {
                var target=world.UnitState(previous.entityId);if(target==null)continue;
                effect.hit.Add(target.entityId);bool ally=!AreEnemies(actor.ownerSlot,target.ownerSlot);
                if(ally||SourceUnitUserData(target.entityId)!=2&&target.rawcode!="O006")
                    world.UpdateProfile(target.entityId,target.profile,target.profile.maxHealth*mean,target.mana);
                // Exact book children: AbilityData.slk25932..25955 DataA1
                // A0X7->A0X4 and A0X8->A0X3. No inherited default child list.
                ApplyUnitAbilityOverlay(target.entityId,ally?new[]{"A0X7","A0X4"}:new[]{"A0X6","A0X8","A0X3"},null);
                if(ally)AddItemSourceShield(target.entityId,350,8,1,"A0X5");
                else ApplyTriggeredHit(effect.actor,effect.owner,world.UnitState(target.entityId),350,OriginalTriggeredDamageMode.SpellMagic);
            }
            effect.period=8;effect.due=world.Clock+8;itemScriptActs.Add(effect);
        }

        void ObserveItemScriptActWeapon(int attacker,OriginalWorldUnitView target,double damage,bool primaryWeapon)
        {
            var actor=world.UnitState(attacker);
            if(!primaryWeapon||damage<=0||actor==null||actor.kind!=OriginalWorldUnitKind.Hero||target==null||target.paused||
                !AreEnemies(actor.ownerSlot,target.ownerSlot)||HasEffectiveUnitAbility(target,"A0K4")||
                combatCatalog.Unit(actor.rawcode).Text("weapTp1")=="normal")return;
            bool active=false;foreach(var effect in itemScriptActs)if(effect.actor==attacker&&effect.ability=="A0T5"&&effect.due>world.Clock)active=true;
            if(!active||!Array.Exists(PlayerAt(actor.ownerSlot).inventory.HeroSlots,item=>item?.itemId=="I070"))return;
            double amount=ScriptedAbilityAttack(PlayerAt(actor.ownerSlot));amount=(amount<=0?50:amount)*.6;int count=0;
            foreach(var recipient in CasterUnits())if(ItemActEnemy(actor,recipient,900,false,true)&&!recipient.hidden&&
                !HasEffectiveUnitAbility(recipient,"A0K4")&&SquaredDistance(actor.position,recipient.position)>50*50)
            {
                itemScriptActs.Add(new ItemScriptAct{ability="A0T5_arrow",actor=attacker,owner=actor.ownerSlot,target=recipient.entityId,
                    amount=amount,origin=actor.position,point=actor.position,period=.03,due=world.Clock+.03});
                if(++count==3)break;
            }
            // This covers the guaranteed B06W active. SQ's separate passive
            // qM(.03802) accumulator is not fabricated by this active driver.
        }

        bool TickExtendedItemScriptAct(ItemScriptAct effect,out bool retained)
        {
            retained=false;var actor=world.UnitState(effect.actor);
            switch(effect.ability)
            {
                case "A1BM":
                    if(actor==null||actor.health<.405||effect.ticks<=0){ObserveScriptedHelperDeath();return true;}
                    double angle=effect.second*12*.0174532;
                    effect.point=new OriginalPoint(actor.position.x+185*Math.Cos(angle),actor.position.y+185*Math.Sin(angle));
                    foreach(var target in CasterUnits())if(ItemActEnemy(actor,target,double.MaxValue,false,true)&&
                        SquaredDistance(effect.point,target.position)<=115*115&&effect.hit.Add(target.entityId))
                    {ApplyTriggeredHit(0,effect.owner,target,effect.amount,OriginalTriggeredDamageMode.SpellNormal);ApplyItemScriptDebuff(world.UnitState(target.entityId),"A0OR");}
                    effect.second++;effect.ticks--;itemScriptActPulses[effect.actor+":"+effect.ability]=(int)effect.second;retained=true;return true;
                case "A0QQ":
                    if(actor==null||effect.ticks--<=0)return true;
                    ApplyItemScriptAreaDebuff(actor,"A0WJ",250);itemScriptActPulses[effect.actor+":"+effect.ability]++;retained=true;return true;
                case "A1CT":ObserveScriptedHelperDeath();return true;
                case "A1CU":
                    if(actor==null||actor.health<=.405||effect.ticks--<=0)return true;
                    foreach(var target in CasterUnits())if(ItemActEnemy(actor,target,400,true))
                    {ItemActHit(effect,target,40,OriginalTriggeredDamageMode.SpellMagic);ApplyItemScriptDebuff(world.UnitState(target.entityId),"A1CZ");}
                    retained=true;return true;
                case "A0KQ":
                    foreach(int id in effect.hit)if(world.UnitState(id)!=null)
                        ApplyUnitAbilityOverlay(id,null,new[]{"A0X5","A0X6","A0X7","A0X8","A0X3","A0X4"});
                    return true;
                case "A0T5":return true;
                case "A0T5_arrow":
                    var victim=world.UnitState(effect.target);if(victim==null){ObserveScriptedHelperDeath();return true;}
                    effect.second+=24;double dx=victim.position.x-effect.origin.x,dy=victim.position.y-effect.origin.y;
                    double distance=Math.Sqrt(dx*dx+dy*dy),heading=Math.Atan2(dy,dx)*57.2958*.0174532;
                    effect.point=new OriginalPoint(effect.origin.x+effect.second*Math.Cos(heading),effect.origin.y+effect.second*Math.Sin(heading));
                    if(effect.second>=distance){ApplyTriggeredHit(0,effect.owner,victim,effect.amount,OriginalTriggeredDamageMode.SpellNormal);ObserveScriptedHelperDeath();return true;}
                    retained=true;return true;
                default:return false;
            }
        }
        void AppendItemScriptActVisuals(List<OriginalVisualEffectView> output)
        {
            foreach(var effect in itemScriptActs)
            {
                var actor=world.UnitState(effect.actor);if(actor==null)continue;
                bool projectile=effect.ability=="A1BM"||effect.ability=="A0T5_arrow";
                string ability=effect.ability=="A0T5_arrow"?"A0T5":effect.ability=="A16O_wave"?"A16O":effect.ability;
                if(!projectile&&ability!="A0QQ"&&effect.ability!="A16O_wave"&&ability!="A1CU")continue;
                output.Add(new OriginalVisualEffectView{kind=projectile?OriginalVisualEffectKind.Orb:OriginalVisualEffectKind.ActiveCircle,
                    abilityId=ability,sourceEntityId=effect.actor,position=projectile?effect.point:actor.position,
                    radius=projectile?30:ability=="A0QQ"?250:effect.ability=="A16O_wave"?70*Math.Max(1,effect.ticks-1):400,progress=1});
            }
        }
    }
}
