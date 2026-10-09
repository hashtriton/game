"""Verify immutable inputs, write scope, artifact budgets and local links."""
import ast
import hashlib
import json
import re
import subprocess
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art/heroes/breakwater/p4'
LOCAL=ROOT/'.local/codex-tasks/hero-b'


def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()


def main():
    baseline=json.loads((LOCAL/'baseline-3.json').read_text())
    changed={group:[n for n,h in baseline[group].items() if not (ROOT/n).is_file() or sha(ROOT/n)!=h]
             for group in ['protected','tracked_unity_docs']}
    before=set(baseline['status'].splitlines())
    status=subprocess.check_output(['git','status','--porcelain=v1','-uall'],cwd=ROOT).decode()
    added=set(status.splitlines())-before
    outside=[line for line in added if not (line[3:].startswith('art/heroes/breakwater/p4/') or
              line[3:].startswith('tools/hero_breakwater/p4_') or line[3:].startswith('.local/codex-tasks/hero-b/'))]
    files=[p for p in OUT.rglob('*') if p.is_file()]
    pngs=[p for p in files if p.suffix=='.png']
    invalid_pngs=[]
    for p in pngs:
        with Image.open(p) as im:
            im.verify()
        if p.stat().st_size>=8*1024*1024:
            invalid_pngs.append(p.relative_to(ROOT).as_posix())
    textfiles=list((ROOT/'tools/hero_breakwater').glob('p4_*.py'))+[p for p in OUT.rglob('*') if p.suffix in ['.md','.json']]
    textfiles += [LOCAL/'progress-3.md']
    textfiles += [p for p in [LOCAL/'REPORT-3-draft.md',LOCAL/'technical-review-3.md',
                            LOCAL/'review-3-before/verdict.md',LOCAL/'review-3-after/verdict.md'] if p.exists()]
    forbidden=[]
    broken=[]
    for p in textfiles:
        text=p.read_text(encoding='utf-8')
        if any(c in text for c in ['\u2014','\u2013','\u2012']):
            forbidden.append(p.relative_to(ROOT).as_posix())
        if re.search(r'Over'+'watch|Bli'+'zzard',text,re.I):
            forbidden.append(p.relative_to(ROOT).as_posix())
        if p.suffix=='.py':
            ast.parse(text,filename=str(p))
        if p.suffix=='.md':
            for target in re.findall(r'\]\(([^)]+)\)',text):
                if not target.startswith(('https:','http:','#')) and not (p.parent/target.split('#')[0]).exists():
                    broken.append({'source':str(p.relative_to(ROOT)),'target':target})
    unwanted=[str(p.relative_to(ROOT)) for folder in [OUT,ROOT/'tools/hero_breakwater',LOCAL/'cycles-3']
              for p in folder.rglob('*') if p.name=='__pycache__' or p.suffix=='.blend1']
    size=sum(p.stat().st_size for p in files)
    checks={'p1_p3_hashes_unchanged':not changed['protected'],'tracked_unity_docs_hashes_unchanged':not changed['tracked_unity_docs'],
            'scope_delta_allowed':not outside,'pngs_under_8_mib':not invalid_pngs,'p4_under_100_mib':size<100*1024*1024,
            'no_forbidden_dashes_or_names':not forbidden,'no_broken_local_links':not broken,'no_caches_backups':not unwanted,
            'branch_unchanged':subprocess.check_output(['git','branch','--show-current'],cwd=ROOT).decode().strip()==baseline['branch'],
            'head_unchanged':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT).decode().strip()==baseline['head']}
    checks['existing_tracked_unity_hashes_unchanged']=not any(n.startswith('unity/') for n in changed['tracked_unity_docs'])
    owned=files+list((ROOT/'tools/hero_breakwater').glob('p4_*.py'))
    own_manifest={p.relative_to(ROOT).as_posix():sha(p) for p in owned}
    artifact_checks={k:v for k,v in checks.items() if k not in ['tracked_unity_docs_hashes_unchanged','scope_delta_allowed']}
    report={'checks':checks,'passed':all(checks.values()),'protected_file_count':len(baseline['protected']),
            'tracked_unity_docs_count':len(baseline['tracked_unity_docs']),'changed':changed,'outside_scope_status':outside,
            'git_status':status,'p4_bytes':size,'p4_files':len(files),'png_count':len(pngs),'invalid_pngs':invalid_pngs,
            'forbidden_files':forbidden,'broken_links':broken,'unwanted':unwanted,
            'owned_artifact_checks':artifact_checks,'owned_artifact_checks_pass':all(artifact_checks.values()),
            'owned_files_sha256':own_manifest,
            'concurrency_note':'Raw checkout comparison is not green when another authorized session writes rig/probe files or docs. Outside-scope observations are retained, not erased or attributed automatically. This task wrote only p4 scripts, p4 art and local hero-b evidence.'}
    (LOCAL/'hygiene-3.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:report[k] for k in ['passed','checks','changed','outside_scope_status','p4_bytes','png_count','unwanted']},indent=2))
    if not report['passed']:
        raise SystemExit(1)


if __name__=='__main__':
    main()
