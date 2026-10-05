"""Regression checks for isolated replay, output drift, and honest skipped status."""
import contextlib
import io
import json
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch

import dota_validate


class EmptyReplayValidationTests(unittest.TestCase):
    def test_successful_commands_without_artifacts_do_not_verify_replay(self):
        local = dota_validate.REPO / ".local/research/lia/dota2"
        local.mkdir(parents=True, exist_ok=True)
        with tempfile.TemporaryDirectory(prefix="empty-replay-test-", dir=local) as directory:
            root = Path(directory).resolve()
            self.assertIn(local.resolve(), root.parents)
            expected = root / "expected"
            expected.mkdir()
            (expected / "README.md").write_text("Navigation only\n", encoding="utf-8")
            (expected / "validation.json").write_text("{}\n", encoding="utf-8")
            with patch.object(dota_validate, "REPO", root), \
                 patch.object(dota_validate, "OUT", expected), \
                 patch.object(dota_validate.subprocess, "run", return_value=SimpleNamespace(returncode=0, stderr="", stdout="")):
                result = dota_validate.reproduce()
            self.assertFalse(result["passed"])
            self.assertEqual(result["evidence"]["files_compared"], 0)
            self.assertEqual(result["evidence"]["regenerated_files"], 0)
            self.assertTrue(result["evidence"]["all_subprocesses_succeeded"])


class ReplayValidationTests(unittest.TestCase):
    def setUp(self):
        # Reuse the real source checks; only the slow subprocess boundary is fake.
        original_out = dota_validate.OUT
        self.read_original = lambda name: json.loads((original_out / (name + ".json")).read_text(encoding="utf-8"))
        local = dota_validate.REPO / ".local/research/lia/dota2"
        local.mkdir(parents=True, exist_ok=True)
        self.temp = tempfile.TemporaryDirectory(prefix="validator-test-", dir=local)
        self.addCleanup(self.temp.cleanup)
        self.expected = Path(self.temp.name) / "expected"
        self.expected.mkdir()
        self.fixture = {"one.json": b'{"value": 1}\n', "nested/two.csv": b'a,b\nx,y\n'}
        for name, value in self.fixture.items():
            path = self.expected / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(value)
        (self.expected / "README.md").write_text("Manual navigation\n", encoding="utf-8")
        (self.expected / "validation.json").write_text("{}\n", encoding="utf-8")

    def run_validation(self, change=None, reproduce=True):
        captured = {}
        before = {p.relative_to(self.expected).as_posix(): p.read_bytes() for p in self.expected.rglob("*") if p.is_file()}

        def subprocess_result(command, **kwargs):
            self.assertIn("--out", command, "Replay must pass a separate output directory")
            destination = Path(command[command.index("--out") + 1])
            self.assertNotEqual(destination.resolve(), self.expected.resolve(), "Replay must not overwrite the reference artifacts")
            script = Path(command[3]).name
            if script == "dota_extract.py":
                self.assertEqual(list(destination.iterdir()), [], "Replay must start in an empty directory")
                for name, value in self.fixture.items():
                    if change == "missing" and name == "nested/two.csv":
                        continue
                    path = destination / name
                    path.parent.mkdir(parents=True, exist_ok=True)
                    path.write_bytes(b'{"value": 2}\n' if change == "changed" and name == "one.json" else value)
            if script == "dota_catalog.py" and change == "extra":
                (destination / "new.json").write_text("{}\n", encoding="utf-8")
            failed = script == "dota_catalog.py" and change == "failed"
            return SimpleNamespace(returncode=23 if failed else 0, stderr="controlled replay failure" if failed else "", stdout="")

        argv = ["dota_validate.py"] + (["--reproduce"] if reproduce else [])
        with patch.object(dota_validate, "OUT", self.expected), \
             patch.object(dota_validate, "read", side_effect=self.read_original), \
             patch.object(dota_validate.sys, "argv", argv), \
             patch.object(dota_validate.subprocess, "run", side_effect=subprocess_result), \
             patch.object(dota_validate, "json_write", side_effect=lambda path, result: captured.update(result)), \
             contextlib.redirect_stdout(io.StringIO()):
            try:
                dota_validate.main()
                exit_code = 0
            except SystemExit as error:
                exit_code = error.code
        after = {p.relative_to(self.expected).as_posix(): p.read_bytes() for p in self.expected.rglob("*") if p.is_file()}
        self.assertEqual(after, before, "Validation must preserve every reference artifact byte")
        return captured, exit_code

    def test_identical_clean_replay_passes_and_preserves_originals(self):
        result, code = self.run_validation()
        self.assertEqual(code, 0)
        self.assertTrue(result["reproducibility_verified"])
        self.assertEqual(result["reproducibility_status"], "passed")

    def test_failed_subprocess_stays_failed_even_with_matching_outputs(self):
        result, code = self.run_validation("failed")
        self.assertEqual(code, 1)
        self.assertFalse(result["extraction_checks_passed"])
        self.assertFalse(result["reproducibility_verified"])
        replay = next(c for c in result["checks"] if c["name"] == "Reproducibility")
        self.assertFalse(replay["passed"])
        self.assertFalse(replay["evidence"]["all_subprocesses_succeeded"])

    def test_new_missing_and_changed_artifacts_fail(self):
        for change, key, wanted in [("extra", "extra", ["new.json"]), ("missing", "missing", ["nested/two.csv"]), ("changed", "changed", ["one.json"])]:
            with self.subTest(change=change):
                result, code = self.run_validation(change)
                self.assertEqual(code, 1)
                self.assertEqual(result["reproducibility_status"], "failed")
                replay = next(c for c in result["checks"] if c["name"] == "Reproducibility")
                self.assertEqual(replay["evidence"][key], wanted)

    def test_skipped_replay_is_not_verified(self):
        result, code = self.run_validation(reproduce=False)
        self.assertEqual(code, 0)
        self.assertTrue(result["extraction_checks_passed"])
        self.assertFalse(result["reproducibility_verified"])
        self.assertEqual(result["reproducibility_status"], "not_run")


if __name__ == "__main__":
    unittest.main()
