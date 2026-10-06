using System;
using System.Collections.Generic;
using System.Text;
using Arena.Original;
using UnityEngine;

namespace Arena
{
    public sealed class OriginalUnitySessionCodec : IOriginalSessionCodec
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        public byte[] EncodeCommand(OriginalSessionCommand command) => Encode(command);
        public byte[] EncodeResponse(OriginalNetworkResponse response) => Encode(response);

        private static byte[] Encode(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Utf8.GetBytes(JsonUtility.ToJson(value));
        }

        public bool TryDecodeCommand(byte[] bytes, out OriginalSessionCommand command)
        {
            command = null;
            if (!Read(bytes, 4096, out var json)) return false;
            try
            {
                var value = JsonUtility.FromJson<OriginalSessionCommand>(json);
                if (value == null || !Enum.IsDefined(typeof(OriginalSessionCommandKind), value.kind) ||
                    value.sequence <= 0 || (value.heroId != null && value.heroId.Length > 4) ||
                    (value.skillId != null && value.skillId.Length > 4) ||
                    (value.itemId != null && value.itemId.Length > 4) ||
                    value.actorEntityId < 0 ||
                    value.targetItemInstanceId < 0 || value.targetItemInstanceId != 0 && value.kind != OriginalSessionCommandKind.UseItem ||
                    (value.contentHash != null && value.contentHash.Length > 64)) return false;
                if (value.kind == OriginalSessionCommandKind.Bet &&
                    (value.betSide < 0 || value.betSide > 2 || value.betStake < 0 || value.betStake > 1000)) return false;
                if ((value.kind == OriginalSessionCommandKind.Move || value.kind == OriginalSessionCommandKind.AttackMove) && !Point(value.x, value.y)) return false;
                if (value.kind == OriginalSessionCommandKind.LearnSkill && !Rawcode(value.skillId)) return false;
                if (value.kind == OriginalSessionCommandKind.BuySoulUpgrade && OriginalSoulUpgradeRules.Maximum(value.skillId) == 0) return false;
                if (value.kind == OriginalSessionCommandKind.CastSkill &&
                    (!Rawcode(value.skillId) || !Point(value.x, value.y) ||
                     !Enum.IsDefined(typeof(OriginalWorldTargetKind), value.targetKind) || value.targetId < 0 ||
                     value.targetKind == OriginalWorldTargetKind.Unit && value.targetId == 0)) return false;
                if ((value.kind == OriginalSessionCommandKind.BuyItem || value.kind == OriginalSessionCommandKind.DropItem ||
                    value.kind == OriginalSessionCommandKind.TransferItem || value.kind == OriginalSessionCommandKind.PickupItem ||
                    value.kind == OriginalSessionCommandKind.UseItem || value.kind == OriginalSessionCommandKind.SellItem) &&
                    !Enum.IsDefined(typeof(OriginalInventoryBag), value.bag)) return false;
                if (value.kind == OriginalSessionCommandKind.BuyItem && (!Rawcode(value.itemId) || value.shopInstanceId <= 0)) return false;
                if (value.kind == OriginalSessionCommandKind.SellItem &&
                    (value.shopInstanceId <= 0 || value.itemSlot < 0 || value.itemSlot >= 6 || value.itemInstanceId <= 0)) return false;
                if ((value.kind == OriginalSessionCommandKind.DropItem || value.kind == OriginalSessionCommandKind.TransferItem || value.kind == OriginalSessionCommandKind.UseItem) &&
                    (value.itemSlot < 0 || value.itemSlot >= 6)) return false;
                if (value.kind == OriginalSessionCommandKind.PickupItem && value.itemInstanceId <= 0) return false;
                if (value.kind == OriginalSessionCommandKind.InteractItem && value.itemInstanceId <= 0 ||
                    value.kind == OriginalSessionCommandKind.InteractWell && value.itemInstanceId != 0) return false;
                if (value.kind == OriginalSessionCommandKind.UseItem &&
                    (value.itemInstanceId <= 0 || !Point(value.x, value.y) ||
                     value.targetKind != OriginalWorldTargetKind.None && value.targetKind != OriginalWorldTargetKind.Unit ||
                     value.targetKind == OriginalWorldTargetKind.None && value.targetId != 0 ||
                     value.targetKind == OriginalWorldTargetKind.Unit && value.targetId <= 0)) return false;
                if (value.kind == OriginalSessionCommandKind.AttackTarget &&
                    (value.targetKind != OriginalWorldTargetKind.Unit && value.targetKind != OriginalWorldTargetKind.Doodad ||
                     value.targetId < 0 || value.targetKind == OriginalWorldTargetKind.Unit && value.targetId == 0)) return false;
                command = value;
                return true;
            }
            catch (ArgumentException) { return false; }
        }

        static bool ValidHealingWell(OriginalSessionView view)
        {
            var well=view.well;
            if(well==null||!Point(well.position.x,well.position.y)||!NonnegativeFinite(well.mana)||!NonnegativeFinite(well.maxMana))return false;
            if(!well.present)return well.position.x==0&&well.position.y==0&&well.mana==0&&well.maxMana==0;
            return view.started&&view.hasWorld&&well.position.x==-60&&well.position.y==-380&&well.maxMana==2000&&well.mana<=well.maxMana;
        }

