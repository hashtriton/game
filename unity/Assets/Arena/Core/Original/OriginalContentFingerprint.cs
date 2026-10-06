using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Arena.Original
{
    public sealed class OriginalContentPart
    {
        public string Name { get; }
        public byte[] Bytes { get; }
        public OriginalContentPart(string name, byte[] bytes) { Name = name; Bytes = bytes; }
    }

    public static class OriginalContentFingerprint
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        // Feed the exact bytes used to load every runtime catalog, including layout
        // and duels. Increment rulesVersion whenever executable rules change.
        public static string Compute(string rulesVersion, IReadOnlyList<OriginalContentPart> parts)
        {
            if (string.IsNullOrWhiteSpace(rulesVersion)) throw new ArgumentException("Missing rules version.", nameof(rulesVersion));
            if (parts == null || parts.Count == 0) throw new ArgumentException("Missing content parts.", nameof(parts));
            var sorted = new List<OriginalContentPart>(parts.Count);
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var part in parts)
            {
                if (part == null || string.IsNullOrWhiteSpace(part.Name) || part.Bytes == null ||
                    part.Bytes.Length == 0 || !names.Add(part.Name))
                    throw new ArgumentException("Content parts need unique names and nonempty bytes.", nameof(parts));
                sorted.Add(part);
            }
            sorted.Sort((a, b) => StringComparer.Ordinal.Compare(a.Name, b.Name));
            using (var hash = SHA256.Create())
            {
                using (var stream = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write))
                {
                    WritePart(stream, Utf8.GetBytes("Arena.Original.Content.v1"));
                    WritePart(stream, Utf8.GetBytes(rulesVersion));
                    WriteLength(stream, sorted.Count);
                    foreach (var part in sorted)
                    {
                        WritePart(stream, Utf8.GetBytes(part.Name));
                        WritePart(stream, part.Bytes);
                    }
                    stream.FlushFinalBlock();
                    var result = new StringBuilder(64);
                    foreach (var value in hash.Hash) result.Append(value.ToString("x2"));
                    return result.ToString();
                }
            }
        }

        private static void WritePart(Stream stream, byte[] bytes)
        {
            WriteLength(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteLength(Stream stream, int length)
        {
            stream.WriteByte((byte)(length >> 24)); stream.WriteByte((byte)(length >> 16));
            stream.WriteByte((byte)(length >> 8)); stream.WriteByte((byte)length);
        }
    }
}
