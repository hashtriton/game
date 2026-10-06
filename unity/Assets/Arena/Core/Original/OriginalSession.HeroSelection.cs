using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        public static bool IsRandomHeroSelection(OriginalHeroSelection selection) =>
            selection == OriginalHeroSelection.Random || selection == OriginalHeroSelection.SameRandom;

        int HeroSelectionCapacity(OriginalHeroSelection selection) =>
            selection == OriginalHeroSelection.Random || selection == OriginalHeroSelection.Free ? availableHeroes.Count : 8;

        bool AssignRandomStartingHeroes(out string[] previousHeroes)
        {
            previousHeroes = null;
            if (!IsRandomHeroSelection(options.heroSelection)) return true;
            var pool = new List<string>(availableHeroes); pool.Sort(StringComparer.Ordinal);
            if (pool.Count == 0 || players.Count > HeroSelectionCapacity(options.heroSelection)) return false;

            // LiA3.9c e2/v2/O2 exclude occupied types; A2 selects AB once for
            // all owners in SameRandom. The approved pool is the current three
            // heroes. This independent seeded stream is not Warcraft RNG replay.
            uint random;
            unchecked
            {
                random = (uint)seed + 0x9E3779B9u;
                random = (random ^ (random >> 16)) * 0x85EBCA6Bu;
                random = (random ^ (random >> 13)) * 0xC2B2AE35u;
                random ^= random >> 16;
            }
            if (random == 0) random = 0x6D2B79F5u;
            int Choose(int count)
            {
                random ^= random << 13; random ^= random >> 17; random ^= random << 5;
                return (int)(((ulong)random * (uint)count) >> 32);
            }
            var assigned = new string[players.Count];
            if (options.heroSelection == OriginalHeroSelection.SameRandom)
            {
                string hero = pool[Choose(pool.Count)];
                for (int i = 0; i < assigned.Length; i++) assigned[i] = hero;
            }
            else
                for (int i = 0; i < assigned.Length; i++)
                {
                    int choice = Choose(pool.Count); assigned[i] = pool[choice]; pool.RemoveAt(choice);
                }
            previousHeroes = new string[players.Count];
            for (int i = 0; i < assigned.Length; i++)
            { previousHeroes[i] = players[i].hero; players[i].hero = assigned[i]; }
            return true;
        }

        void RestoreStartingHeroes(string[] previousHeroes)
        {
            if (previousHeroes == null) return;
            for (int i = 0; i < previousHeroes.Length; i++) players[i].hero = previousHeroes[i];
        }
    }
}