        public bool TryDecodeResponse(byte[] bytes, out OriginalNetworkResponse response)
        {
            response = null;
            if (!Read(bytes, OriginalSessionWire.MaximumMessageBytes, out var json)) return false;
            try
            {
                var value = JsonUtility.FromJson<OriginalNetworkResponse>(json);
                if (value == null || !Enum.IsDefined(typeof(OriginalNetworkResponseKind), value.kind) ||
                    !Enum.IsDefined(typeof(OriginalSessionReplyCode), value.code) ||
                    value.assignedSlot < 0 || value.assignedSlot > 8 || value.acknowledgedSequence < 0) return false;
                if (value.kind == OriginalNetworkResponseKind.Reply &&
                    (value.commandSequence <= 0 || (value.code == OriginalSessionReplyCode.Accepted && value.assignedSlot == 0))) return false;
                if (value.kind == OriginalNetworkResponseKind.Snapshot)
                {
                    if (!ValidSnapshot(value)) return false;
                    if (!value.snapshot.hasDuel) value.snapshot.duel = null;
                    if (!value.snapshot.hasWorld) value.snapshot.world = null;
                    foreach (var player in value.snapshot.players)
                    {
                        if (!player.hasProgression) player.progression = null;
                        if (!player.hasInventory) player.inventory = null;
                        else
                        {
                            NormalizeSlots(player.inventory.heroSlots);
                            NormalizeSlots(player.inventory.servantSlots);
                        }
                    }
                }
                response = value;
                return true;
            }
            catch (ArgumentException) { return false; }
        }

        private static bool ValidSnapshot(OriginalNetworkResponse response)
        {
            var view = response.snapshot;
            if (response.code != OriginalSessionReplyCode.Accepted || response.assignedSlot == 0 ||
                view == null || view.protocol != OriginalSession.Protocol || !ValidHash(view.contentHash) ||
                view.revision < 0 || view.players == null || view.players.Length < 1 || view.players.Length > 8 ||
                view.enemies == null || view.options == null || view.initialParticipants < 0 || view.initialParticipants > 8 ||
                view.round < 0 || view.round > 30 || !Enum.IsDefined(typeof(OriginalMatchPhase), view.phase) ||
                !NonnegativeFinite(view.time) || !NonnegativeFinite(view.remainingSeconds) ||
                view.remainingEnemies < 0 || view.altars < 0 ||
                !Enum.IsDefined(typeof(OriginalDifficulty), view.options.difficulty) ||
                !Enum.IsDefined(typeof(OriginalHeroSelection), view.options.heroSelection) ||
                !Enum.IsDefined(typeof(OriginalDefensiveBarrels), view.options.defensiveBarrels) || !ValidHealingWell(view)) return false;
            if (view.started)
            {
                if (view.initialParticipants != view.players.Length || view.round == 0 ||
                    view.phase == OriginalMatchPhase.Configuration) return false;
            }
            else if (view.initialParticipants != 0 || view.round != 0 || view.phase != OriginalMatchPhase.Configuration ||
                view.time != 0 || view.remainingSeconds != 0 || view.remainingEnemies != 0 || view.altars != 0 ||
                view.enemies.Length != 0) return false;

            var slots = new bool[9];
            var matchSlots = new bool[9];
            OriginalSessionPlayerView own = null;
            foreach (var player in view.players)
            {
                if (player == null || player.slot < 1 || player.slot > 8 || slots[player.slot] ||
                    player.acknowledgedSequence < 0 || player.gold < 0 || player.souls < 0 || player.experience < 0 ||
                    (!string.IsNullOrEmpty(player.heroId) && player.heroId.Length != 4) ||
                    (player.lobbyReady && string.IsNullOrEmpty(player.heroId) &&
                        (view.started || !OriginalSession.IsRandomHeroSelection(view.options.heroSelection))) ||
                    (player.slot == 1 && !player.connected) || !ValidProgression(player, view.started) || !ValidSoulUpgrades(player, view.started) || !ValidCombatStats(player,view)) return false;
                slots[player.slot] = true;
                if (view.started)
                {
                    if (player.matchSlot < 1 || player.matchSlot > view.initialParticipants || matchSlots[player.matchSlot] ||
                        string.IsNullOrEmpty(player.heroId) || (player.waveReady && view.phase != OriginalMatchPhase.Preparation)) return false;
                    matchSlots[player.matchSlot] = true;
                }
                else if (player.matchSlot != 0 || player.alive || player.waveReady || !player.connected) return false;
                if (player.slot == response.assignedSlot) own = player;
            }
            // This ACK belongs to this snapshot. It may be older than a Reply
            // already received while a large snapshot was waiting to be sent.
            if (!slots[1] || own == null || !own.connected || own.acknowledgedSequence != response.acknowledgedSequence) return false;
            var entities = new HashSet<int>();
            foreach (var enemy in view.enemies)
                if (enemy == null || enemy.entityId <= 0 || !entities.Add(enemy.entityId) ||
                    enemy.rawcode == null || enemy.rawcode.Length != 4 || enemy.attackGroup < 0 || enemy.attackGroup > 2 ||
                    float.IsNaN(enemy.x) || float.IsInfinity(enemy.x) || float.IsNaN(enemy.y) || float.IsInfinity(enemy.y)) return false;
            return ValidDuel(view) && ValidWorld(view) && ValidItems(view) && ValidAbilities(view) && ValidEffects(view) && ValidUnitAbilities(view);
        }

