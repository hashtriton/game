"""Compare acquired snapshots by rawcode, preserving version boundaries."""
from __future__ import annotations
import argparse,csv,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];BASE=ROOT/'research/lia/warcraft'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def save(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
VALUE_TYPES={0:'integer',1:'real',2:'unreal',3:'string'}
NUMERIC_TYPES={'integer','real','unreal'}

def binary_values(obj):
    # Object identity is the outer catalog key. Byte offsets and extraction
    # provenance do not affect the stored field value.
    return {(r['field'],r['level'],r['pointer']):
            (VALUE_TYPES[r['type']],r['value'])
            for r in obj.get('binary_overrides',[])}

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument('--base',type=Path,default=BASE,
                    help='Catalog root containing 3.4 and 3.9c; outputs are written here')
    base=ap.parse_args().base
    changes=[];inventory={}
    for kind in ['units','items','abilities','upgrades','buffs']:
        a=read(base/'3.4'/f'{kind}.json');b=read(base/'3.9c'/f'{kind}.json')
        inventory[kind]={'added':sorted(b.keys()-a.keys()),'removed':sorted(a.keys()-b.keys()),'shared':len(a.keys()&b.keys())}
        for oid in sorted(a.keys()&b.keys()):
            identity={'kind':kind,'rawcode':oid,'name_3.4':a[oid].get('display_name'),'name_3.9c':b[oid].get('display_name')}
            for field in sorted(a[oid].keys()|b[oid].keys()):
                if field in ['sources','jass_functions','profile_conflict_fields','binary_overrides']:continue
                av=a[oid].get(field);bv=b[oid].get(field)
                if av!=bv:changes.append(identity|{'source_kind':'catalog_field','field':field,'value_3.4':av,'value_3.9c':bv})
            av=binary_values(a[oid]);bv=binary_values(b[oid])
            for key in sorted(av.keys()|bv.keys(),key=lambda k:(k[0],-1 if k[1] is None else k[1],-1 if k[2] is None else k[2])):
                old=av.get(key);new=bv.get(key)
                if old==new:continue
                field,level,pointer=key
                changes.append(identity|{'source_kind':'binary_override','field':field,'level':level,'pointer':pointer,
                                         'value_type_3.4':old[0] if old else None,'value_type_3.9c':new[0] if new else None,
                                         'value_3.4':old[1] if old else None,'value_3.9c':new[1] if new else None})
    save(base/'version-field-diff.json',changes)
    numeric=[x for x in changes if re.match(r'^(HP|mana|STR|AGI|INT|Primary|spd|def|realdef|regen|gold|lumber|bounty|dmg|dice|sides|cool|range|Data|Cost|Cool|Dur|HeroDur|Cast|Area|Rng|levels|reqLevel|maxlevel|base\d|mod\d|effect\d|abilList|heroAbilList)',x['field'])]
    numeric=[r for r in numeric if r['source_kind']=='catalog_field']+[r for r in changes if r['source_kind']=='binary_override' and (r['value_type_3.4'] in NUMERIC_TYPES or r['value_type_3.9c'] in NUMERIC_TYPES)]
    with (base/'version-balance-diff.csv').open('w',encoding='utf-8-sig',newline='') as f:
        w=csv.DictWriter(f,fieldnames=['kind','rawcode','name_3.4','name_3.9c','source_kind','field','level','pointer','value_type_3.4','value_type_3.9c','value_3.4','value_3.9c']);w.writeheader();w.writerows(numeric)
    a={h['id'] for h in read(base/'3.4/selectable-heroes.json')};b={h['id'] for h in read(base/'3.9c/selectable-heroes.json')}
    misc_a=read(base/'3.4/profiles.json')['war3mapMisc.txt']['Misc'];misc_b=read(base/'3.9c/profiles.json')['war3mapMisc.txt']['Misc']
    summary={'warning':'Equal rawcode means matching technical key only; it does not establish the same design, behavior, compatibility or rights.',
             'comparison_scope':'Explicit catalog fields and typed binary overrides on shared rawcodes. Binary keys are rawcode/field/level/pointer; offsets and provenance are excluded. Missing explicit values do not establish removal of an inherited runtime value. SLK fields and binary overrides remain separate representations.',
             'balance_candidate_scope':'Catalog field-name heuristic plus all numeric binary overrides (integer/real/unreal). String overrides remain in the full JSON; neither list proves a runtime balance change.',
             'catalog_changes':inventory,'selectable_heroes_added':sorted(b-a),'selectable_heroes_removed':sorted(a-b),
             'all_changed_fields':len(changes),'catalog_field_changes':sum(r['source_kind']=='catalog_field' for r in changes),
             'binary_override_changes':sum(r['source_kind']=='binary_override' for r in changes),
             'balance_field_candidates':len(numeric),'numeric_binary_override_changes':sum(r['source_kind']=='binary_override' for r in numeric),
             'misc_changes':{k:{'3.4':misc_a.get(k),'3.9c':misc_b.get(k)} for k in sorted(misc_a.keys()|misc_b.keys()) if not k.startswith('_') and misc_a.get(k)!=misc_b.get(k)}}
    save(base/'version-diff-summary.json',summary)
    print(json.dumps({k:v for k,v in summary.items() if k!='catalog_changes'},ensure_ascii=False,indent=2))
if __name__=='__main__':main()
