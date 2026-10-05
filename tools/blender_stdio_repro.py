"""Diagnose Blender background stdin behavior without running Blender Python."""
import json
import os
import pathlib
import subprocess
import time

project = pathlib.Path(__file__).resolve().parent.parent
launch = json.loads((project / "tools/mcp-blender-launch.json").read_text(encoding="utf-8"))
command = [launch["env"]["BLENDER_PATH"], "--background", str(project / "art/smoke/blender-smoke.blend")]
report = {"no_blender_python_code_executed": True, "variants": []}
for label, stdin in (("inherited_open_pipe", subprocess.PIPE), ("closed_stdin_DEVNULL", subprocess.DEVNULL)):
    started = time.monotonic()
    process = subprocess.Popen(command, env={**os.environ, **launch["env"]}, stdin=stdin,
                               stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    timed_out = False
    try:
        process.wait(timeout=12)
    except subprocess.TimeoutExpired:
        timed_out = True
        process.terminate()
        process.wait(timeout=10)
    if process.stdin:
        process.stdin.close()
    stdout = process.stdout.read().decode("utf-8", errors="replace")
    stderr = process.stderr.read().decode("utf-8", errors="replace")
    report["variants"].append({"stdin": label, "timed_out": timed_out,
                               "exit_code": process.returncode,
                               "elapsed_sec": round(time.monotonic() - started, 2),
                               "stdout": stdout, "stderr": stderr})
target = project / "verification/blender-stdin-repro.json"
target.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(report, ensure_ascii=False))
