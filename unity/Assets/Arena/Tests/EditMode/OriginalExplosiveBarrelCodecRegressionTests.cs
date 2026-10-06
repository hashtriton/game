using System;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    // Uses the real three-player Session setup and codec.
    public sealed class OriginalExplosiveBarrelCodecRegressionTests
    {
        static OriginalSession CreateThreePlayers()
        {
            var factory = typeof(OriginalSessionRuneTests).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static);
            return (OriginalSession)factory.Invoke(null, new object[] { null, null, true, true, 3, "H008" });
        }
        static bool Decode(OriginalSessionView view)
        {
            var codec = new OriginalUnitySessionCodec();
            var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot,
                code = OriginalSessionReplyCode.Accepted, assignedSlot = 1,
                acknowledgedSequence = view.players[0].acknowledgedSequence, snapshot = view };
            return codec.TryDecodeResponse(codec.EncodeResponse(response), out _);
        }
        [Test] public void ThreePlayerStandardProtectedExplosiveBarrelSnapshotRoundTrips()
        {
            var view = CreateThreePlayers().Snapshot();
            Assert.That(view.options.difficulty, Is.EqualTo(OriginalDifficulty.Standard));
            Assert.That(view.world.doodads.Single(d => d.rawcode == "LTex").invulnerable, Is.True);
            Assert.That(Decode(view), Is.True);
        }
        [Test] public void ForgedInvulnerabilityOnOrdinaryAuthoredBarrelIsRejected()
        {
            var view = CreateThreePlayers().Snapshot();
            Assert.That(Decode(view), Is.True,"The unmodified authority snapshot must be valid before the mutation.");
            view.world.doodads.Single(d => d.rawcode == "LTbr").invulnerable = true;
            Assert.That(Decode(view), Is.False);
        }
        [Test] public void ProtectedExplosiveBarrelCannotAppearWhenItsSourceOptionIsDisabled()
        {
            var view = CreateThreePlayers().Snapshot();
            Assert.That(Decode(view), Is.True);
            view.options.explosiveBarrels=false;
            view.options.difficulty=view.options.ClassifyDifficulty();
            Assert.That(Decode(view), Is.False);
        }
    }
}
