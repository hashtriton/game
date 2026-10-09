"""Capture immutable input hashes and the read-only checkout baseline."""
import hashlib
import json
import subprocess
from datetime import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
LOCAL = ROOT / '.local/codex-tasks/hero-b'


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    protected = {}
    for folder in ['art/heroes/breakwater', 'tools/hero_breakwater']:
        for p in (ROOT / folder).rglob('*'):
            if p.is_file() and '/p4/' not in p.as_posix() and not p.name.startswith('p4_'):
                protected[p.relative_to(ROOT).as_posix()] = digest(p)
    tracked = subprocess.check_output(['git', 'ls-files', '-z', 'unity', 'docs'], cwd=ROOT).decode().split('\0')
    source = {n: digest(ROOT / n) for n in tracked if n and (ROOT / n).is_file()}
    result = {'time': datetime.now().astimezone().isoformat(),
              'head': subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT).decode().strip(),
              'branch': subprocess.check_output(['git', 'branch', '--show-current'], cwd=ROOT).decode().strip(),
              'status': subprocess.check_output(['git', 'status', '--porcelain=v1', '-uall'], cwd=ROOT).decode(),
              'protected': protected, 'tracked_unity_docs': source}
    LOCAL.mkdir(parents=True, exist_ok=True)
    (LOCAL / 'baseline-3.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print('Captured', len(protected), 'hero input hashes and', len(source), 'tracked Unity/docs hashes')


if __name__ == '__main__':
    main()