        static bool ValidUnitAbilities(OriginalSessionView view)
        {
            if(view.unitAbilities==null||view.unitAbilities.Length>8192||!view.hasWorld&&view.unitAbilities.Length!=0)return false;
            var actors=new HashSet<int>();
            foreach(var group in view.unitAbilities)
            {
                if(group==null||!actors.Add(group.entityId)||group.abilities==null||group.abilities.Length>64)return false;
                var actor=Array.Find(view.world.units,u=>u.entityId==group.entityId);
                if(actor==null||actor.kind!=OriginalWorldUnitKind.Summon||actor.ownerSlot<1||actor.ownerSlot>8)return false;
                var ids=new HashSet<string>(StringComparer.Ordinal);
                foreach(var ability in group.abilities)
                {
                    if(ability==null||!Rawcode(ability.id)||ability.castAbilityId!=ability.id||!ids.Add(ability.id)||ability.rank!=1||
                        string.IsNullOrEmpty(ability.name)||ability.name.Length>256||
                        !Enum.IsDefined(typeof(OriginalAbilityTargetMode),ability.targetMode)||ability.targetMode==OriginalAbilityTargetMode.UnitOrPoint||!Enum.IsDefined(typeof(OriginalAbilityUseCode),ability.code)||
                        !NonnegativeFinite(ability.manaCost)||!NonnegativeFinite(ability.cooldownRemaining)||
                        !ability.manaCostKnown&&ability.manaCost!=0||ability.code==OriginalAbilityUseCode.NotLearned||ability.code==OriginalAbilityUseCode.Passive)return false;
                    if(ability.code==OriginalAbilityUseCode.Ready&&(!ability.implemented||!ability.manaCostKnown||ability.cooldownRemaining!=0||
                        actor.health<=.405||actor.paused||actor.hidden||actor.mana<ability.manaCost))return false;
                }
            }
            if(view.hasWorld)foreach(var actor in view.world.units)
                if(actor.kind==OriginalWorldUnitKind.Summon&&!actors.Contains(actor.entityId))return false;
            return true;
        }

        static bool ValidCombatStats(OriginalSessionPlayerView player,OriginalSessionView view)
        {
            var s=player.combat;
            foreach(double value in new[]{s.strength,s.agility,s.intelligence,s.armor,s.attackMinimum,s.attackMaximum,s.attackInterval})
                if(double.IsNaN(value)||double.IsInfinity(value)||Math.Abs(value)>1000000000)return false;
            if(!s.known)return string.IsNullOrEmpty(s.primaryAttribute)&&s.strength==0&&s.agility==0&&s.intelligence==0&&
                s.armor==0&&s.attackMinimum==0&&s.attackMaximum==0&&s.attackInterval==0;
            return view.started&&view.hasWorld&&player.hasProgression&&
                (s.primaryAttribute=="STR"||s.primaryAttribute=="AGI"||s.primaryAttribute=="INT")&&
                s.attackMinimum>=0&&s.attackMaximum>=s.attackMinimum&&s.attackInterval>0;
        }

        static bool ValidSoulUpgrades(OriginalSessionPlayerView player, bool started)
        {
            var rows = player.soulUpgrades;
            if (rows == null) return false;
            if (!started) return rows.Length == 0;
            if (rows.Length != 12) return false;
            var ids = new HashSet<string>(StringComparer.Ordinal); int basic = 0, pending = 0;
            foreach (var row in rows)
            {
                if (row == null || !ids.Add(row.id)) return false;
                int maximum = OriginalSoulUpgradeRules.Maximum(row.id);
                if (maximum == 0 || row.maximumRank != maximum || row.rank < 0 || row.rank > maximum ||
                    string.IsNullOrEmpty(row.name) || row.name.Length > 128 || !NonnegativeFinite(row.remainingSeconds) || row.remainingSeconds > 1.001 ||
                    row.soulCost != (row.rank == maximum ? 0 : OriginalSoulUpgradeRules.Cost(row.id, row.rank))) return false;
                if (OriginalSoulUpgradeRules.IsBasic(row.id)) basic += row.rank;
                if (row.researching) { pending++; if (row.rank == maximum || !row.unlocked) return false; }
                else if (row.remainingSeconds != 0) return false;
            }
            if (pending > 1) return false;
            foreach (var row in rows)
                if (row.unlocked != (OriginalSoulUpgradeRules.IsBasic(row.id) || basic == 60) ||
                    OriginalSoulUpgradeRules.IsUnique(row.id) && row.rank > 0 && !row.unlocked) return false;
            return true;
        }

        static bool ValidEffects(OriginalSessionView view)
        {
            if (view.effects == null || view.effects.Length > 2048 || !view.hasWorld && view.effects.Length != 0) return false;
            foreach (var effect in view.effects)
                if (effect == null || !Enum.IsDefined(typeof(OriginalVisualEffectKind), effect.kind) || !Rawcode(effect.abilityId) ||
                    effect.sourceEntityId < 0 || effect.variant < 0 || effect.variant > 16 ||
                    !Point(effect.position.x, effect.position.y) || !Point(effect.end.x, effect.end.y) ||
                    !NonnegativeFinite(effect.radius) || effect.radius > 4096 ||
                    !NonnegativeFinite(effect.progress) || effect.progress > 1) return false;
            return true;
        }

