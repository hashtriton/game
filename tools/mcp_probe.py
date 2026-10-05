"""Small STDIO MCP verification client. Does not load Codex credentials/config."""
import argparse
import collections
import json
import os
import pathlib
import queue
import re
import subprocess
import threading
import time

parser = argparse.ArgumentParser()
parser.add_argument("--launch-file", required=True)
parser.add_argument("--output", required=True)
parser.add_argument("--call-tool")
parser.add_argument("--arguments-file")
parser.add_argument("--timeout", type=float, default=150)
options = parser.parse_args()
launch = json.loads(pathlib.Path(options.launch_file).read_text(encoding="utf-8-sig"))
process = subprocess.Popen(
    [launch["command"], *launch.get("args", [])],
    cwd=launch.get("cwd"), env={**os.environ, **launch.get("env", {})}, stdin=subprocess.PIPE, stdout=subprocess.PIPE,
    stderr=subprocess.PIPE, text=True, encoding="utf-8", errors="replace", bufsize=1,
)
messages = queue.Queue()
stderr_tail = collections.deque(maxlen=40)
report = {"transport": "stdio", "initialize_passed": False, "tools_list_passed": False,
          "native_codex_tool_catalog_verified": False}
phase = "initialize"

def persist():
    pathlib.Path(options.output).write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

def read_stderr():
    for line in process.stderr:
        if re.search(r"token|secret|authorization|credential|cookie", line, re.IGNORECASE):
            line = "[sensitive diagnostic line omitted]\n"
        stderr_tail.append(line.rstrip())

def read_stdout():
    for line in process.stdout:
        try:
            messages.put(json.loads(line))
        except ValueError:
            pass
    messages.put({"process_closed": True})

threading.Thread(target=read_stdout, daemon=True).start()
threading.Thread(target=read_stderr, daemon=True).start()
sequence = 0

def send(value):
    process.stdin.write(json.dumps(value) + "\n")
    process.stdin.flush()

def request(method, params):
    global sequence
    sequence += 1
    identity = sequence
    send({"jsonrpc": "2.0", "id": identity, "method": method, "params": params})
    deadline = time.monotonic() + options.timeout
    while time.monotonic() < deadline:
        try:
            message = messages.get(timeout=max(0.1, deadline - time.monotonic()))
        except queue.Empty as error:
            raise TimeoutError(f"{method} exceeded {options.timeout}s") from error
        if message.get("process_closed"):
            raise RuntimeError(f"MCP process closed during {method}; exit={process.poll()}")
        if message.get("id") == identity:
            if "error" in message:
                raise RuntimeError(json.dumps(message["error"]))
            return message["result"]
    raise TimeoutError(method)

try:
    initialized = request("initialize", {
        "protocolVersion": "2024-11-05", "capabilities": {},
        "clientInfo": {"name": "Codex-setup-verification", "version": "1.0"},
    })
    send({"jsonrpc": "2.0", "method": "notifications/initialized"})
    report["initialize_passed"] = True
    phase = "tools/list"
    tools = request("tools/list", {})["tools"]
    report = {
        "transport": "stdio", "client": "shell-hosted Python JSON-RPC verification client invoked by Codex",
        "native_codex_tool_catalog_verified": False,
        "protocol_version": initialized.get("protocolVersion"),
        "server_info": initialized.get("serverInfo"),
        "tools": [{"name": t["name"], "annotations": t.get("annotations", {}), "inputSchema": t.get("inputSchema", {})} for t in tools],
        "initialize_passed": True, "tools_list_passed": True,
    }
    persist()
    if options.call_tool:
        if options.call_tool not in [t["name"] for t in tools]:
            raise ValueError("Requested tool not advertised by MCP server")
        arguments = json.loads(pathlib.Path(options.arguments_file).read_text(encoding="utf-8-sig")) if options.arguments_file else {}
        report["called_tool"] = options.call_tool
        report["tool_call_passed"] = False
        phase = "tools/call"
        persist()
        result = request("tools/call", {"name": options.call_tool, "arguments": arguments})
        report["tool_result"] = result
        report["tool_call_passed"] = not result.get("isError", False)
    persist()
    print(json.dumps({k: v for k, v in report.items() if k not in ("tools", "tool_result")}))
    print("Tool count: " + str(len(tools)))
except Exception as error:
    report.update({"failure_phase": phase, "error_type": type(error).__name__,
                   "error": str(error), "stderr_tail": list(stderr_tail)})
    persist()
    raise
finally:
    process.terminate()
    try:
        process.wait(timeout=10)
    except subprocess.TimeoutExpired:
        process.kill()
