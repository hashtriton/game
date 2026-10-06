using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        double nextDisconnectedAi = .25;
        int disconnectedAiIndex;
        readonly Dictionary<int, int> disconnectedAttackCycles = new Dictionary<int, int>();

        // 3.9c J3:17881 and ebv:23465 transfer a departed hero to computer
        // control and apply Zz's current-level choice. Existing inventory and
        // attributes remain authoritative; no fabricated rewards or levels.
        void LearnDisconnectedSkill(Player player)
        {
            if (player.connected || player.progression == null) return;
            int level = player.progression.Level;
            int index = level == 1 || level == 4 || level == 6 ? 0 :
                level == 2 || level == 7 || level == 8 ? 1 :
                level == 3 || level == 10 || level == 11 ? 2 :
                level == 5 || level == 9 || level == 13 ? 3 :
                level == 12 || level >= 14 && level <= 27 ? 4 : -1;
            if (index >= 0) LearnSkill(player, combatCatalog.Hero(player.hero).skills[index]);
        }

        // AI/oBv:86183,25401 visits one roster slot every .25s, including
        // connected slots. Group enumeration is replaced with stable world ID
        // order; this is a deterministic host adaptation, not native RNG replay.
        void AdvanceDisconnectedAi()
        {
            AdvanceDisconnectedShopping();
            AdvanceDisconnectedGroundPickup();
            if (world.Clock + 1e-9 < nextDisconnectedAi) return;
            nextDisconnectedAi = world.Clock + .25;
            if (players.Count == 0) return;
            var player = players[disconnectedAiIndex++ % players.Count];
            if (player.connected) return;
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused) return;
            bool fighting = DuelActive || match.Phase == OriginalMatchPhase.Combat || match.Phase == OriginalMatchPhase.FinalIntermission;
            if (!fighting) { DisconnectedPreparationPickup(player,actor); return; }
            if (AbilityControlsActor(actor.entityId)) return;
            AdvanceDisconnectedBattleMovement(player,actor);
            OriginalWorldUnitView target;
            UseDisconnectedSourceItems(player, actor);
            actor=world.UnitState(actor.entityId);
            if(actor==null||actor.health<=.405||actor.hidden||actor.paused)return;
            DisconnectedCombatPickup(player,actor);
            // xXv24266 / xSv24753 / x6v25071. The Unity command adapter uses
            // canonical skills so temporary secondary orders and upgraded
            // Pyromancer aliases share exactly the human resource/cooldown gate.
            if (player.hero == "H008")
            {
                if (DisconnectedAiEnemy(actor, 700) != null) DisconnectedAiCast(player, "A05N", actor.position);
                if (DisconnectedAiEnemy(actor, 300) != null) DisconnectedAiCast(player, "A102", actor.position);
                foreach (var ally in world.Snapshot().units)
                    if (ally.health > .405 && IsNativeHeroPredicate(ally) && !AreEnemies(actor.ownerSlot, ally.ownerSlot) &&
                        ally.health <= ally.profile.maxHealth * .7 && SquaredDistance(actor.position, ally.position) <= 100 * 100)
                    { DisconnectedAiCast(player, "A0E6", actor.position); break; }
            }
            else if (player.hero == "N0A0")
            {
                target = DisconnectedAiEnemy(actor, 800);
                if (target != null) DisconnectedAiCast(player, "A15W", target.position);
                if (DisconnectedAiEnemy(actor, 700) != null) DisconnectedAiCast(player, "A0AS", actor.position);
                if (DisconnectedAiEnemy(actor, 600) != null) DisconnectedAiCast(player, "A15X", actor.position);
            }
            else if (player.hero == "H024")
            {
                target = DisconnectedAiEnemy(actor, 900);
                if (target != null)
                {
                    DisconnectedAiCast(player, "A0SJ", target.position, "A0SJ");
                    DisconnectedAiCast(player, "A0SM", target.position);
                    if (DisconnectedAiEnemy(actor, 700) != null)
                    { DisconnectedAiCast(player, "A0AE", target.position); DisconnectedAiCast(player, "A0SP", target.position, "A0SO"); }
                }
                if (DisconnectedAiEnemy(actor, 800) != null)
                {
                    // Temporary berserk A0SN has the same order name as the
                    // sphere creator. Never interpret monsoon as detonation.
                    var point = target?.position ?? actor.position;
                    if (!DisconnectedAiCast(player, "A0SJ", point, "A0SN"))
                        DisconnectedAiCast(player, "A0SP", point, "A0SP", "A0SR");
                }
            }
        }

        OriginalWorldUnitView DisconnectedAiEnemy(OriginalWorldUnitView actor, double radius)
        {
            foreach (var target in world.Snapshot().units)
                if (target.health > .405 && !target.hidden && !target.invulnerable && AreEnemies(actor.ownerSlot, target.ownerSlot) &&
                    !HasEffectiveUnitAbility(target, "A0K4") && SquaredDistance(actor.position, target.position) <= radius * radius) return target;
            return null;
        }
        bool DisconnectedAiCast(Player player, string skill, OriginalPoint point, params string[] actualOrders)
        {
            var view = Array.Find(AbilityViews(player), value => value.id == skill);
            if (view == null || view.code != OriginalAbilityUseCode.Ready ||
                actualOrders.Length > 0 && Array.IndexOf(actualOrders, view.castAbilityId) < 0) return false;
            return ApplyCastCommand(player, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill,
                skillId = view.castAbilityId, x = point.x, y = point.y }) == OriginalSessionReplyCode.Accepted;
        }
    }
}
