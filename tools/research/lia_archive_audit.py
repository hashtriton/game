"""Read unnamed MPQ entries by StormLib's documented pseudo-filename fallback."""
from __future__ import annotations

import collections
import hashlib
import json
import struct
from pathlib import Path

from warcraft_extract import StormReader

ROOT = Path(__file__).resolve().parents[2]


def classify(data):
    if data.endswith(b"TRUEVISION-XFILE.\x00"):
        return "tga_image"
    if data[:4] in (b"\x00\x01\x00\x00", b"OTTO") and len(data) >= 12:
        count = struct.unpack_from(">H", data, 4)[0]
        if 0 < count < 256 and 12 + count * 16 <= len(data):
            entries = [struct.unpack_from(">4sIII", data, 12 + i * 16) for i in range(count)]
            if all(offset + length <= len(data) for _, _, offset, length in entries):
                return "sfnt_font"
    for magic, kind in [(b"MDLX", "warcraft_model"), (b"BLP1", "texture_blp"), (b"BLP2", "texture_blp"),
                        (b"RIFF", "riff_audio_or_media"), (b"ID3", "mp3_audio"), (b"OggS", "ogg_audio"),
                        (b"\x89PNG", "png_image"), (b"DDS ", "dds_texture"), (b"\xff\xd8\xff", "jpeg_image")]:
        if data.startswith(magic):
            return kind
    if len(data) >= 2 and data[0] == 255 and data[1] & 0xE0 == 0xE0:
        return "possible_mpeg_audio"
    if not data:
        return "empty"
    sample = data[:65536]
    if b"\x00" not in sample:
        for encoding in ["utf-8-sig", "cp1251"]:
            try:
                text = sample.decode(encoding)
            except UnicodeError:
                continue
            ratio = sum(c.isprintable() or c in "\n\r\t" for c in text) / max(1, len(text))
            if ratio > 0.98:
                return "text_requires_review"
    return "unidentified_binary"


def main():
    output = {"date": "2026-10-05", "method": "StormLib 9.40 SFileOpenFileEx File########.xxx pseudo-name",
              "official_source": "https://github.com/ladislav-zezula/StormLib/blob/v9.40/src/SFileOpenFileEx.cpp#L314",
              "executes_game_code": False, "versions": {}}
    for version, name in [("3.4", "Life_in_Arena_v3_4.w3x"), ("3.9c", "Life_in_Arena_v3_9_c.w3x")]:
        root = ROOT / ".local/research/lia/warcraft"
        manifest = json.loads((ROOT / "research/lia/warcraft" / version / "extraction-manifest.json").read_text())
        reader = StormReader(root / name)
        records = []
        try:
            for row in manifest["unresolved_hash_entries"]:
                block = row["block_table_index"]
                pseudo = f"File{block:08d}.xxx"
                result = {"block_index": block, "pseudo_name": pseudo, "original_name_known": False}
                try:
                    data = reader.read_file(pseudo)
                    if data is None:
                        raise ValueError("pseudo-name did not open")
                    result.update(bytes=len(data), sha256=hashlib.sha256(data).hexdigest(), kind=classify(data), readable=True)
                    if result["kind"] == "text_requires_review":
                        target = root / version / "unnamed-text" / (pseudo + ".txt")
                        target.parent.mkdir(parents=True, exist_ok=True)
                        target.write_bytes(data)
                        result["local_path"] = str(target.relative_to(ROOT)).replace("\\", "/")
                except Exception as exc:
                    result.update(readable=False, kind="unreadable", error=str(exc))
                records.append(result)
        finally:
            reader.close()
        output["versions"][version] = {"map_sha256": manifest["sha256"], "entries": len(records),
                                      "all_readable": all(r["readable"] for r in records),
                                      "classification": dict(collections.Counter(r["kind"] for r in records)), "records": records}
    output["passed"] = all(v["all_readable"] for v in output["versions"].values())
    output["limits"] = ["Magic-byte classification is not proof of gameplay irrelevance for all unknown entries",
                        "Original file names remain unresolved; originals retain every archive block",
                        "Unreadable encrypted entries require original names or further format analysis, not guesses"]
    dest = ROOT / "research/lia/unnamed-archive-audit.json"
    dest.write_text(json.dumps(output, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({version: {k: v for k, v in values.items() if k != "records"} for version, values in output["versions"].items()}))
    raise SystemExit(0 if output["passed"] else 1)


if __name__ == "__main__":
    main()
