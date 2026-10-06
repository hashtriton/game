using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemRegenState { internal double phase; internal int pulses; }
        readonly Dictionary<int,ItemRegenState> itemRegeneration=new Dictionary<int,ItemRegenState>();

        NativeItemActionRule RegenerationItemRule()
        {
            var item=itemCatalog.Item("I0AJ");var a=combatCatalog.Ability("A18H");
            if(item==null||Array.IndexOf(item.abilityIds,"A18H")<0||a.Text("code")!="AIrg"||
                a.Number("DataB1")!=1||a.Number("DataD1")!=1||a.Number("DataE1")!=1||
                a.Number("Dur1")!=10||a.Number("HeroDur1")!=10||a.Text("BuffID1")!="B0B1,B0B1,B0B1")
                throw new InvalidOperationException("Measured regeneration potion declaration changed.");
            foreach(string key in new[]{"Cost1","Cool1"})
                if(Array.Exists(a.fields,f=>f.key==key)&&(!a.TryNumber(key,out double value,out _)||value!=0))
                    throw new InvalidOperationException("Regeneration potion sparse native field conflicts.");
            // ITEMREGEN1 d67b21c5efac: ten .1 MP pulses, no extra HP and
            // immediate cost0. Cool0 transfers from native AIrg aliases;
            // same-buff refresh and pause freezing remain host policies.
            return new NativeItemActionRule{abilityId="A18H",cooldownGroup=item.cooldownId,status="B0B1",duration=10};
        }

        void AdvanceItemRegeneration(double seconds)
        {
            foreach(var pair in new List<KeyValuePair<int,ItemRegenState>>(itemRegeneration))
            {
                var actor=world.UnitState(pair.Key);
                if(actor==null||actor.health<=.405){itemRegeneration.Remove(pair.Key);continue;}
                if(actor.paused)continue;
                pair.Value.phase+=seconds;
                while(pair.Value.pulses<10&&pair.Value.phase+1e-9>=1)
                {
                    pair.Value.phase-=1;pair.Value.pulses++;
                    actor=world.UnitState(pair.Key);
                    world.UpdateProfile(pair.Key,actor.profile,actor.health,Math.Min(actor.profile.maxMana,actor.mana+.1));
                }
                if(pair.Value.pulses==10)itemRegeneration.Remove(pair.Key);
            }
        }

        // ITEMSTAT2 direct positive incoming damage removed B0B1. Zero native
        // events and rejected damage do not count as that measured boundary.
        void InterruptItemRegeneration(int actor) => itemRegeneration.Remove(actor);
    }
}
