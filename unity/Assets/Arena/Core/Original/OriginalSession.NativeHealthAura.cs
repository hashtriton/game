using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly Dictionary<int,double> nativeHealthAuraUntil=new Dictionary<int,double>();
        double nextNativeHealthAuraScan=.5;
        void AdvanceNativeHealthAuras()
        {
            if(world.Clock+1e-9<nextNativeHealthAuraScan)return;
            nextNativeHealthAuraScan=world.Clock+.5;
            var ability=combatCatalog.Ability("A11G");
            if(ability.Text("code")!="Aabr"||ability.Text("BuffID1")!="B095"||ability.Number("Area1")!=200||
                ability.Number("DataA1")!=.004||ability.Text("targs1")!="air,friend,ground,notself,neutral,invulnerable,organic,vulnerable")
                throw new InvalidOperationException("Galvanization aura declaration changed.");
            // HEALTHAURA5 campaign02df2d5c: B095 on330/1080HP undead,
            // equal .004 FLAT HP/s; nonundead and300WC controls are negative.
            // The source is the effective per-unit ability list, so D3's n01X
            // removal remains authoritative. The stock Aabr failed to add and
            // contributes no positive evidence or inherited percent flag.
            // Scan.5, linger3, same-buff nonstacking, paused eligibility and
            // hidden/dead source exclusion are explicit host reconstruction.
            var units=world.Snapshot().units;
            foreach(var source in units)
            {
                if(source.health<=.405||source.hidden||source.kind==OriginalWorldUnitKind.Illusion||!HasEffectiveUnitAbility(source,"A11G"))continue;
                foreach(var target in units)
                    if(target.entityId!=source.entityId&&target.health>.405&&!target.hidden&&!AreEnemies(source.ownerSlot,target.ownerSlot)&&
                        CasterHasType(target,"undead")&&!CasterHasType(target,"mechanical")&&
                        WeaponTargetTypeAllowed(ability.Text("targs1"),combatCatalog.Unit(target.rawcode).Text("targType"))&&
                        SquaredDistance(source.position,target.position)<=200*200)
                        nativeHealthAuraUntil[target.entityId]=world.Clock+3;
            }
            foreach(int id in new List<int>(nativeHealthAuraUntil.Keys))
            {
                var target=world.UnitState(id);
                if(target==null||target.health<=.405||nativeHealthAuraUntil[id]<=world.Clock+1e-9)nativeHealthAuraUntil.Remove(id);
            }
        }
        double NativeHealthAuraRate(OriginalWorldUnitView target) =>
            target!=null&&target.health>.405&&nativeHealthAuraUntil.TryGetValue(target.entityId,out double until)&&until>world.Clock+1e-9?.004:0;
    }
}
