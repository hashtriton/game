using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        NativeItemActionRule SourceNativeItemRule(string item)
        {
            if(item!="I0AL"&&item!="I04B")return null;
            string id=item=="I0AL"?"A1DZ":"A06W";
            var a=combatCatalog.Ability(id);var definition=itemCatalog.Item(item);
            double cost=item=="I0AL"?125:250,cooldown=item=="I0AL"?18:25;
            if(definition==null||Array.IndexOf(definition.abilityIds,id)<0||
                a.Text("code")!=(item=="I0AL"?"ANdh":"AOwk")||a.Number("Cost1")!=cost||a.Number("Cool1")!=cooldown)
                throw new InvalidOperationException("Source item activation declaration changed.");
            if(item=="I0AL")
            {
                if(a.Number("DataA1")!=8||a.Number("Dur1")!=3||a.Number("HeroDur1")!=3||a.Number("Rng1")!=700||a.Text("BuffID1")!="B0CT")
                    throw new InvalidOperationException("Void haze declaration changed.");
                return new NativeItemActionRule{abilityId=id,cooldownGroup=definition.cooldownId,requiresCharge=false,
                    targetMode=OriginalAbilityTargetMode.Unit,range=700,manaCost=cost,cooldown=cooldown,sourceEffect="void"};
            }
            var helper=combatCatalog.Ability("A05V");
            if(helper.Text("code")!="ANsi"||helper.Number("DataA1")!=15||helper.Number("Area1")!=350||
                helper.Number("Dur1")!=4||helper.Number("HeroDur1")!=4||helper.Text("BuffID1")!="B040")
                throw new InvalidOperationException("Silence helper declaration changed.");
            return new NativeItemActionRule{abilityId=id,cooldownGroup=definition.cooldownId,requiresCharge=false,
                manaCost=cost,cooldown=cooldown,sourceEffect="silence"};
        }

        void ApplySourceNativeItemAction(OriginalWorldUnitView actor,OriginalSessionCommand command,NativeItemActionRule rule)
        {
            // These source callbacks use immediate host activation. Native
            // projectile/cast latency for the exact item aliases is unmeasured.
            // DataA masks transfer from measured ANsi8/15; absent ANdh B/C/D
            // are not promoted to measured neutral miss/movement/IAS axes.
            if(rule.sourceEffect=="void")
            {
                var target=world.UnitState(command.targetId);if(target==null)return;
                SetActorControl(target.entityId,"item-haze:B0CT",OriginalActorControlMask.Cast,3,true,true);
                // xu/OL/ ZT10245: native B06X suppresses only this source
                // handler. Debit the current MP first, then hL mode3.
                if(HasEffectiveUnitAbility(target,"B06X"))return;
                double burn=Math.Min(300,target.mana);
                if(burn<=0)return;
                world.UpdateProfile(target.entityId,target.profile,target.health,target.mana-burn);
                ApplyTriggeredHit(actor.entityId,actor.ownerSlot,world.UnitState(target.entityId),burn*1.5,OriginalTriggeredDamageMode.ChaosUniversal);
                return;
            }
            // vEv22443: a helper at the caster point casts A05V. The helper
            // owns the native spell; it does not emit another hero hK event.
            foreach(var target in world.Snapshot().units)
                if(target.health>.405&&!target.hidden&&!target.invulnerable&&AreEnemies(actor.ownerSlot,target.ownerSlot)&&
                    !CasterHasType(target,"mechanical")&&!CasterMagicImmune(target)&&
                    WeaponTargetTypeAllowed(combatCatalog.Ability("A05V").Text("targs1"),combatCatalog.Unit(target.rawcode).Text("targType"))&&
                    SquaredDistance(actor.position,target.position)<=350*350)
                    SetActorControl(target.entityId,"item-silence:B040",OriginalActorControlMask.Cast|OriginalActorControlMask.Weapon,4,true,true);
        }
    }
}
