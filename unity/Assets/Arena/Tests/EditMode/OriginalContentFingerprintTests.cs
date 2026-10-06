using System;
using System.Text;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalContentFingerprintTests
    {
        private static OriginalContentPart Part(string name, string value) =>
            new OriginalContentPart(name, Encoding.UTF8.GetBytes(value));

        [Test]
        public void Fingerprint_IsIndependentOfEnumerationOrder()
        {
            var first = new[] { Part("match.json", "{}"), Part("items.json", "[1]") };
            var second = new[] { first[1], first[0] };
            var hash = OriginalContentFingerprint.Compute("rules-1", first);
            // Independent vector from Python hashlib + big-endian struct.pack.
            Assert.That(hash, Is.EqualTo("f6e5ac71695bc2895563c972a6471861048d8b8f6150584952b494de77331c69"));
            Assert.That(OriginalContentFingerprint.Compute("rules-1", second), Is.EqualTo(hash));
        }

        [Test]
        public void Fingerprint_CoversRawBytesNamesRulesVersionAndAdditionalCatalogs()
        {
            var original = OriginalContentFingerprint.Compute("rules-1", new[] { Part("a.json", "{}") });
            Assert.That(OriginalContentFingerprint.Compute("rules-2", new[] { Part("a.json", "{}") }), Is.Not.EqualTo(original));
            Assert.That(OriginalContentFingerprint.Compute("rules-1", new[] { Part("b.json", "{}") }), Is.Not.EqualTo(original));
            Assert.That(OriginalContentFingerprint.Compute("rules-1", new[] { Part("a.json", "{}\n") }), Is.Not.EqualTo(original));
            Assert.That(OriginalContentFingerprint.Compute("rules-1", new[] { Part("a.json", "{}"), Part("duels.json", "[]") }), Is.Not.EqualTo(original));
            Assert.That(OriginalContentFingerprint.Compute("ab", new[] { Part("c", "d") }),
                Is.Not.EqualTo(OriginalContentFingerprint.Compute("a", new[] { Part("bc", "d") })));
        }

        [Test]
        public void Fingerprint_RejectsMissingAndAmbiguousParts()
        {
            Assert.Throws<ArgumentException>(() => OriginalContentFingerprint.Compute("", new[] { Part("a", "{}") }));
            Assert.Throws<ArgumentException>(() => OriginalContentFingerprint.Compute("rules", Array.Empty<OriginalContentPart>()));
            Assert.Throws<ArgumentException>(() => OriginalContentFingerprint.Compute("rules", new[] { Part("a", "{}"), Part("a", "[]") }));
            Assert.Throws<ArgumentException>(() => OriginalContentFingerprint.Compute("rules", new[] { Part("", "{}") }));
            Assert.Throws<ArgumentException>(() => OriginalContentFingerprint.Compute("rules", new[] { new OriginalContentPart("a", null) }));
        }

        [Test]
        public void Fingerprint_RejectsIllFormedUtf16InsteadOfHashingReplacementCharacters()
        {
            Assert.Throws<EncoderFallbackException>(() => OriginalContentFingerprint.Compute("rules\ud800", new[] { Part("a", "{}") }));
            Assert.Throws<EncoderFallbackException>(() => OriginalContentFingerprint.Compute("rules", new[] { Part("a\ud800", "{}") }));
        }
    }
}
