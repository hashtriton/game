using System;
using System.Collections;
using System.Collections.Generic;
using Arena;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// Owns what the match keeps besides the units: the item data, the hero's purse and bag and the buying guides.
    /// The data of the original map is large, so it is parsed on the second frame, after the first one is on screen.
    /// </summary>
    public sealed class GameSession : MonoBehaviour
    {
        public Unit hero;
        /// <summary>The json assets of the original map's catalogs, in the form OriginalGameCatalogs expects.</summary>
        public TextAsset[] dataAssets = Array.Empty<TextAsset>();
        public TextAsset guidesJson;
        public long startGold = 130;
        public long startSouls = 4;

        public ItemBook Book { get; private set; }
        public Loadout Loadout { get; private set; }
        public GuideFile Guides { get; private set; }
        public bool Ready { get; private set; }
        public string LoadError { get; private set; }

        public event Action BecameReady;

        private IEnumerator Start()
        {
            // Let the first frame reach the screen: the parse below blocks the main thread for a moment.
            yield return null;
            yield return null;
            var clock = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                Book = new ItemBook(OriginalGameCatalogs.Load(dataAssets));
                Guides = guidesJson != null ? JsonUtility.FromJson<GuideFile>(guidesJson.text) : new GuideFile();
                Loadout = new Loadout(Book, hero, startGold, startSouls);
                Unit.Killed += OnKilled;
                Ready = true;
                Debug.Log("Item data loaded in " + clock.ElapsedMilliseconds + " ms");
            }
            catch (Exception exception)
            {
                LoadError = exception.Message;
                Debug.LogError("Item data failed to load: " + exception);
            }
            BecameReady?.Invoke();
        }

        private void OnDestroy()
        {
            Unit.Killed -= OnKilled;
        }

        private void OnKilled(Unit victim, Unit killer)
        {
            if (!Ready || victim.faction != Faction.Creep || killer == null || killer.faction != Faction.Hero) return;
            if (victim.bounty > 0) Loadout.Grant(victim.bounty);
        }

        private void Update()
        {
            // Testing key until waves pay out gold: F9 adds 500 gold.
            var keyboard = Keyboard.current;
            if (Ready && keyboard != null && keyboard.f9Key.wasPressedThisFrame) Loadout.Grant(500);
        }
    }
}
