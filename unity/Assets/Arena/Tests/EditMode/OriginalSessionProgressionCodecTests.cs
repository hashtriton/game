using System;
using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionProgressionCodecTests
    {
        readonly OriginalUnitySessionCodec codec = new OriginalUnitySessionCodec();
        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + file)));
        static OriginalSession Create(bool start, string hero = "H008")
        {
            var session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match.json"), Load<OriginalItemCatalog>("lia39-items.json"),
                Load<OriginalCombatCatalog>("lia39-combat.json"), Load<OriginalDuelCatalog>("lia39-duels.json"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            session.ConfigureProgression(Load<OriginalNativeCatalog>("lia39-native126.json"), Load<OriginalObservedCatalog>("lia39-observed126.json"));
            if (start)
            {
                session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = hero });
                session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
                session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start });
            }
            return session;
        }
        static OriginalNetworkResponse Response(OriginalSession session)
        {
            var view = session.Snapshot();
            return new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = 1,
                code = OriginalSessionReplyCode.Accepted, acknowledgedSequence = view.players[0].acknowledgedSequence, snapshot = view };
        }

        static OriginalNetworkResponse UpgradedPyro(bool learned)
        {
            var response = Response(Create(true, "H024")); var p = response.snapshot.players[0];
            p.inventory.heroSlots[0] = new OriginalItemInstance { instanceId = 9001, itemId = "I00Z", ownerId = 1 };
            p.itemUses = new[] { new OriginalItemUseView { instanceId = 9001, itemId = "I00Z", bag = OriginalInventoryBag.Hero, slot = 0,
                code = OriginalItemUseCode.RuleUnavailable } };
            p.auxiliaryAbilities = learned ? new[] { new OriginalLearnedAbilityView { id = "A0SU", rank = 1 },
                new OriginalLearnedAbilityView { id = "A0SR", rank = 1 }, new OriginalLearnedAbilityView { id = "A0SS", rank = 1 } } :
                new[] { new OriginalLearnedAbilityView { id = "A0SU", rank = 1 } };
            if (learned)
            {
                p.progression.level = 10; p.progression.unspentSkillPoints = 8;
                foreach (int i in new[] { 1, 3 })
                {
                    p.progression.skills[i].rank = 1; p.learning[i].rank = 1; p.abilities[i].rank = 1;
                    p.abilities[i].castAbilityId = i == 1 ? "A0SR" : "A0SS";
                    p.abilities[i].targetMode = i == 1 ? OriginalAbilityTargetMode.None : OriginalAbilityTargetMode.Point;
                }
            }
            return response;
        }

        static OriginalNetworkResponse DynamicDoodadResponse()
        {
            var response = Response(Create(true)); response.snapshot.hasWorld = true;
            response.snapshot.world = new OriginalWorldSnapshot { units = new[] { new OriginalWorldUnitView {
                    entityId = 1, ownerSlot = 1, rawcode = "H008", kind = OriginalWorldUnitKind.Hero, health = 631, mana = 145,
                    profile = new OriginalWorldUnitProfile { maxHealth = 631, maxMana = 145, moveSpeed = 250, collisionRadius = 24 } } },
                doodads = new[] { new OriginalWorldDoodadView { editorId = 1000000, rawcode = "B009", dynamic = true,
                    invulnerable = true, maxHealth = 9999, health = 9999, position = new OriginalPoint(0, 1000), scale = 1.2 } } };
            return response;
        }

        [Test] public void EnemyImageWireUsesFactorySpecificLineageAndAllowsRemovedEnemyDonor()
        {
            OriginalNetworkResponse ImageResponse(bool boss)
            {
                var response = DynamicDoodadResponse(); var hero = response.snapshot.world.units[0];
                response.snapshot.world.units = new[] { hero, new OriginalWorldUnitView {
                    entityId = 1000000000, ownerSlot = 0, kind = OriginalWorldUnitKind.Illusion,
                    imageFactory = boss ? OriginalImageFactory.BossMirror : OriginalImageFactory.EnemyWand,
                    sourceHeroEntityId = boss ? 0 : 1, copySourceEntityId = boss ? 1001 : 1,
                    rawcode = boss ? "n017" : hero.rawcode, position = new OriginalPoint(100, 1000), destination = new OriginalPoint(100, 1000),
                    health = 300, mana = 0, profile = new OriginalWorldUnitProfile { maxHealth = 300, maxMana = 0, moveSpeed = 250, collisionRadius = 16 } } };
                return response;
            }
            foreach (bool boss in new[] { false, true })
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(ImageResponse(boss)), out _), Is.True);
            foreach (var mutate in new Action<OriginalWorldUnitView>[] { u => u.ownerSlot = 1,
                u => u.sourceHeroEntityId = 2, u => u.copySourceEntityId = u.entityId,
                u => u.copySourceEntityId = 1001, u => u.rawcode = "n017", u => u.imageFactory = OriginalImageFactory.None,
                u => u.imageFactory = OriginalImageFactory.BossMirror, u => u.kind = OriginalWorldUnitKind.Enemy })
            {
                var response = ImageResponse(false); mutate(response.snapshot.world.units[1]);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            }
            var mismatch = ImageResponse(true); var donor = JsonUtility.FromJson<OriginalWorldUnitView>(JsonUtility.ToJson(mismatch.snapshot.world.units[1]));
            donor.entityId = 1001; donor.kind = OriginalWorldUnitKind.Enemy; donor.rawcode = "n008";
            donor.copySourceEntityId = 0; donor.imageFactory = OriginalImageFactory.None;
            mismatch.snapshot.world.units = new[] { mismatch.snapshot.world.units[0], mismatch.snapshot.world.units[1], donor };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(mismatch), out _), Is.False);
        }

        [Test] public void SummonWireRequiresItsOwnNamespaceOwnerAndCanonicalAncestry()
        {
            OriginalNetworkResponse SummonResponse()
            {
                var response = DynamicDoodadResponse();
                var hero = response.snapshot.world.units[0];
                response.snapshot.world.units = new[] { hero, new OriginalWorldUnitView {
                    entityId = 100000000, ownerSlot = 1, sourceHeroEntityId = 1, kind = OriginalWorldUnitKind.Summon,
                    rawcode = "n001", position = new OriginalPoint(100, 1000), destination = new OriginalPoint(100, 1000),
                    health = 300, mana = 0, profile = new OriginalWorldUnitProfile { maxHealth = 300, maxMana = 0, moveSpeed = 250, collisionRadius = 16 } } };
                response.snapshot.unitAbilities = new[] { new OriginalUnitAbilityView {
                    entityId = 100000000, abilities = Array.Empty<OriginalAbilityView>() } };
                return response;
            }
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(SummonResponse()), out var decoded), Is.True);
            Assert.That(decoded.snapshot.world.units[1].kind, Is.EqualTo(OriginalWorldUnitKind.Summon));
            Assert.That(decoded.snapshot.unitAbilities[0].entityId, Is.EqualTo(100000000));
            Assert.That(decoded.snapshot.unitAbilities[0].abilities, Is.Empty);
            var missingGroup = SummonResponse(); missingGroup.snapshot.unitAbilities = Array.Empty<OriginalUnitAbilityView>();
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(missingGroup), out _), Is.False);
            foreach (var mutate in new Action<OriginalWorldUnitView>[] { u => u.ownerSlot = 0, u => u.ownerSlot = 2,
                u => u.sourceHeroEntityId = 2, u => u.sourceHeroEntityId = 0, u => u.entityId = 99999999,
                u => u.entityId = 500000000, u => u.kind = OriginalWorldUnitKind.Illusion,
                u => { u.kind = OriginalWorldUnitKind.Enemy; u.ownerSlot = 0; u.sourceHeroEntityId = 0; } })
            {
                var response = SummonResponse(); mutate(response.snapshot.world.units[1]);
                response.snapshot.unitAbilities[0].entityId = response.snapshot.world.units[1].entityId;
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            }
        }

        [Test] public void DynamicDoodadWireRejectsIdentityMaskAndGeometryConflicts()
        {
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(DynamicDoodadResponse()), out var read), Is.True);
            Assert.That(read.snapshot.world.doodads[0].dynamic, Is.True);
            foreach (var mutate in new Action<OriginalWorldDoodadView>[] {
                d => d.editorId = 1, d => d.dynamic = false, d => d.rawcode = "LTbr", d => d.invulnerable = false,
                d => d.health = 9998, d => d.maxHealth = 10000, d => d.scale = 0, d => d.scale = 9, d => d.facingDegrees = 45 })
            {
                var response = DynamicDoodadResponse(); mutate(response.snapshot.world.doodads[0]);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            }
        }

        [Test] public void PyroUpgradeAliasesRequireHeroItemAndExactLearnedCanonicalRanks()
        {
            foreach (bool learned in new[] { false, true })
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(UpgradedPyro(learned)), out _), Is.True);
            var orbit = UpgradedPyro(true); orbit.snapshot.players[0].abilities[1].castAbilityId = "A0SO";
            orbit.snapshot.players[0].abilities[1].targetMode = OriginalAbilityTargetMode.Point;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(orbit), out _), Is.True);
            foreach (var mutate in new Action<OriginalSessionPlayerView>[] {
                p => { p.inventory.heroSlots[0] = null; p.itemUses = Array.Empty<OriginalItemUseView>(); },
                p => { p.inventory.servantSlots[0] = p.inventory.heroSlots[0]; p.inventory.heroSlots[0] = null; p.itemUses[0].bag = OriginalInventoryBag.Servant; },
                p => p.auxiliaryAbilities[0].rank = 2,
                p => p.auxiliaryAbilities[1].rank = 2,
                p => p.auxiliaryAbilities = new[] { p.auxiliaryAbilities[1], p.auxiliaryAbilities[2] },
                p => p.abilities[1].targetMode = OriginalAbilityTargetMode.Point,
                p => p.abilities[3].castAbilityId = "A0SR",
                p => p.abilities[1].castAbilityId = "A0SP" })
            {
                var invalid = UpgradedPyro(true); mutate(invalid.snapshot.players[0]);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(invalid), out _), Is.False);
            }
            var unlearned = UpgradedPyro(false);
            unlearned.snapshot.players[0].auxiliaryAbilities = new[] { new OriginalLearnedAbilityView { id = "A0SU", rank = 1 },
                new OriginalLearnedAbilityView { id = "A0SR", rank = 1 } };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(unlearned), out _), Is.False);
        }

        [Test] public void ProgressionPresenceAndFiveChoicesSurviveActualUnityJsonSerialization()
        {
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(Response(Create(false))), out var lobby), Is.True);
            Assert.That(lobby.snapshot.players[0].hasProgression, Is.False);
            Assert.That(lobby.snapshot.players[0].progression, Is.Null);
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(Response(Create(true))), out var game), Is.True);
            var player = game.snapshot.players[0];
            Assert.That(player.hasProgression, Is.True); Assert.That(player.progression.level, Is.EqualTo(1));
            Assert.That(player.learning.Length, Is.EqualTo(5)); Assert.That(player.learning[0].id, Is.EqualTo("A05N"));
        }

        [Test] public void InvalidProgressionCannotInjectRanksPointsOrForeignHelperAbilities()
        {
            var session = Create(true);
            foreach (var mutate in new Action<OriginalSessionPlayerView>[] {
                p => p.hasProgression = false, p => p.progression = null, p => p.progression.heroId = "H024",
                p => p.progression.level = 51, p => p.progression.unspentSkillPoints = 2,
                p => p.progression.matchExperienceWatermark = -1, p => p.progression.skills[0].rank = 1,
                p => p.progression.skills[0].id = "A15X", p => p.progression.attributeBonus.known = false,
                p => p.progression.attributeBonus.strength = -1, p => p.learning = Array.Empty<OriginalSessionSkillView>(),
                p => p.learning[0].code = (OriginalLearnCode)999, p => p.learning[0].rank = 2,
                p => p.learning[0].requiredLevel = 60, p => p.learning[0].name = new string('x',257),
                p => p.auxiliaryAbilities = new[] { new OriginalLearnedAbilityView { id = "A15Z", rank = 1 } },
                p => p.archerAttackHandlerRegistered = true, p => p.bowElement = 1 })
            {
                var response = Response(session); mutate(response.snapshot.players[0]);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            }
        }

        [Test] public void LearnCommandCarriesOnlyABoundedSkillIdentity()
        {
            var command = new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05N" };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out var decoded), Is.True);
            Assert.That(decoded.skillId, Is.EqualTo("A05N"));
            foreach (var invalid in new[] { null, "", "A05", "A05NN" })
            {
                command.skillId = invalid;
                Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.False);
            }
        }

        [Test] public void SecondaryPyroActionKeepsItsLearningIdentityAndRejectsForeignOrUnlearnedAliases()
        {
            var session = Create(true, "H024");
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 4,
                kind = OriginalSessionCommandKind.LearnSkill, skillId = "A0SJ" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var response = Response(session); var action = response.snapshot.players[0].abilities[0];
            action.castAbilityId = "A0SN"; action.targetMode = OriginalAbilityTargetMode.None;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out var decoded), Is.True);
            Assert.That(decoded.snapshot.players[0].abilities[0].id, Is.EqualTo("A0SJ"));
            Assert.That(decoded.snapshot.players[0].abilities[0].castAbilityId, Is.EqualTo("A0SN"));
            foreach (var mutate in new Action<OriginalSessionPlayerView>[] {
                p => p.abilities[0].castAbilityId = "A0SO",
                p => p.abilities[0].targetMode = OriginalAbilityTargetMode.Point,
                p => { p.abilities[1].castAbilityId = "A0SO"; p.abilities[1].targetMode = OriginalAbilityTargetMode.Point; },
                p => p.abilities[0].castAbilityId = null })
            {
                var invalid = codec.EncodeResponse(response);
                Assert.That(codec.TryDecodeResponse(invalid, out var candidate), Is.True);
                mutate(candidate.snapshot.players[0]);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(candidate), out _), Is.False);
            }
            var knight = Response(Create(true)); knight.snapshot.players[0].abilities[0].castAbilityId = "A0SN";
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(knight), out _), Is.False);
        }

        [Test] public void GlobalCurseHelpersAreAllowedButIncompleteOrInconsistentRingBooksAreRejected()
        {
            var session = Create(true); var response = Response(session);
            response.snapshot.players[0].auxiliaryAbilities = new[] {
                new OriginalLearnedAbilityView { id = "A19P", rank = 1 }, new OriginalLearnedAbilityView { id = "A19Q", rank = 1 } };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.True);
            foreach (var ids in new[] { new[] { "A10H" }, new[] { "A10H", "A10I", "A10J", "A0LW" } })
            {
                response = Response(session); var abilities = new OriginalLearnedAbilityView[ids.Length];
                for (int i = 0; i < ids.Length; i++) abilities[i] = new OriginalLearnedAbilityView { id = ids[i], rank = i == 2 ? 2 : 1 };
                response.snapshot.players[0].auxiliaryAbilities = abilities;
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            }
        }
    }
}
