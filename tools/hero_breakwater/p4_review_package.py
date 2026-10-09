"""Freeze the four visual-only review inputs with source hashes."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art/heroes/breakwater/p4'
LOCAL=ROOT/'.local/codex-tasks/hero-b'


def main():
    p=argparse.ArgumentParser()
    p.add_argument('--stage',choices=['before','after'],required=True)
    a=p.parse_args()
    folder=LOCAL/f'review-3-{a.stage}'
    folder.mkdir(parents=True,exist_ok=True)
    files=[ROOT/'art/concepts/2026-10-09-hero-b-breakwater-v1.png',OUT/'sheet-p4.png',
           OUT/'sheet-p4-vs-p3.png',OUT/'sheet-p4-gameview.png']
    rows=[]
    for f in files:
        shutil.copyfile(f,folder/f.name)
        rows.append({'source':str(f.relative_to(ROOT)),'copy':f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()})
    (folder/'inputs.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
    print(folder.relative_to(ROOT))


if __name__=='__main__':
    main()
