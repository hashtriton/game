using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public enum OriginalSessionCommandKind { Hello = 1, SelectHero, LobbyReady, Start, WaveReady, Bet, DiscardBet, Move, Stop, AttackTarget, LearnSkill, HoldPosition, BuyItem, DropItem, TransferItem, PickupItem, CastSkill, UseItem, SetQuickBuy, AttackMove, SellItem, BuySoulUpgrade, UseWell, InteractItem, InteractWell }
    public enum OriginalSessionReplyCode
    {
        Accepted, VersionMismatch, UnknownConnection, Full, AlreadyJoined, AlreadyStarted,
        InvalidSequence, InvalidCommand, InvalidHero, HeroTaken, NotHost, NotReady, RuleUnavailable, ItemRejected
    }

    [Serializable]
    public sealed class OriginalSessionCommand
    {
        public int protocol;
        public string contentHash;
        public long sequence;
        public OriginalSessionCommandKind kind;
        public string heroId;
        public string skillId;
        public string itemId;
        public int shopInstanceId, itemSlot;
        public long itemInstanceId;
        public long targetItemInstanceId;
        public OriginalInventoryBag bag;
        public bool ready;
        public int betSide, betStake;
        public double x, y;
        public OriginalWorldTargetKind targetKind;
        public int targetId;
        public int actorEntityId;
    }

    [Serializable]
    public sealed class OriginalSessionPlayerView
    {
        public int slot;
        public int matchSlot;
        public string heroId;
        public bool connected;
        public bool lobbyReady;
        public bool waveReady;
        public bool alive;
        public long gold;
        public long souls;
        public int experience;
        public long acknowledgedSequence;
        public bool hasProgression;
        public OriginalHeroProgressionSnapshot progression;
        public OriginalSessionSkillView[] learning;
        public OriginalLearnedAbilityView[] auxiliaryAbilities;
        public bool archerAttackHandlerRegistered;
        public int bowElement;
        public bool hasInventory;
        public OriginalInventorySnapshot inventory;
        public OriginalItemActionCode lastItemAction;
        public OriginalAbilityView[] abilities;
        public OriginalItemUseView[] itemUses;
        public OriginalHeroCombatView combat;
        public OriginalSoulUpgradeView[] soulUpgrades = Array.Empty<OriginalSoulUpgradeView>();
    }

    [Serializable]
    public sealed class OriginalSessionView
    {
        public int protocol;
        public string contentHash;
        public long revision;
        public bool started;
        public string haltReason;
        public int initialParticipants;
        public int round;
        public OriginalMatchPhase phase;
        public double time;
        public double remainingSeconds;
        public int remainingEnemies;
        public int altars;
        public OriginalSessionPlayerView[] players;
        public OriginalMatchEnemy[] enemies;
        public OriginalMatchOptions options;
        public bool pendingDuel;
        public bool hasDuel;
        public long duelId;
        public OriginalDuelKind duelKind;
        public OriginalDuelSnapshot duel;
        public bool hasWorld;
        public OriginalWorldSnapshot world;
        public OriginalShopView[] shops;
        public OriginalGroundItemView[] groundItems;
        public OriginalVisualEffectView[] effects = Array.Empty<OriginalVisualEffectView>();
        public OriginalUnitAbilityView[] unitAbilities = Array.Empty<OriginalUnitAbilityView>();
        public OriginalWellView well = new OriginalWellView();
    }

    // One host FIFO preserves ordering across both referees. Nested slots are
    // match slots; players[].matchSlot maps them to persistent lobby slots.
    [Serializable]
    public sealed class OriginalSessionEvent
    {
        public long sequence, duelId;
        public OriginalMatchEvent matchEvent;
        public OriginalDuelEvent duelEvent;
    }

    // The owning host calls this on one simulation thread. Clients send intents;
    // only the trusted world reports kills, life observations and duel ratings.
    public sealed partial class OriginalSession
    {
        public const int Protocol = 17;
        public const long LocalHostConnection = 0;
        private sealed class Player
        {
            public long connection;
            public int slot, matchSlot;
            public string hero;
            public bool connected = true, ready;
            public long acknowledged;
            public OriginalInventory inventory;
            public OriginalItemActionCode lastItemAction;
            public OriginalHeroProgression progression;
            public OriginalHeroStatsSnapshot stats;
            public SortedDictionary<string, int> auxiliaryAbilities = new SortedDictionary<string, int>(StringComparer.Ordinal);
            public bool archerAttackHandlerRegistered;
            public int bowElement;
        }

        private readonly OriginalMatchCatalog matchCatalog;
        private readonly OriginalItemCatalog itemCatalog;
        private readonly OriginalDuelCatalog duelCatalog;
        private readonly OriginalCombatCatalog combatCatalog;
        private readonly HashSet<string> availableHeroes = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<Player> players = new List<Player>();
        private readonly List<OriginalSessionEvent> events = new List<OriginalSessionEvent>();
        private readonly string contentHash;
        private readonly int seed;
        private OriginalMatchOptions options;
        private OriginalMatch match;
        private OriginalDuel duel;
        private bool pendingDuel;
        private int carriedDuelPrize;
        private long duelId, appliedDuelSequence;
        private long revision;
        private long eventSequence;
        public string HaltReason { get; private set; }
        public bool Started => match != null;

        public OriginalSession(OriginalMatchCatalog matchCatalog, OriginalItemCatalog itemCatalog,
            OriginalCombatCatalog combatCatalog, OriginalDuelCatalog duelCatalog, string contentHash, OriginalMatchOptions options, int seed)
        {
            this.matchCatalog = matchCatalog ?? throw new ArgumentNullException(nameof(matchCatalog));
            this.itemCatalog = itemCatalog ?? throw new ArgumentNullException(nameof(itemCatalog));
            if (duelCatalog == null) throw new ArgumentNullException(nameof(duelCatalog));
            duelCatalog.Validate();
            this.duelCatalog = duelCatalog.Copy();
            if (combatCatalog == null) throw new ArgumentNullException(nameof(combatCatalog));
            this.combatCatalog = combatCatalog;
            if (contentHash == null || contentHash.Length != 64) throw new ArgumentException("Expected a SHA256 content fingerprint.");
            foreach (var c in contentHash)
                if (!(c >= '0' && c <= '9') && !(c >= 'a' && c <= 'f')) throw new ArgumentException("Invalid content fingerprint.");
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate(); matchCatalog.Validate(); combatCatalog.BuildIndexes(); itemCatalog.BuildIndexes();
            if (itemCatalog.mapSha256 != matchCatalog.sourceSha256 || combatCatalog.sourceSha256 != matchCatalog.sourceSha256 ||
                duelCatalog.sourceSha256 != matchCatalog.sourceSha256)
                throw new ArgumentException("Catalogs have different source maps.");
            foreach (var hero in combatCatalog.selectedHeroes) availableHeroes.Add(hero.id);
            this.contentHash = contentHash; this.options = options.Copy(); this.seed = seed;
            players.Add(new Player { connection = LocalHostConnection, slot = 1 });
        }

        public OriginalSessionReplyCode Apply(long connectionId, OriginalSessionCommand command)
        {
            if (command == null || !Enum.IsDefined(typeof(OriginalSessionCommandKind), command.kind))
                return OriginalSessionReplyCode.InvalidCommand;
            var player = players.Find(x => x.connection == connectionId && x.connected);
            if (command.kind == OriginalSessionCommandKind.Hello)
            {
                if (Started) return OriginalSessionReplyCode.AlreadyStarted;
                if (player != null) return OriginalSessionReplyCode.AlreadyJoined;
                if (connectionId <= 0) return OriginalSessionReplyCode.UnknownConnection;
                if (command.protocol != Protocol || command.contentHash != contentHash) return OriginalSessionReplyCode.VersionMismatch;
                if (command.sequence != 1) return OriginalSessionReplyCode.InvalidSequence;
                if (players.Count >= HeroSelectionCapacity(options.heroSelection)) return OriginalSessionReplyCode.Full;
                var slot = 1;
                while (players.Exists(x => x.slot == slot)) slot++;
                players.Add(new Player { connection = connectionId, slot = slot, acknowledged = 1 });
                revision++;
                return OriginalSessionReplyCode.Accepted;
            }
            if (player == null) return OriginalSessionReplyCode.UnknownConnection;
            if (command.sequence <= 0 || command.sequence != player.acknowledged + 1)
                return OriginalSessionReplyCode.InvalidSequence;
            // Even a rejected command is consumed, so a previously rejected
            // purchase/action cannot become valid when replayed at a later time.
            player.acknowledged = command.sequence; revision++;
            if (HaltReason != null) return OriginalSessionReplyCode.RuleUnavailable;
            // Networking can continue while the world is paused or stalled.
            // Apply the same consumer bound to commands as to simulation ticks.
            if (Started && !CanMutateHost()) return OriginalSessionReplyCode.RuleUnavailable;
            switch (command.kind)
            {
                case OriginalSessionCommandKind.SelectHero:
                    if (Started) return OriginalSessionReplyCode.AlreadyStarted;
                    if (command.heroId == null || !availableHeroes.Contains(command.heroId)) return OriginalSessionReplyCode.InvalidHero;
                    if (IsRandomHeroSelection(options.heroSelection))
                        return OriginalSessionReplyCode.RuleUnavailable;
                    if (options.heroSelection == OriginalHeroSelection.Free && players.Exists(x => x != player && x.hero == command.heroId))
                        return OriginalSessionReplyCode.HeroTaken;
                    player.hero = command.heroId; player.ready = false;
                    return OriginalSessionReplyCode.Accepted;
                case OriginalSessionCommandKind.LobbyReady:
                    if (Started) return OriginalSessionReplyCode.AlreadyStarted;
                    if (command.ready && player.hero == null && !IsRandomHeroSelection(options.heroSelection)) return OriginalSessionReplyCode.InvalidHero;
                    player.ready = command.ready;
                    return OriginalSessionReplyCode.Accepted;
                case OriginalSessionCommandKind.Start:
                    if (connectionId != LocalHostConnection) return OriginalSessionReplyCode.NotHost;
                    if (Started) return OriginalSessionReplyCode.AlreadyStarted;
                    if (players.Exists(x => !x.ready || !x.connected || x.hero == null && !IsRandomHeroSelection(options.heroSelection))) return OriginalSessionReplyCode.NotReady;
                    if (!AssignRandomStartingHeroes(out var previousHeroes)) return OriginalSessionReplyCode.RuleUnavailable;
                    OriginalWorld startingWorld;
                    StartingProgression[] startingProgressions;
                    try { startingProgressions = CreateStartingProgressions(); startingWorld = CreateWorld(); }
                    catch (InvalidOperationException) { RestoreStartingHeroes(previousHeroes); return OriginalSessionReplyCode.RuleUnavailable; }
                    var startingMatch = new OriginalMatch(matchCatalog, options, players.Count, seed);
                    var startingInventories = new OriginalInventory[players.Count];
                    for (var i = 0; i < players.Count; i++)
                    {
                        startingInventories[i] = StartingInventory(players[i]);
                        startingMatch.RegisterHero(i + 1, players[i].hero);
                    }
                    startingMatch.Begin();
                    for (var i = 0; i < players.Count; i++)
                    {
                        players[i].matchSlot = i + 1;
                        players[i].inventory = startingInventories[i];
                        players[i].progression = startingProgressions[i]?.progression;
                        players[i].stats = startingProgressions[i]?.stats;
                    }
                    match = startingMatch;
                    world = startingWorld;
                    ratingLedger = new OriginalDuelRatingLedger(players.Count);
                    InitializeRoundBonuses();
                    InitializeWorldCurses();
                    StartShops();
                    CollectEvents();
                    return OriginalSessionReplyCode.Accepted;
                case OriginalSessionCommandKind.WaveReady:
                    return match != null && match.SetReady(player.matchSlot) ? ConsumeMatchEvents() : OriginalSessionReplyCode.NotReady;
                case OriginalSessionCommandKind.LearnSkill:
                    return LearnSkill(player, command.skillId);
                case OriginalSessionCommandKind.CastSkill:
                    return AfterAcceptedInteractionInterrupt(player, command, ApplyCastCommand(player, command));
                case OriginalSessionCommandKind.UseItem:
                    return AfterAcceptedInteractionInterrupt(player, command, ApplyCastCommand(player, command));
                case OriginalSessionCommandKind.BuySoulUpgrade:
                    return BuySoulUpgrade(player, command.skillId);
                case OriginalSessionCommandKind.UseWell:
                    return AfterAcceptedInteractionInterrupt(player, command, UseHealingWell(player, command.actorEntityId));
                case OriginalSessionCommandKind.SetQuickBuy:
                    if (!Started || player.inventory == null) return OriginalSessionReplyCode.NotReady;
                    player.inventory.QuickBuyEnabled = command.ready;
                    return OriginalSessionReplyCode.Accepted;
                case OriginalSessionCommandKind.BuyItem:
                case OriginalSessionCommandKind.SellItem:
                case OriginalSessionCommandKind.DropItem:
                case OriginalSessionCommandKind.TransferItem:
                case OriginalSessionCommandKind.PickupItem:
                    return AfterAcceptedInteractionInterrupt(player, command, ApplyItemCommand(player, command));
                case OriginalSessionCommandKind.InteractItem:
                case OriginalSessionCommandKind.InteractWell:
                    return ApplyInteractionCommand(player, command);
                case OriginalSessionCommandKind.Bet:
                    if (!DuelActive || !duel.TrySetBet(player.matchSlot, command.betSide, command.betStake,
                        (int)Math.Min(int.MaxValue, player.inventory.Gold))) return OriginalSessionReplyCode.NotReady;
                    CollectDuelEvents();
                    return HaltReason == null ? OriginalSessionReplyCode.Accepted : OriginalSessionReplyCode.RuleUnavailable;
                case OriginalSessionCommandKind.DiscardBet:
                    if (!DuelActive || !duel.DiscardBet(player.matchSlot)) return OriginalSessionReplyCode.NotReady;
                    CollectDuelEvents();
                    return OriginalSessionReplyCode.Accepted;
                case OriginalSessionCommandKind.Move:
                case OriginalSessionCommandKind.AttackMove:
                case OriginalSessionCommandKind.Stop:
                case OriginalSessionCommandKind.AttackTarget:
                case OriginalSessionCommandKind.HoldPosition:
                    return ApplyWorldCommand(player, command);
                default: return OriginalSessionReplyCode.InvalidCommand;
            }
        }

        private OriginalSessionReplyCode ConsumeMatchEvents()
        {
            CollectEvents();
            return OriginalSessionReplyCode.Accepted;
        }

        public bool SetOptions(OriginalMatchOptions value)
        {
            if (Started || value == null) return false;
            value.Validate();
            if (players.Count > HeroSelectionCapacity(value.heroSelection)) return false;
            options = value.Copy();
            foreach (var player in players) { player.hero = null; player.ready = false; }
            revision++;
            return true;
        }

        private bool DuelActive => duel != null && duel.Phase != OriginalDuelPhase.Completed;

        // Host-only. Input slots are persistent lobby slots, unlike the core
        // duel's contiguous match slots. The world supplies the actual rating
        // (SourceRating uses nH/IH/AH/bH) and current A19P/A19Q curse states.
        public bool BeginDuel(OriginalDuelParticipant[] roster)
        {
            if (!CanMutateHost() || !pendingDuel || match.Phase != OriginalMatchPhase.Duel) return false;
            if (roster == null || roster.Length != players.Count)
                throw new ArgumentException("A complete authoritative duel roster is required.", nameof(roster));
            var participants = new OriginalDuelParticipant[players.Count];
            foreach (var supplied in roster)
            {
                var player = supplied == null ? null : players.Find(p => p.slot == supplied.slot);
                if (player == null || player.hero != supplied.heroRawcode || participants[player.matchSlot - 1] != null)
                    throw new ArgumentException("Duel roster must identify each selected hero exactly once.", nameof(roster));
                participants[player.matchSlot - 1] = new OriginalDuelParticipant { slot = player.matchSlot,
                    heroRawcode = player.hero, rating = supplied.rating, mirrorCurse = supplied.mirrorCurse,
                    bloodPoisonCurse = supplied.bloodPoisonCurse };
            }
            // Portable deterministic instance seeds are not Warcraft RNG replay.
            long nextId = checked(duelId + 1);
            var candidate = new OriginalDuel(duelCatalog, match.DuelKind, match.Round, participants,
                unchecked(seed + (int)nextId * 104729), carriedDuelPrize);
            candidate.Begin();
            duel = candidate; duelId = nextId; appliedDuelSequence = 0; pendingDuel = false;
            CollectDuelEvents(); revision++;
            return true;
        }

        // The world must validate actual death and source kill eligibility.
        // Client commands can never report damage, death, currency or rating.
        public bool ReportEnemyKilled(int entityId, bool eligibleAlliedKill = true)
        {
            if (!CanMutateHost() || pendingDuel || DuelActive) return false;
            int sourceUserData=0;foreach(var enemy in match.Enemies)if(enemy.entityId==entityId){sourceUserData=enemy.sourceUserData;break;}
            var body = world?.UnitState(OriginalWorld.EnemyEntityId(entityId));
            if (body != null && body.health <= 0) OnPyroUnitDied(body.entityId);
            if (!match.EnemyKilled(entityId, eligibleAlliedKill)) return false;
            ObserveCurseMegaDeath(sourceUserData);
            CollectEvents(); revision++;
            return true;
        }

        // Slots and the optional simultaneous-death mask use lobby slots, with
        // bit0 clear. The world reports primary-hero eligible deaths only;
        // summoned-unit killers pass their owner's lobby slot. A duel death
        // must never also reach Match.HeroDied and consume an altar.
        public bool ReportHeroDied(int slot, int killerSlot = 0, int aliveMaskAfterEvent = -1)
        {
            if (!CanMutateHost() || pendingDuel) return false;
            var player = PlayerAt(slot);
            var body = world?.UnitState(OriginalWorld.HeroEntityId(slot));
            if (body != null && body.health <= 0) OnPyroUnitDied(body.entityId);
            if (DuelActive)
            {
                int killerMatchSlot = killerSlot == 0 ? 0 : PlayerAt(killerSlot).matchSlot;
                if (!duel.HeroDied(player.matchSlot, killerMatchSlot, MatchAliveMask(aliveMaskAfterEvent))) return false;
                CollectDuelEvents();
            }
            else
            {
                ObserveDeathBonds(slot);
                if (!match.HeroDied(player.matchSlot)) return false;
                CollectEvents();
            }
            revision++;
            return true;
        }

        public bool ObserveDuelHeroAlive(int slot, bool alive)
        {
            if (!CanMutateHost() || !DuelActive) return false;
            duel.ObserveHeroAlive(PlayerAt(slot).matchSlot, alive); revision++;
            return true;
        }

        public bool UpdateDuelRating(int slot, int rating)
        {
            if (!CanMutateHost() || !DuelActive) return false;
            duel.SetRating(PlayerAt(slot).matchSlot, rating); revision++;
            return true;
        }

        private Player PlayerAt(int slot)
        {
            var player = players.Find(p => p.slot == slot);
            if (player == null) throw new ArgumentOutOfRangeException(nameof(slot));
            return player;
        }

        private int MatchAliveMask(int mask)
        {
            if (mask == -1) return -1;
            int valid = 0, translated = 0;
            foreach (var player in players)
            {
                valid |= 1 << player.slot;
                if ((mask & (1 << player.slot)) != 0) translated |= 1 << player.matchSlot;
            }
            if (mask < 0 || (mask & ~valid) != 0) throw new ArgumentOutOfRangeException(nameof(mask));
            return translated;
        }

        private bool CanMutateHost()
        {
            if (match == null || HaltReason != null) return false;
            if (events.Count < 4096) return true;
            HaltReason = "host-event-consumer-backpressure"; revision++;
            return false;
        }

        public bool Disconnect(long connectionId)
        {
            if (connectionId == LocalHostConnection) return false;
            var player = players.Find(x => x.connection == connectionId && x.connected);
            if (player == null) return false;
            if (Started)
            {
                player.connected = false;
                if (DuelActive) { duel.ParticipantDisconnected(player.matchSlot); CollectDuelEvents(); }
                LearnDisconnectedSkill(player);
            }
            else players.Remove(player);
            revision++;
            return true;
        }

        public void Advance(double seconds)
        {
            if (!OriginalCombatDefinition.IsFinite(seconds) || seconds < 0 || seconds > 1)
                throw new ArgumentOutOfRangeException(nameof(seconds), "Host advances in bounded simulation steps.");
            if (world != null)
            {
                for (double left = seconds; left > 1e-9 && HaltReason == null;)
                {
                    double step = Math.Min(.05, left);
                    worldStepEndsAt = world.Clock + step;
                    SyncFinalBossHealth();
                    SyncCocoonAccelerators();
                    AdvanceReferees(step);
                    if (HaltReason == null && !pendingDuel) AdvanceWorld(step);
                    left -= step;
                }
                return;
            }
            AdvanceReferees(seconds);
        }

        private void AdvanceReferees(double seconds)
        {
            if (!CanMutateHost() || pendingDuel) return;
            // PvE timers do not run while the duel referee owns simulation time.
            // Completion starts the next outer timer only after all duel events
            // were accounted for and published in source order.
            if (DuelActive)
            {
                double before = duel.Time;
                duel.Advance(seconds); CollectDuelEvents();
                // The referee stops its clock at completion. A fractional host
                // step may still have time left after that instant; only that
                // remainder belongs to the next PvE timer.
                double remainder = seconds - (duel.Time - before);
                if (HaltReason == null && !DuelActive && remainder > 1e-9)
                { match.Advance(remainder); CollectEvents(); }
            }
            else { match.Advance(seconds); CollectEvents(); }
            revision++;
        }

        private void CollectEvents()
        {
            foreach (var item in match.DrainEvents())
            {
                ApplyRatingMatchEvent(item);
                if (item.kind == OriginalMatchEventKind.Gold || item.kind == OriginalMatchEventKind.Souls)
                {
                    var player = players.Find(x => x.matchSlot == item.slot);
                    player.inventory.GrantResources(item.kind == OriginalMatchEventKind.Gold ? item.amount : 0,
                        item.kind == OriginalMatchEventKind.Souls ? item.amount : 0);
                }
                if (item.kind == OriginalMatchEventKind.Unresolved) HaltReason = item.sourceRule;
                if (item.kind == OriginalMatchEventKind.ShopAccess) ApplyShopAccessEvent(item);
                if (item.kind == OriginalMatchEventKind.Experience)
                    SyncProgressionExperience(players.Find(x => x.matchSlot == item.slot));
                if (item.kind == OriginalMatchEventKind.DuelRequired) pendingDuel = true;
                ApplyWorldEvent(item);
                events.Add(new OriginalSessionEvent { sequence = ++eventSequence, matchEvent = item });
            }
            BeginWorldDuelIfReady();
        }

        private void CollectDuelEvents()
        {
            var batch = duel.DrainEvents();
            if (!ApplyDuelWorldBatch(batch)) return;
            foreach (var item in batch)
            {
                // Core sequence numbers restart per instance. This watermark is
                // scoped by duelId, so publication/drain/snapshot cannot regrant.
                if (item.sequence != appliedDuelSequence + 1)
                    throw new InvalidOperationException("Duel event ledger sequence is not contiguous.");
                ApplyDuelRingEvent(item);
                if (item.kind == OriginalDuelEventKind.Gold || item.kind == OriginalDuelEventKind.Souls)
                {
                    var player = players.Find(p => p.matchSlot == item.slot);
                    if (item.amount < 0)
                    {
                        bool spent = player.inventory.TrySpendResources(item.kind == OriginalDuelEventKind.Gold ? -(long)item.amount : 0,
                            item.kind == OriginalDuelEventKind.Souls ? -(long)item.amount : 0);
                        if (!spent) throw new InvalidOperationException("A funded duel debit exceeded the authoritative inventory.");
                    }
                    else player.inventory.GrantResources(item.kind == OriginalDuelEventKind.Gold ? item.amount : 0,
                        item.kind == OriginalDuelEventKind.Souls ? item.amount : 0);
                }
                appliedDuelSequence = item.sequence;
                if (item.kind == OriginalDuelEventKind.Unresolved) HaltReason = item.code;
                events.Add(new OriginalSessionEvent { sequence = ++eventSequence, duelId = duelId, duelEvent = item });
                if (item.kind == OriginalDuelEventKind.Completed)
                {
                    carriedDuelPrize = duel.CarryPrizeGold;
                    if (!match.CompleteDuelSequence()) throw new InvalidOperationException("Unexpected duel completion outside the match duel phase.");
                    CollectEvents();
                }
            }
        }

        public OriginalSessionEvent[] DrainEvents()
        {
            var result = events.ToArray(); events.Clear(); return result;
        }

        public OriginalSessionView Snapshot()
        {
            var duelView = duel?.Snapshot();
            var views = new List<OriginalSessionPlayerView>();
            foreach (var player in players)
                views.Add(new OriginalSessionPlayerView { slot = player.slot, matchSlot = player.matchSlot,
                    heroId = player.hero, connected = player.connected, lobbyReady = player.ready,
                    waveReady = match != null && match.IsReady(player.matchSlot),
                    alive = match != null && (DuelActive ? duelView.alive[player.matchSlot] : match.IsAlive(player.matchSlot)),
                    gold = player.inventory?.Gold ?? 0, souls = player.inventory?.Lumber ?? 0,
                    experience = player.progression?.Experience ?? match?.Experience(player.matchSlot) ?? 0, acknowledgedSequence = player.acknowledged,
                    hasProgression = player.progression != null, progression = player.progression?.Snapshot(), learning = LearningView(player),
                    auxiliaryAbilities = AuxiliaryView(player), archerAttackHandlerRegistered = player.archerAttackHandlerRegistered, bowElement = player.bowElement,
                    hasInventory = player.inventory != null, inventory = player.inventory?.Snapshot(), lastItemAction = player.lastItemAction,
                    abilities = AbilityViews(player), itemUses = ItemUseViews(player), soulUpgrades = SoulUpgradeViews(player), combat = HeroCombatView(player) });
            var enemies = new List<OriginalMatchEnemy>();
            if (match != null)
                foreach (var enemy in match.Enemies)
                    enemies.Add(new OriginalMatchEnemy { entityId = enemy.entityId, rawcode = enemy.rawcode, x = enemy.x, y = enemy.y,
                        counted = enemy.counted, finalAdd = enemy.finalAdd, cocoon = enemy.cocoon, megaBoss = enemy.megaBoss,
                        cocoonTimer = enemy.cocoonTimer, nearbyAccelerators = enemy.nearbyAccelerators, attackGroup = enemy.attackGroup,
                        sourceUserData = enemy.sourceUserData });
            return new OriginalSessionView { protocol = Protocol, contentHash = contentHash, revision = revision,
                started = Started, haltReason = HaltReason, initialParticipants = match?.Participants ?? 0,
                round = match?.Round ?? 0, phase = match?.Phase ?? OriginalMatchPhase.Configuration,
                time = match?.Clock ?? 0, remainingSeconds = DuelActive ? Math.Max(0, duelView.phaseEndsAt - duelView.time) : match?.RemainingSeconds ?? 0,
                remainingEnemies = match?.RemainingCount ?? 0, altars = match?.Altars ?? 0,
                players = views.ToArray(), enemies = enemies.ToArray(), options = options.Copy(), pendingDuel = pendingDuel,
                duelId = duelId, duelKind = match?.DuelKind ?? OriginalDuelKind.Pairs, hasDuel = duelView != null, duel = duelView,
                hasWorld = world != null, world = WorldVisibilitySnapshot(), shops = ShopViews(), groundItems = GroundItemViews(), effects = VisualEffects(), unitAbilities = UnitAbilityViews(), well = SnapshotHealingWellView() };
        }
    }
}
