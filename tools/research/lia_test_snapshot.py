"""Check snapshot drift using independent fixtures, without downloaded originals."""
import hashlib
import json
import tempfile
import unittest
from pathlib import Path

import lia_snapshot


class SnapshotValidationTests(unittest.TestCase):
    def setUp(self):
        local = lia_snapshot.ROOT / ".local/research/lia"
        local.mkdir(parents=True, exist_ok=True)
        self.temp = tempfile.TemporaryDirectory(prefix="snapshot-test-", dir=local)
        self.root = Path(self.temp.name).resolve()
        self.assertIn(local.resolve(), self.root.parents)
        self.addCleanup(self.temp.cleanup)
        self.manifest_path = self.root / "research/lia/review/reviewed-snapshot.json"
        self.repository_files = {
            "docs/lia-review.md": b"Reviewed fixture\n",
            "tools/research/extract.py": b"print('fixture')\n",
            "tools/research/requirements.txt": b"fixture==1\n",
            "research/lia/core/table.csv": b"level,value\n1,8\n",
            "research/lia/dota2/manifest.json": json.dumps({
                "local_archive": ".local/research/lia/dota2/archive.zip",
                "local_source": ".local/research/lia/dota2/source",
            }).encode("utf-8"),
        }
        self.local_files = {
            ".local/research/lia/dota2/archive.zip": b"archive fixture",
            ".local/research/lia/dota2/source/game/scripts/unit.lua": b"return 1\n",
            ".local/research/lia/dota2/source/game/scripts/unit.txt": b'"HP" "8"\n',
            ".local/research/lia/warcraft/Life_in_Arena_v3_4.w3x": b"map 3.4 fixture",
            ".local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x": b"map 3.9c fixture",
            ".local/research/lia/warcraft/mpyq-source/mpyq.py": b"reader fixture",
            ".local/research/lia/warcraft/stormlib/x64/StormLib.dll": b"dll fixture",
            ".local/research/lia/dota2/parser-deps/parser/__init__.py": b"VERSION = 1\n",
            ".local/research/lia/dota2/parser-deps/parser-1.dist-info/METADATA": b"Name: parser\n",
            ".local/research/lia/dota2/parser-deps/parser-1.dist-info/RECORD": b"fixture record\n",
        }
        for relative, content in {**self.repository_files, **self.local_files}.items():
            self.write(relative, content)
        # Do not use records(), scoped_files(), or external_inputs() to build the
        # expected set: their omission of a source file must be detected here.
        expected = {
            section: {name: {"bytes": len(content), "sha256": hashlib.sha256(content).hexdigest()}
                      for name, content in files.items()}
            for section, files in (("files", self.repository_files), ("local_inputs", self.local_files))
        }
        self.write("research/lia/review/reviewed-snapshot.json", json.dumps(expected).encode("utf-8"))
        self.manifest_bytes = self.manifest_path.read_bytes()

    def write(self, relative, content):
        target = self.root / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(content)

    def verify(self, include_local=True):
        result = lia_snapshot.verify(root=self.root, include_local=include_local)
        self.assertEqual(self.manifest_path.read_bytes(), self.manifest_bytes,
                         "Verification must not update the reviewed baseline")
        return result

    def assert_drift(self, relative, change, section):
        target = self.root / relative
        self.assertIn(self.root, target.resolve().parents)
        original = target.read_bytes() if target.is_file() else None
        try:
            if change == "missing":
                target.unlink()
            elif change == "extra":
                self.assertIsNone(original)
                self.write(relative, b"extra fixture\n")
            else:
                self.assertIsNotNone(original)
                # Keep the byte length unchanged so hashing, not only size, is tested.
                self.write(relative, bytes([original[0] ^ 1]) + original[1:])
            result = self.verify()
            self.assertFalse(result["passed"])
            for group in ("files", "local_inputs"):
                for kind in ("missing", "extra", "modified"):
                    wanted = [relative] if group == section and kind == change else []
                    self.assertEqual(result[group][kind], wanted)
        finally:
            if original is None:
                if target.is_file():
                    target.unlink()
            else:
                self.write(relative, original)

    def test_identical_snapshot_passes_without_changing_files(self):
        before = {p.relative_to(self.root).as_posix(): p.read_bytes()
                  for p in self.root.rglob("*") if p.is_file()}
        self.assertTrue(self.verify()["passed"])
        after = {p.relative_to(self.root).as_posix(): p.read_bytes()
                 for p in self.root.rglob("*") if p.is_file()}
        self.assertEqual(after, before)

    def test_missing_snapshot_fails(self):
        self.manifest_path.unlink()
        self.assertFalse(lia_snapshot.verify(root=self.root)["passed"])
        self.assertFalse(self.manifest_path.exists())

    def test_repository_missing_extra_and_modified_files_fail(self):
        for change in ("missing", "extra", "modified"):
            relative = "research/lia/core/extra.csv" if change == "extra" else "research/lia/core/table.csv"
            with self.subTest(change=change):
                self.assert_drift(relative, change, "files")

    def test_raw_source_missing_extra_and_modified_files_fail(self):
        for change in ("missing", "extra", "modified"):
            name = "new.lua" if change == "extra" else "unit.lua"
            with self.subTest(change=change):
                self.assert_drift(".local/research/lia/dota2/source/game/scripts/" + name, change, "local_inputs")

    def test_dependency_missing_extra_and_modified_files_fail(self):
        for change in ("missing", "extra", "modified"):
            name = "extra.py" if change == "extra" else "__init__.py"
            with self.subTest(change=change):
                self.assert_drift(".local/research/lia/dota2/parser-deps/parser/" + name, change, "local_inputs")

    def test_dependency_metadata_missing_or_modified_fails(self):
        for name, change in (("METADATA", "missing"), ("RECORD", "modified")):
            with self.subTest(name=name, change=change):
                self.assert_drift(".local/research/lia/dota2/parser-deps/parser-1.dist-info/" + name, change, "local_inputs")

    def test_original_archive_missing_or_modified_fails(self):
        for change in ("missing", "modified"):
            with self.subTest(change=change):
                self.assert_drift(".local/research/lia/dota2/archive.zip", change, "local_inputs")

    def test_repository_only_explicitly_skips_local_drift(self):
        self.write(".local/research/lia/dota2/source/game/scripts/unit.lua", b"changed\n")
        result = self.verify(include_local=False)
        self.assertTrue(result["passed"])
        self.assertEqual(result["local_inputs"], {"checked": False})


if __name__ == "__main__":
    unittest.main()
