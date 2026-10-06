using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemShellBuff { internal int actor; internal string buff; internal double remaining; }
        sealed class ItemLightningPulse { internal int actor,owner,carrier,ticks; internal string buff; internal double elapsed,next=1,damage; }
        readonly Dictionary<int,double> itemAntiMagicShells=new Dictionary<int,double>();
        readonly List<ItemShellBuff> itemLightningBuffs=new List<ItemShellBuff>();
        readonly List<ItemLightningPulse> itemLightningPulses=new List<ItemLightningPulse>();

        NativeItemActionRule ShellItemRule(string itemId)
        {
            bool anti=itemId=="I01Y";if(!anti&&itemId!="I01B"&&itemId!="I07M")return null;
            string id=anti?"AIxs":itemId=="I01B"?"A07W":"A16M";
            string buff=anti?"Bams,Bam2":itemId=="I01B"?"B00O,B00P":"B0AH,B00P";
            double cost=anti?0:itemId=="I01B"?350:450;
            var a=combatCatalog.Ability(id);var item=itemCatalog.Item(itemId);
            if(Array.IndexOf(item.abilityIds,id)<0||a.Text("code")!=(anti?"Aami":"Alsh")||a.Text("BuffID1")!=buff||
                a.Number("Dur1")!=(anti?6:8)||a.Number("HeroDur1")!=(anti?6:8)||a.Number("Cool1")!=(anti?18:25))
                throw new InvalidOperationException("Item shell declaration changed.");
            if(anti)
            {
                bool present=Array.Exists(a.fields,f=>f.key=="Cost1")||Array.Exists(a.overrides,f=>f.field=="amcs"&&f.level==1);
                if(a.Number("DataB1")!=100||present&&(!a.TryNumber("Cost1",out double v,out _)||v!=0))
                    throw new InvalidOperationException("AIxs conflicts with ITEMSHELL2.");
            }
            else if(a.Number("Cost1")!=cost||a.Number("Rng1")!=800||a.Text("targs1")!="player,enemies,self,allies")
                throw new InvalidOperationException("Lightning shell declaration changed.");
            // ITEMSHELL2 cf6553fc43ab: instant0/350/450 MP, charge onlyAIxs.
            return new NativeItemActionRule{abilityId=id,cooldownGroup=item.cooldownId,manaCost=cost,cooldown=anti?18:25,
                requiresCharge=anti,targetMode=anti?OriginalAbilityTargetMode.None:OriginalAbilityTargetMode.Unit,
                range=anti?0:800,sourceEffect=anti?"anti-magic-shell":"lightning-shell"};
        }
        OriginalSessionReplyCode ValidateLightningItemTarget(Player player,OriginalSessionCommand command,bool enforceRange=true)
        {
            var actor=world.UnitState(player.slot);var target=world.UnitState(command.targetId);
            // Native self control is measured; extending the declared
            // player/enemies/self/allies mask to other living carriers is derived.
            if(command.targetKind!=OriginalWorldTargetKind.Unit||target==null||target.health<=.405||target.invulnerable||
                !CanSeeForCombat(actor.ownerSlot,target)||CasterMagicImmune(target)||enforceRange&&SquaredDistance(actor.position,target.position)>800*800)
                return OriginalSessionReplyCode.InvalidCommand;
            return OriginalSessionReplyCode.Accepted;
        }
        bool ItemAntiMagicShellActive(int actor) => itemAntiMagicShells.TryGetValue(actor,out double seconds)&&seconds>0;
        bool HasItemLightningBuff(int actor,string buff)
        {
            var unit=world.UnitState(actor);if(unit==null||unit.health<=.405)return false;
            foreach(var entry in itemLightningBuffs)if(entry.actor==actor&&entry.buff==buff&&entry.remaining>0)return true;
            return false;
        }
        void BeginItemShell(OriginalWorldUnitView actor,int targetId,NativeItemActionRule rule)
        {
            if(rule.sourceEffect=="anti-magic-shell"){itemAntiMagicShells[actor.entityId]=6;return;}
            var target=world.UnitState(targetId);if(target==null||target.health<=.405)return;
            string buff=rule.abilityId=="A07W"?"B00O":"B0AH";
            itemLightningBuffs.RemoveAll(x=>x.actor==targetId&&x.buff==buff);
            itemLightningBuffs.Add(new ItemShellBuff{actor=targetId,buff=buff,remaining=8});
            // T8 tests OL/B06X only for the source U8 timer. The native buff
            // still applies. Each source cast retains its own eight callbacks.
            if(HasEffectiveUnitAbility(target,"B06X"))return;
            itemLightningPulses.Add(new ItemLightningPulse{actor=actor.entityId,owner=actor.ownerSlot,carrier=targetId,
                buff=buff,damage=rule.abilityId=="A07W"?150:200});
        }
        void AdvanceItemShells(double seconds)
        {
            if(!OriginalCombatDefinition.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
            while(seconds>1e-9){double step=Math.Min(.05,seconds);AdvanceItemShellStep(step);seconds-=step;}
        }
        void AdvanceItemShellStep(double seconds)
        {
            foreach(int id in new List<int>(itemAntiMagicShells.Keys))
            {
                var u=world.UnitState(id);if(u==null||u.health<=.405){itemAntiMagicShells.Remove(id);continue;}
                if(u.paused)continue;itemAntiMagicShells[id]-=seconds;if(itemAntiMagicShells[id]<=1e-9)itemAntiMagicShells.Remove(id);
            }
            // Source U8 survives caster and carrier death, but not removal of
            // the retained carrier handle. Buff expiry/pause and multiple
            // simultaneous casts use explicit host policies, not native proof.
            foreach(var pulse in itemLightningPulses.ToArray())
            {
                pulse.elapsed+=seconds;
                while(pulse.next<=pulse.elapsed+1e-9)
                {
                    pulse.next+=1;var carrier=world.UnitState(pulse.carrier);
                    if(pulse.ticks++>=8||carrier==null){itemLightningPulses.Remove(pulse);break;}
                    foreach(var victim in world.Snapshot().units)
                        if(victim.health>.405&&AreEnemies(pulse.owner,victim.ownerSlot)&&!CasterHasType(victim,"structure")&&
                            !CasterMagicImmune(victim)&&!HasItemLightningBuff(victim.entityId,pulse.buff)&&
                            SquaredDistance(carrier.position,victim.position)<=275*275)
                            ApplyTriggeredHit(pulse.actor,pulse.owner,victim,pulse.damage,OriginalTriggeredDamageMode.SpellMagic);
                }
            }
            foreach(var buff in itemLightningBuffs.ToArray())
            {
                var u=world.UnitState(buff.actor);if(u==null||u.health<=.405){itemLightningBuffs.Remove(buff);continue;}
                if(u.paused)continue;buff.remaining-=seconds;if(buff.remaining<=1e-9)itemLightningBuffs.Remove(buff);
            }
        }
    }
}