        static bool ValidAbilities(OriginalSessionView view)
        {
            foreach (var player in view.players)
            {
                if (!player.hasProgression)
                { if (player.abilities?.Length > 0) return false; continue; }
                if (player.abilities == null || player.abilities.Length != player.progression.skills.Length) return false;
                for (int i = 0; i < player.abilities.Length; i++)
                {
                    var ability = player.abilities[i]; var skill = player.progression.skills[i];
                    if (ability == null || ability.id != skill.id || ability.rank != skill.rank ||
                        !ValidCastIdentity(player, ability) ||
                        !NonnegativeFinite(ability.manaCost) || !NonnegativeFinite(ability.cooldownRemaining) ||
                        !ability.manaCostKnown && ability.manaCost != 0 ||
                        !Enum.IsDefined(typeof(OriginalAbilityTargetMode), ability.targetMode) || ability.targetMode == OriginalAbilityTargetMode.UnitOrPoint ||
                        !Enum.IsDefined(typeof(OriginalAbilityUseCode), ability.code) ||
                        ability.toggledOn && (ability.rank == 0 || !ability.implemented &&
                            !(player.heroId == "N0A0" && ability.id == "A15X")) ||
                        ability.code == OriginalAbilityUseCode.Ready && (!ability.implemented || !ability.manaCostKnown || ability.rank == 0 || ability.cooldownRemaining > 0))
                        return false;
                }
            }
            return true;
        }

        static bool HasPyroUpgradeItem(OriginalSessionPlayerView player)
        {
            if (player.heroId != "H024" || !player.hasInventory || player.inventory?.heroSlots == null) return false;
            foreach (var item in player.inventory.heroSlots) if (!EmptyItem(item) && item.itemId == "I00Z" && !item.removed) return true;
            return false;
        }
        static bool ValidCastIdentity(OriginalSessionPlayerView player, OriginalAbilityView ability)
        {
            if (!Rawcode(ability.castAbilityId)) return false;
            if (HasPyroUpgradeItem(player) && ability.rank > 0)
            {
                if (ability.id == "A0SP") return ability.castAbilityId == "A0SR" && ability.targetMode == OriginalAbilityTargetMode.None ||
                    ability.castAbilityId == "A0SO" && ability.targetMode == OriginalAbilityTargetMode.Point;
                if (ability.id == "A0SM") return ability.castAbilityId == "A0SS" && ability.targetMode == OriginalAbilityTargetMode.Point;
            }
            return ability.castAbilityId == ability.id || player.heroId == "H024" && ability.rank > 0 &&
                (ability.id == "A0SJ" && ability.castAbilityId == "A0SN" && ability.targetMode == OriginalAbilityTargetMode.None ||
                 ability.id == "A0SP" && ability.castAbilityId == "A0SO" && ability.targetMode == OriginalAbilityTargetMode.Point);
        }

        static bool EmptyItem(OriginalItemInstance item) => item == null ||
            (item.instanceId == 0 && string.IsNullOrEmpty(item.itemId) && item.ownerId == 0 &&
             !item.chargesKnown && item.charges == 0 && !item.removed);
        static void NormalizeSlots(OriginalItemInstance[] slots)
        { for (int i = 0; i < slots.Length; i++) if (EmptyItem(slots[i])) slots[i] = null; }

