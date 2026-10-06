using System;
using System.Collections.Generic;

namespace Arena.Original
{
    // 3.9c Oqv/OPv34551, OSv/Otv/OTv/Ouv/OUv/Owv34706 and I9v/Aev/Axv36958.
    // Aura refresh/linger and sparse native numeric defaults are host policies,
    // not measured defaults. Source timers and explicit DataA remain exact rules.
    public sealed class OriginalCasterFieldRules
    {
        public readonly string abilityId, helper, buff;
        public readonly double radius, movementBonus;
        public readonly int activeCallbacks;
        public OriginalCasterFieldRules(OriginalCombatCatalog catalog,string id)
        {
            if(catalog==null||catalog.sourceSha256!=OriginalNativeCatalog.ExpectedMapSha256)
                throw new InvalidOperationException("caster-field-source-unavailable");
            abilityId=id;
            if(id=="A123")
            {
                helper="n06L"; radius=4000; activeCallbacks=14;
                var unit=catalog.Unit(helper);
                if(unit==null||unit.Number("HP")!=20||unit.Number("spd")!=1||unit.Text("targType")!="ward"||
                    unit.Text("abilList")!="A0K4,ACmi")throw new InvalidOperationException("totem-profile-conflict");
                return;
            }
            string native;
            switch(id)
            {
                case "A1DC":native="S002";helper="h01O";buff="B0CN";radius=400;movementBonus=-.5;break;
                case "A1DE":native="S003";helper="h05X";buff="B0CO";radius=500;break;
                case "A1DF":native="S004";helper="n02U";buff="B0CP";radius=500;break;
                default:throw new InvalidOperationException("unknown-caster-field");
            }
            activeCallbacks=10;
            var aura=catalog.Ability(native);
            if(aura==null||aura.Text("code")!="AOae"||aura.Number("levels")!=1||aura.Number("Area1")!=radius||
                aura.Text("BuffID1")!=buff||aura.Text("targs1")!="air,enemies,ground")
                throw new InvalidOperationException("caster-aura-declaration-conflict");
            foreach(var key in new[]{"DataA1","DataB1"})
            {
                double expected=key=="DataA1"?movementBonus:0;
                if(aura.TryNumber(key,out var value,out var state)?value!=expected:state!=null||expected!=0)
                    throw new InvalidOperationException("caster-aura-modifier-conflict:"+key);
            }
        }
        static readonly HashSet<string> excludedSpells=new HashSet<string>(StringComparer.Ordinal){
            "A1BE","A1BX","A1BY","A1BZ","A1AP","A158","A12G","A0MN","A10U","A0QG","A0Y4","A0KD","A107","A0WO","A0WP",
            "A0YK","A0W2","A0PO","A0QN","A0SW","A0SO","A0SN","A0K0","A082","A0QB","A0RK","A02M","A040","A05L","A05Q",
            "A02Q","A08F","A0NS","A0MZ","A0LU","A10P","A10Q","A05M","A0N5","A0PG","A0JW","A0PX","A0U2"};
        static readonly HashSet<string> excludedUnits=new HashSet<string>(StringComparer.Ordinal){
            "h01O","h015","h04J","h046","e00M","h00G","u00M","h01H","n03W","e00P","n02Z","n02Y","n02X","e00O"};
        public static bool SpellTriggersCurse(string ability,string unit) => ability!=null&&ability.Length==4&&
            !excludedSpells.Contains(ability)&&!excludedUnits.Contains(unit);
    }
}
