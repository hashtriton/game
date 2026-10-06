using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class WaveExplosion { internal OriginalPoint position; internal double damage, due; internal int generation; }
        readonly HashSet<string> waveTraits = new HashSet<string>(StringComparer.Ordinal);
        readonly HashSet<int> waveTraitDeaths = new HashSet<int>();
        readonly List<WaveExplosion> waveExplosions = new List<WaveExplosion>();
        readonly Dictionary<int, HashSet<string>> waveDarkRanks = new Dictionary<int, HashSet<string>>();
        string[] randomWaveTraits = Array.Empty<string>();
        bool waveHelperActive;
        int waveTraitGeneration;
        uint waveTraitRandom;
        double nextWaveBloodlust, nextWaveSilence, nextWaveDark;

        // Ezv31956 and EWv31901, original3.9c. The helper lifetime ends in
        // Xvv32144..87. Its periodic triggers have global6/8/2s clocks; phase
        // is aligned to the host clock, not claimed as Warcraft timer replay.
        void OnWaveTraitMatchEvent(OriginalMatchEvent item)
        {
            if (item.kind == OriginalMatchEventKind.ShopAccess && !item.enabled && item.round % 5 != 0)
                BeginWaveTraits(item.round);
            else if (item.kind == OriginalMatchEventKind.PhaseChanged && item.amount != (int)OriginalMatchPhase.Combat)
                EndWaveTraits();
        }

        void BeginWaveTraits(int round)
        {
            EndWaveTraits(); waveTraitGeneration++; waveHelperActive = round % 5 != 0;
            if (!waveHelperActive) return;
            if (round == 1 || round == 12 || round == 19 || round == 27) waveTraits.Add("A15E");
            if (round == 9 || round == 16 || round == 18) waveTraits.Add("A15M");
            if (round == 12 || round == 18 || round == 23) waveTraits.Add("A15L");
            if (round == 7 || round == 11 || round == 14 || round == 27) waveTraits.Add("A15K");
            if (round == 4 || round == 7 || round == 21 || round == 28) waveTraits.Add("A15R");
            if (round == 24 || round == 29)
            {
                var choices = new List<string> { "A15E", "A15F", "A15I", "A15G", "A15H", "A15J", "A15K", "A15L", "A15M", "A15R" };
                randomWaveTraits = new string[round == 24 ? 3 : 4];
                for (int i = 0; i < randomWaveTraits.Length; i++)
                {
                    int selected = WaveTraitRoll(choices.Count);
                    randomWaveTraits[i] = choices[selected]; waveTraits.Add(choices[selected]);
                    choices[selected] = choices[choices.Count - 1]; choices.RemoveAt(choices.Count - 1);
                }
            }
            nextWaveBloodlust = (Math.Floor(world.Clock / 6) + 1) * 6;
            nextWaveSilence = (Math.Floor(world.Clock / 8) + 1) * 8;
            nextWaveDark = (Math.Floor(world.Clock / 2) + 1) * 2;
        }

        int WaveTraitRoll(int count)
        {
            // Independent deterministic stream is host policy. The source
            // algorithm samples without replacement; native RNG is not replayed.
            if (waveTraitRandom == 0) waveTraitRandom = unchecked((uint)seed) ^ 0xA151E571u;
            waveTraitRandom ^= waveTraitRandom << 13; waveTraitRandom ^= waveTraitRandom >> 17; waveTraitRandom ^= waveTraitRandom << 5;
            return (int)(waveTraitRandom % (uint)count);
        }

        void ApplyWaveSpawnAbilities(OriginalWorldUnitView actor)
        {
            if (waveHelperActive && actor?.kind == OriginalWorldUnitKind.Enemy && randomWaveTraits.Length != 0)
                ApplyUnitAbilityOverlay(actor.entityId, randomWaveTraits, Array.Empty<string>());
        }

        void EndWaveTraits()
        {
            if (waveHelperActive && waveTraits.Contains("A15R")) ApplyWaveDarkRanks(false);
            waveHelperActive = false; waveTraits.Clear(); randomWaveTraits = Array.Empty<string>();
            // Delayed nOv would use the removed MD. HELPER29 native null-source
            // hits are rejected; no later wave may revive that old helper.
            waveExplosions.Clear();
        }

        void ObserveWaveTraitDeath(OriginalWorldUnitView dead, int killerOwner)
        {
            if (!waveHelperActive || dead == null || dead.health > .405 || dead.kind == OriginalWorldUnitKind.Illusion ||
                HasEffectiveUnitAbility(dead,"A0K4") || !waveTraitDeaths.Add(dead.entityId)) return;
            if (waveTraits.Contains("A15K") && HasEffectiveUnitAbility(dead,"A15K"))
                waveExplosions.Add(new WaveExplosion { position = dead.position, damage = dead.profile.maxHealth * .1,
                    due = world.Clock + 1.5, generation = waveTraitGeneration });
            if (waveTraits.Contains("A15L") && HasEffectiveUnitAbility(dead,"A15L") && killerOwner > 0)
                ApplyWaveRoot(OriginalWorld.HeroEntityId(killerOwner));
        }

        void AdvanceWaveTraits(double seconds)
        {
            AdvanceWaveNativeEffects(seconds);
            if (!waveHelperActive) return;
            while (world.Clock + 1e-9 >= nextWaveBloodlust)
            { nextWaveBloodlust += 6; if (waveTraits.Contains("A15E")) ApplyWaveBloodlustPulse(); }
            while (world.Clock + 1e-9 >= nextWaveSilence)
            { nextWaveSilence += 8; if (waveTraits.Contains("A15M")) ApplyWaveSilencePulse(); }
            while (world.Clock + 1e-9 >= nextWaveDark)
            { nextWaveDark += 2; if (waveTraits.Contains("A15R")) ApplyWaveDarkRanks(true); }
            foreach (var explosion in new List<WaveExplosion>(waveExplosions))
            {
                if (explosion.due > world.Clock + 1e-9) continue;
                waveExplosions.Remove(explosion);
                if (!waveHelperActive || explosion.generation != waveTraitGeneration) continue;
                foreach (var actor in world.Snapshot().units)
                {
                    if (!waveHelperActive || explosion.generation != waveTraitGeneration) break;
                    if (actor.health > .405 && AreEnemies(0,actor.ownerSlot) && !CasterMagicImmune(actor) &&
                        !CasterHasType(actor,"structure") && SquaredDistance(actor.position,explosion.position) <= 250 * 250)
                        ApplyTriggeredHit(0,0,actor,explosion.damage,OriginalTriggeredDamageMode.SpellMagic);
                }
            }
        }

        void ApplyWaveDarkRanks(bool weakened)
        {
            // nAv28787/E8v32081. The latter intentionally includes dead heroes.
            // Source's I08M -> literal I0M1 is not an ability and remains a no-op.
            foreach (var actor in world.Snapshot().units)
            {
                if (!AreEnemies(0,actor.ownerSlot) || !IsNativeHeroPredicate(actor) || weakened && actor.health <= .405 ||
                    SquaredDistance(actor.position,new OriginalPoint(-50,1000)) > 3000 * 3000) continue;
                var player = players.Find(p=>p.slot == actor.ownerSlot);
                if (player?.inventory == null || actor.entityId != OriginalWorld.HeroEntityId(player.slot)) continue;
                if (!waveDarkRanks.TryGetValue(actor.entityId,out var ranks))
                { ranks = new HashSet<string>(StringComparer.Ordinal); waveDarkRanks.Add(actor.entityId,ranks); }
                foreach (var item in player.inventory.HeroSlots)
                {
                    string ability = item?.itemId == "I05A" ? "A0KM" : item?.itemId == "I07A" ? "A0GH" :
                        item?.itemId == "I00H" ? "A00W" : item?.itemId == "I03S" ? "A0BN" : item?.itemId == "I04B" ? "A007" : null;
                    if (ability == null) continue;
                    if (weakened) ranks.Add(ability); else ranks.Remove(ability);
                }
            }
        }

        int WaveTraitItemAbilityRank(int actor,string ability,int baselineRank) =>
            waveDarkRanks.TryGetValue(actor,out var ranks) && ranks.Contains(ability) ? 2 : baselineRank;
        void ForgetWaveTraitItemAbility(int actor,string ability)
        { if (waveDarkRanks.TryGetValue(actor,out var ranks)) ranks.Remove(ability); }

        // WTRAIT1 native axes are implemented separately in WaveNative.
        partial void ApplyWaveBloodlustPulse();
        partial void ApplyWaveSilencePulse();
        partial void ApplyWaveRoot(int hero);
    }
}