        static bool ValidItems(OriginalSessionView view)
        {
            var ids = new HashSet<long>();
            bool Item(OriginalItemInstance item) => item != null && item.instanceId > 0 && ids.Add(item.instanceId) &&
                Rawcode(item.itemId) && item.ownerId >= 0 && item.ownerId <= 8 && !item.removed &&
                item.charges >= 0 && (item.chargesKnown || item.charges == 0);
            foreach (var player in view.players)
            {
                if (!Enum.IsDefined(typeof(OriginalItemActionCode), player.lastItemAction)) return false;
                var inventory = player.inventory;
                if (!player.hasInventory)
                {
                    if (player.itemUses?.Length > 0) return false;
                    if (inventory != null && (inventory.ownerId != 0 || inventory.gold != 0 || inventory.lumber != 0 ||
                        inventory.quickBuyEnabled || inventory.heroSlots?.Length > 0 || inventory.servantSlots?.Length > 0)) return false;
                    continue;
                }
                if (!view.started || inventory == null || inventory.ownerId != player.slot || inventory.gold != player.gold ||
                    inventory.lumber != player.souls || inventory.heroSlots?.Length != 6 || inventory.servantSlots?.Length != 6) return false;
                foreach (var item in inventory.heroSlots) if (!EmptyItem(item) && !Item(item)) return false;
                foreach (var item in inventory.servantSlots) if (!EmptyItem(item) && !Item(item)) return false;
                if (player.itemUses != null)
                {
                    if (player.itemUses.Length > 12) return false;
                    var uses = new HashSet<long>();
                    int occupied = 0;
                    foreach (var item in inventory.heroSlots) if (!EmptyItem(item)) occupied++;
                    foreach (var item in inventory.servantSlots) if (!EmptyItem(item)) occupied++;
                    if (player.itemUses.Length != occupied) return false;
                    foreach (var use in player.itemUses)
                    {
                        if (use == null || !Enum.IsDefined(typeof(OriginalInventoryBag), use.bag) || use.slot < 0 || use.slot >= 6 ||
                            !Enum.IsDefined(typeof(OriginalItemUseCode), use.code) || !NonnegativeFinite(use.cooldownRemaining) ||
                            !Enum.IsDefined(typeof(OriginalAbilityTargetMode), use.targetMode) ||
                            (use.itemId == "I021" || use.itemId == "I094") != (use.targetMode == OriginalAbilityTargetMode.UnitOrPoint) ||
                            !uses.Add(use.instanceId)) return false;
                        var item = (use.bag == OriginalInventoryBag.Hero ? inventory.heroSlots : inventory.servantSlots)[use.slot];
                        if (EmptyItem(item) || item.instanceId != use.instanceId || item.itemId != use.itemId) return false;
                        if (use.code == OriginalItemUseCode.Ready && (!use.implemented || use.bag != OriginalInventoryBag.Hero ||
                            use.cooldownRemaining != 0 || !item.chargesKnown || use.requiresCharge && item.charges <= 0)) return false;
                    }
                }
            }
            if (view.groundItems != null)
            {
                if (view.groundItems.Length > 8192 || !view.started && view.groundItems.Length != 0) return false;
                foreach (var ground in view.groundItems)
                    if (ground == null || !Item(ground.item) || !Point(ground.position.x, ground.position.y)) return false;
            }
            if (view.shops != null)
            {
                if (view.shops.Length > OriginalShops.Placements(view.options.compactShops).Length ||
                    !view.started && view.shops.Length != 0) return false;
                var shops = new HashSet<int>();
                foreach (var shop in view.shops)
                {
                    if (shop == null || shop.instanceId <= 0 || !shops.Add(shop.instanceId) || !Rawcode(shop.unitId) ||
                        !Point(shop.position.x, shop.position.y) || shop.stock == null || shop.stock.Length > 12) return false;
                    var offers = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var stock in shop.stock)
                        if (stock == null || !Rawcode(stock.itemId) || !offers.Add(stock.itemId) || stock.available < 0 ||
                            // A fresh shop can have a measured readiness deadline
                            // before its inventory count becomes known.
                            !NonnegativeFinite(stock.nextAvailableAt) || !stock.known && stock.available != 0) return false;
                }
            }
            return true;
        }

        private static bool ValidProgression(OriginalSessionPlayerView player, bool started)
        {
            var p = player.progression;
            if (!player.hasProgression)
                return EmptyProgression(p) && (player.learning == null || player.learning.Length == 0) &&
                    (player.auxiliaryAbilities == null || player.auxiliaryAbilities.Length == 0) &&
                    !player.archerAttackHandlerRegistered && player.bowElement == 0;
            if (!started || p == null || p.heroId != player.heroId || p.experience != player.experience ||
                p.experience < 0 || p.level < 1 || p.level > 50 || p.unspentSkillPoints < 0 || p.unspentSkillPoints > p.level ||
                p.matchExperienceWatermark < 0 || p.skills == null || p.skills.Length != 5 ||
                p.attributeBonus == null || !p.attributeBonus.known || !string.IsNullOrEmpty(p.attributeBonus.unresolved) ||
                !NonnegativeFinite(p.attributeBonus.strength) || !NonnegativeFinite(p.attributeBonus.agility) || !NonnegativeFinite(p.attributeBonus.intelligence) ||
                player.learning == null || player.learning.Length != 5 || player.auxiliaryAbilities == null || player.auxiliaryAbilities.Length > 12)
                return false;
            string[] expected;
            switch (p.heroId)
            {
                case "H008": expected = new[] { "A05N", "A05M", "A102", "A0E6", "A001" }; break;
                case "H024": expected = new[] { "A0SJ", "A0SP", "A0AE", "A0SM", "A001" }; break;
                case "N0A0": expected = new[] { "A15W", "A0AS", "A15X", "A0AC", "A001" }; break;
                default: return false;
            }
            int spent = 0;
            for (int i = 0; i < expected.Length; i++)
            {
                var skill = p.skills[i]; var choice = player.learning[i];
                if (skill == null || skill.id != expected[i] || skill.maximumRank != (i == 4 ? 15 : 3) || skill.rank < 0 || skill.rank > skill.maximumRank ||
                    choice == null || choice.id != skill.id || choice.rank != skill.rank || choice.maximumRank != skill.maximumRank ||
                    string.IsNullOrEmpty(choice.name) || choice.name.Length > 256 ||
                    !Enum.IsDefined(typeof(OriginalLearnCode), choice.code) || choice.code == OriginalLearnCode.Learned || choice.code == OriginalLearnCode.UnknownSkill ||
                    choice.requiredLevel < 0 || choice.requiredLevel > 50 || (!choice.requiredLevelKnown && choice.requiredLevel != 0) ||
                    (choice.code == OriginalLearnCode.Available && (p.unspentSkillPoints == 0 || !choice.requiredLevelKnown || p.level < choice.requiredLevel || skill.rank == skill.maximumRank)))
                    return false;
                spent += skill.rank;
            }
            if (spent + p.unspentSkillPoints != p.level) return false;
            var auxiliaries = new HashSet<string>();
            var ring = new Dictionary<string, int>();
            var pyro = new Dictionary<string, int>();
            foreach (var ability in player.auxiliaryAbilities)
            {
                if (ability == null || !Rawcode(ability.id) || !auxiliaries.Add(ability.id) || ability.rank < 1 || ability.rank > 3) return false;
                if (OriginalCurseRules.IsCurse(ability.id)) { if (ability.rank != 1) return false; continue; }
                if (ability.id == "A10H" || ability.id == "A10I" || ability.id == "A10J" || ability.id == "A0LW")
                { ring.Add(ability.id, ability.rank); continue; }
                if (p.heroId == "H024" && (ability.id == "A0SU" || ability.id == "A0SR" || ability.id == "A0SS"))
                { pyro.Add(ability.id, ability.rank); continue; }
                if (p.heroId != "N0A0" || ability.id != "A15Z" && ability.id != "A160" && ability.id != "A161" &&
                    ability.id != "A162" && ability.id != "A17M" && ability.id != "A0N6") return false;
            }
            if (ring.Count != 0 && (ring.Count != 4 || ring["A10H"] != 1 || ring["A10I"] > 2 ||
                ring["A10J"] != ring["A10I"] || ring["A0LW"] != ring["A10I"])) return false;
            if (p.heroId == "H024")
            {
                bool upgraded = HasPyroUpgradeItem(player);
                if (pyro.ContainsKey("A0SU") != upgraded || upgraded && pyro["A0SU"] != 1) return false;
                foreach (var pair in new[] { ("A0SR", 1), ("A0SS", 3) })
                {
                    int rank = p.skills[pair.Item2].rank;
                    if (pyro.ContainsKey(pair.Item1) != (upgraded && rank > 0) || pyro.TryGetValue(pair.Item1, out int aliasRank) && aliasRank != rank)
                        return false;
                }
            }
            if (p.heroId != "N0A0") return !player.archerAttackHandlerRegistered && player.bowElement == 0;
            int bowRank = p.skills[2].rank, enchantedRank = p.skills[3].rank;
            if (player.archerAttackHandlerRegistered != (bowRank > 0) ||
                (bowRank == 0 ? player.bowElement != 0 : player.bowElement < 1 || player.bowElement > 5)) return false;
            var bowHelpers = new[] { "A15Z", "A160", "A161", "A162", "A17M" };
            for (int i = 0; i < bowHelpers.Length; i++)
                if (auxiliaries.Contains(bowHelpers[i]) != (bowRank > 0 && player.bowElement == i + 1)) return false;
            if (auxiliaries.Contains("A0N6") != (enchantedRank > 0)) return false;
            foreach (var ability in player.auxiliaryAbilities)
                if (ability.id == "A0N6" && ability.rank != enchantedRank || Array.IndexOf(bowHelpers, ability.id) >= 0 && ability.rank != bowRank) return false;
            return true;
        }

        private static bool EmptyProgression(OriginalHeroProgressionSnapshot p) => p == null ||
            (string.IsNullOrEmpty(p.heroId) && p.experience == 0 && p.level == 0 && p.unspentSkillPoints == 0 && p.matchExperienceWatermark == 0 &&
             (p.skills == null || p.skills.Length == 0) && (p.attributeBonus == null || (!p.attributeBonus.known &&
              p.attributeBonus.strength == 0 && p.attributeBonus.agility == 0 && p.attributeBonus.intelligence == 0 && string.IsNullOrEmpty(p.attributeBonus.unresolved))));

        private static bool ValidWorld(OriginalSessionView view)
        {
            var world = view.world;
            if (!view.hasWorld) return world == null || (world.revision == 0 && world.navigationRevision == 0 && world.time == 0 &&
                (world.units == null || world.units.Length == 0) && (world.doodads == null || world.doodads.Length == 0));
            if (!view.started || world == null || world.revision < 0 || world.navigationRevision < 0 || !NonnegativeFinite(world.time) ||
                world.units == null || world.units.Length > 8192 || world.doodads == null || world.doodads.Length > 4096) return false;
            var units = new HashSet<int>(); var heroes = new HashSet<int>(); var doodads = new HashSet<int>();
            foreach (var unit in world.units)
            {
                if (unit == null || unit.entityId <= 0 || !units.Add(unit.entityId) || !Rawcode(unit.rawcode) ||
                    unit.ownerSlot < 0 || unit.ownerSlot > 8 || unit.attackSequence < 0 || unit.castSequence < 0 ||
                    unit.visibleToOwners < 0 || unit.visibleToOwners > 255 || unit.hidden && unit.visibleToOwners != 0 ||
                    !unit.hidden && !unit.invisible && unit.visibleToOwners != 255 ||
                    !Enum.IsDefined(typeof(OriginalWorldUnitKind), unit.kind) || unit.sourceHeroEntityId < 0 || unit.copySourceEntityId < 0 ||
                    !Enum.IsDefined(typeof(OriginalImageFactory), unit.imageFactory) ||
                    unit.kind != OriginalWorldUnitKind.Illusion && (unit.copySourceEntityId != 0 || unit.imageFactory != OriginalImageFactory.None) ||
                    !NonnegativeFinite(unit.facingDegrees) || unit.facingDegrees >= 360 || !unit.hasFacing && unit.facingDegrees != 0 ||
                    !Point(unit.position.x, unit.position.y) || !Point(unit.destination.x, unit.destination.y) ||
                    !Enum.IsDefined(typeof(OriginalWorldOrder), unit.order) || !Enum.IsDefined(typeof(OriginalWorldTargetKind), unit.targetKind) ||
                    unit.profile == null || !NonnegativeFinite(unit.profile.moveSpeed) || unit.profile.moveSpeed > 81920 ||
                    !NonnegativeFinite(unit.profile.collisionRadius) || unit.profile.collisionRadius == 0 || unit.profile.collisionRadius > 32768 ||
                    !NonnegativeFinite(unit.profile.maxHealth) || unit.profile.maxHealth == 0 || !NonnegativeFinite(unit.profile.maxMana) ||
                    !NonnegativeFinite(unit.health) || unit.health > unit.profile.maxHealth || !NonnegativeFinite(unit.mana) || unit.mana > unit.profile.maxMana)
                    return false;
                if (unit.kind == OriginalWorldUnitKind.Hero)
                {
                    var player = Array.Find(view.players, p => p.slot == unit.ownerSlot);
                    if (player == null || player.heroId != unit.rawcode || unit.entityId != unit.ownerSlot ||
                        unit.sourceHeroEntityId != 0 || !heroes.Add(unit.ownerSlot)) return false;
                }
                else if (unit.kind == OriginalWorldUnitKind.Enemy)
                { if (unit.ownerSlot != 0 || unit.entityId <= 1000 || unit.entityId >= 1000000000 ||
                        unit.entityId >= OriginalWorld.FirstSummonEntityId && unit.entityId <= OriginalWorld.LastSummonEntityId || unit.sourceHeroEntityId != 0) return false; }
                else if (unit.kind == OriginalWorldUnitKind.Summon)
                {
                    if (unit.ownerSlot == 0 || unit.entityId < OriginalWorld.FirstSummonEntityId || unit.entityId > OriginalWorld.LastSummonEntityId ||
                        unit.sourceHeroEntityId != unit.ownerSlot || !Array.Exists(view.players, p => p.slot == unit.ownerSlot)) return false;
                }
                else if (!ValidImageLineage(view, unit)) return false;
                if (unit.holding && (unit.health <= 0 || unit.order == OriginalWorldOrder.Move || unit.approaching)) return false;
                if (unit.order == OriginalWorldOrder.AttackTarget)
                { if (unit.targetKind == OriginalWorldTargetKind.None || unit.targetId < 0 || unit.targetKind == OriginalWorldTargetKind.Unit && unit.targetId == 0) return false; }
                else if (unit.targetKind != OriginalWorldTargetKind.None || unit.targetId != 0 || unit.approaching) return false;
            }
            if (heroes.Count != view.players.Length) return false;
            foreach (var d in world.doodads)
            {
                if (d == null || d.editorId < 0 || !doodads.Add(d.editorId) || !Rawcode(d.rawcode) || !Point(d.position.x, d.position.y) ||
                    !NonnegativeFinite(d.maxHealth) || d.maxHealth == 0 || !NonnegativeFinite(d.health) || d.health > d.maxHealth ||
                    !NonnegativeFinite(d.facingDegrees) || d.facingDegrees >= 360 || !NonnegativeFinite(d.scale) || d.scale == 0 || d.scale > 8) return false;
                if (d.dynamic)
                {
                    if (d.editorId < OriginalWorld.FirstDynamicDoodadId || d.rawcode != "B009" || !d.invulnerable ||
                        d.maxHealth != 9999 || d.health != d.maxHealth || d.facingDegrees % 90 != 0) return false;
                }
                else if (d.editorId >= OriginalWorld.FirstDynamicDoodadId ||
                    d.invulnerable && (d.rawcode != "LTex" || !view.options.explosiveBarrels) ||
                    d.facingDegrees != 0 || d.scale != 1) return false;
            }
            foreach (var unit in world.units)
                if (unit.order == OriginalWorldOrder.AttackTarget && (unit.targetKind == OriginalWorldTargetKind.Unit ?
                    unit.targetId == unit.entityId || !units.Contains(unit.targetId) : !doodads.Contains(unit.targetId))) return false;
            return true;
        }

        private static bool ValidImageLineage(OriginalSessionView view, OriginalWorldUnitView unit)
        {
            if (unit.entityId < 1000000000 || unit.copySourceEntityId == unit.entityId) return false;
            if (unit.imageFactory == OriginalImageFactory.ItemWand)
            {
                if (unit.ownerSlot < 1 || unit.ownerSlot > 8 || !Array.Exists(view.players,p=>p.slot==unit.ownerSlot)) return false;
                var donor=Array.Find(view.world.units,u=>u!=null&&u.entityId==unit.copySourceEntityId);
                if (unit.copySourceEntityId >= 1 && unit.copySourceEntityId <= 8)
                {
                    int slot=unit.copySourceEntityId;
                    return unit.sourceHeroEntityId==slot && Array.Exists(view.players,p=>p.slot==slot&&p.heroId==unit.rawcode) &&
                        (donor==null||donor.kind==OriginalWorldUnitKind.Hero&&donor.ownerSlot==slot&&donor.rawcode==unit.rawcode);
                }
                if (unit.sourceHeroEntityId!=0 || unit.copySourceEntityId<=1000 || unit.copySourceEntityId>=1000000000 ||
                    unit.copySourceEntityId>=OriginalWorld.FirstSummonEntityId&&unit.copySourceEntityId<=OriginalWorld.LastSummonEntityId) return false;
                return donor==null||donor.kind==OriginalWorldUnitKind.Enemy&&donor.ownerSlot==0&&donor.rawcode==unit.rawcode;
            }
            if (unit.imageFactory == OriginalImageFactory.KnightMirror || unit.imageFactory == OriginalImageFactory.EnemyWand)
            {
                int sourceSlot = unit.copySourceEntityId;
                if (sourceSlot < 1 || sourceSlot > 8 || unit.sourceHeroEntityId != sourceSlot ||
                    unit.ownerSlot != (unit.imageFactory == OriginalImageFactory.KnightMirror ? sourceSlot : 0)) return false;
                var player = Array.Find(view.players, p => p.slot == sourceSlot);
                if (player == null || player.heroId != unit.rawcode) return false;
                var donor = Array.Find(view.world.units, u => u != null && u.entityId == sourceSlot);
                return donor == null || donor.kind == OriginalWorldUnitKind.Hero && donor.ownerSlot == sourceSlot && donor.rawcode == unit.rawcode;
            }
            if (unit.imageFactory != OriginalImageFactory.BossMirror || unit.ownerSlot != 0 || unit.sourceHeroEntityId != 0 ||
                unit.rawcode != "n017" || unit.copySourceEntityId <= 1000 || unit.copySourceEntityId >= 1000000000 ||
                unit.copySourceEntityId >= OriginalWorld.FirstSummonEntityId && unit.copySourceEntityId <= OriginalWorld.LastSummonEntityId) return false;
            var source = Array.Find(view.world.units, u => u != null && u.entityId == unit.copySourceEntityId);
            return source == null || source.kind == OriginalWorldUnitKind.Enemy && source.ownerSlot == 0 && source.rawcode == unit.rawcode;
        }

        private static bool Rawcode(string value) => value != null && value.Length == 4;
        private static bool Point(double x, double y) => !double.IsNaN(x) && !double.IsNaN(y) && Math.Abs(x) <= 1048576 && Math.Abs(y) <= 1048576;

        private static bool ValidDuel(OriginalSessionView view)
        {
            if (view.duelId < 0 || !Enum.IsDefined(typeof(OriginalDuelKind), view.duelKind)) return false;
            // Unity's inline class serialization materializes a null nested DTO
            // as an all-default object. Presence is explicit on the wire.
            if (!view.hasDuel && !EmptyDuel(view.duel)) return false;
            if (!view.started) return !view.pendingDuel && view.duelId == 0 && !view.hasDuel;
            if (view.pendingDuel && (view.phase != OriginalMatchPhase.Duel || view.initialParticipants < 2)) return false;
            var duel = view.duel;
            if (!view.hasDuel) return view.duelId == 0;
            if (duel == null) return false;
            int count = view.initialParticipants;
            if (view.duelId == 0 || count < 2 || !Enum.IsDefined(typeof(OriginalDuelKind), duel.kind) ||
                !Enum.IsDefined(typeof(OriginalDuelPhase), duel.phase) || duel.phase == OriginalDuelPhase.NotStarted ||
                duel.completedRound < 4 || duel.completedRound > 29 || duel.completedRound % 5 != 4 ||
                duel.completedRound > view.round || duel.pair < 1 || duel.pair > 4 ||
                duel.firstSlot < 0 || duel.firstSlot > count || duel.secondSlot < 0 || duel.secondSlot > count ||
                duel.remaining < 0 || duel.remaining > count || duel.carryPrizeGold < 0 ||
                !NonnegativeFinite(duel.time) || !NonnegativeFinite(duel.phaseEndsAt) ||
                !NonnegativeFinite(duel.ringRadius) || duel.ringRadius > 810 || duel.ringStage < 0 || duel.ringStage > 2 ||
                duel.participants == null || duel.participants.Length != count ||
                duel.alive == null || duel.alive.Length != count + 1 || duel.alive[0] ||
                duel.betSides == null || duel.betSides.Length != count + 1 || duel.betSides[0] != 0 ||
                duel.betStakes == null || duel.betStakes.Length != count + 1 || duel.betStakes[0] != 0) return false;
            if (duel.phase != OriginalDuelPhase.Completed &&
                (view.pendingDuel || view.phase != OriginalMatchPhase.Duel || view.duelKind != duel.kind)) return false;
            var slots = new bool[count + 1];
            foreach (var participant in duel.participants)
            {
                if (participant == null || participant.slot < 1 || participant.slot > count || slots[participant.slot]) return false;
                slots[participant.slot] = true;
                var player = Array.Find(view.players, p => p.matchSlot == participant.slot);
                if (player == null || player.heroId != participant.heroRawcode) return false;
            }
            for (int i = 1; i <= count; i++)
                if (duel.betSides[i] < 0 || duel.betSides[i] > 2 || duel.betStakes[i] < 0 || duel.betStakes[i] > 1000) return false;
            return true;
        }

        private static bool EmptyDuel(OriginalDuelSnapshot d) => d == null ||
            (d.kind == OriginalDuelKind.Pairs && d.phase == OriginalDuelPhase.NotStarted && d.completedRound == 0 &&
             d.pair == 0 && d.firstSlot == 0 && d.secondSlot == 0 && d.remaining == 0 && d.carryPrizeGold == 0 &&
             d.ringStage == 0 && d.time == 0 && d.phaseEndsAt == 0 && d.ringRadius == 0 && !d.betsOpen &&
             string.IsNullOrEmpty(d.unresolved) && (d.participants == null || d.participants.Length == 0) &&
             (d.alive == null || d.alive.Length == 0) && (d.betSides == null || d.betSides.Length == 0) &&
             (d.betStakes == null || d.betStakes.Length == 0));

        private static bool NonnegativeFinite(double value) => value >= 0 && !double.IsInfinity(value);

        private static bool ValidHash(string value)
        {
            if (value == null || value.Length != 64) return false;
            foreach (var c in value)
                if (!(c >= '0' && c <= '9') && !(c >= 'a' && c <= 'f')) return false;
            return true;
        }

        private static bool Read(byte[] bytes, int maximum, out string json)
        {
            json = null;
            if (bytes == null || bytes.Length == 0 || bytes.Length > maximum) return false;
            try { json = Utf8.GetString(bytes).Trim(); }
            catch (DecoderFallbackException) { return false; }
            if (json.Length < 2 || json[0] != '{' || json[json.Length - 1] != '}') return false;
            // Bound nesting before invoking Unity's native parser. This checks
            // structure outside strings, not braces inside serialized text.
            int depth = 0;
            bool quoted = false, escaped = false;
            for (var i = 0; i < json.Length; i++)
            {
                var c = json[i];
                if (quoted)
                {
                    if (escaped) escaped = false;
                    else if (c == '\\') escaped = true;
                    else if (c == '"') quoted = false;
                    continue;
                }
                if (c == '"') quoted = true;
                else if (c == '{' || c == '[') { if (++depth > 24) return false; }
                else if (c == '}' || c == ']')
                {
                    if (--depth < 0 || (depth == 0 && i != json.Length - 1)) return false;
                }
            }
            return depth == 0 && !quoted;
        }
    }
}
