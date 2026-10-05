"""Deterministic relational catalog from warcraft_extract.py outputs, static only."""
from __future__ import annotations
import argparse, collections, csv, json, re
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def js(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def csvfile(p,rows,fields=None):
    if fields is None:fields=list(dict.fromkeys(k for r in rows for k in r))
    with p.open('w',encoding='utf-8-sig',newline='') as f:
        w=csv.DictWriter(f,fieldnames=fields);w.writeheader()
        for r in rows:w.writerow({k:json.dumps(v,ensure_ascii=False) if isinstance(v,(list,dict)) else v for k,v in r.items()})
def clean(s):return re.sub(r'\|c[0-9A-Fa-f]{8}|\|[rR]', '',str(s).strip('"')).replace('|n',' / ')
def atoms(args):return [a.strip().strip("'") for a in args.split(',')]

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--version',choices=['3.4','3.9c'],required=True)
    ap.add_argument('--out',type=Path);ap.add_argument('--raw',type=Path);args=ap.parse_args()
    out=args.out or ROOT/'research/lia/warcraft'/args.version;raw=args.raw or ROOT/'.local/research/lia/warcraft'/args.version/'extracted'
    slks=read(out/'slk.json');prof=read(out/'profiles.json');objects=read(out/'objects.json');funcs=read(out/'jass-functions.json')
    lines=(raw/'war3map.rawcodes.j').read_text(encoding='utf-8').splitlines();text='\n'.join(lines)
    units={};items={};abilities={};upgrades={};buffs={};effects={};allfields=[]
    tables={'UnitAbilities.slk':(units,'unitAbilID'),'UnitBalance.slk':(units,'unitBalanceID'),'UnitData.slk':(units,'unitID'),'UnitUI.slk':(units,'unitUIID'),'UnitWeapons.slk':(units,'unitWeapID'),'ItemData.slk':(items,'itemID'),'AbilityData.slk':(abilities,'alias'),'UpgradeData.slk':(upgrades,'upgradeid'),'AbilityBuffData.slk':(buffs,'alias')}
    for path,rows in slks.items():
        table,key=tables.get(path.split('\\')[-1],(effects,'Name'))
        for r in rows:
            oid=r[key];ob=table.setdefault(oid,{'id':oid,'sources':[]})
            ob['sources'].append(f'{path}:{r["_line"]}')
            for k,v in r.items():
                if k.startswith('_') or k==key:continue
                ob[k]=v
                allfields.append({'object_id':oid,'source_kind':'SLK','source':path,'line':r['_line'],'slk_row':r['_slk_row'],'field':k,'value':v})
    duplicates=[];profile_conflicts=[];seen_profiles={}
    for path,sections in prof.items():
        for oid,fields in sections.items():
            for row in fields['_assignments']:
                allfields.append({'object_id':oid,'source_kind':'profile','source':path,**row})
            duplicates.extend({'source':path,'object_id':oid,**row} for row in fields.get('_duplicates',[]))
            for row in fields['_assignments']:
                key=(oid,row['field']);prior=seen_profiles.get(key)
                if prior and prior['source']!=path and prior['value']!=row['value']:
                    profile_conflicts.append({'object_id':oid,'field':row['field'],
                        'earlier':prior,'later':{'source':path,**row},
                        'status':'unresolved-engine-precedence; catalog shows last visited profile'})
                seen_profiles[key]={'source':path,**row}
            for table in (units,items,abilities,upgrades,buffs):
                if oid in table:
                    table[oid].update({k:v.strip('"') for k,v in fields.items() if not k.startswith('_')})
                    table[oid]['sources'].append(f'{path}:{fields["_line"]}')
    for path,data in objects.items():
        for row in data['fields']:
            allfields.append({'object_id':row['object_id'],'source_kind':'binary_override','source':path,'offset':row['offset'],'field':row['field'],'level':row['level'],'pointer':row['pointer'],'value':row['value']})
            # Keep binary raw fields distinct; never invent an implicit field mapping.
            if path.endswith('.w3h'):
                buffs.setdefault(row['object_id'],{'id':row['object_id'],'sources':[path+':offset '+str(row['object_offset'])],'base_id':row['old_id']})
            for table in (units,items,abilities,upgrades,buffs):
                if row['object_id'] in table:
                    table[row['object_id']].setdefault('binary_overrides',[]).append(row)
    js(out/'profile-duplicates.json',duplicates)
    js(out/'profile-conflicts.json',profile_conflicts)
    csvfile(out/'all-object-fields.csv',allfields)
    for label,table in [('units',units),('items',items),('abilities',abilities),('upgrades',upgrades),('buffs',buffs)]:
        for oid,ob in table.items():
            conflicts={r['field'] for r in profile_conflicts if r['object_id']==oid}
            conflicts.update(r['field'] for r in duplicates if r['object_id']==oid and r['previous_value']!=r['value'])
            if conflicts:ob['profile_conflict_fields']=sorted(conflicts)
            ob['display_name']=clean(ob.get('Name',ob.get('name',oid)))
            ob['jass_functions']=[f['name'] for f in funcs if oid in f['rawcodes']]
        js(out/(label+'.json'),table)
        csvfile(out/(label+'.csv'),list(table.values()))
    current=None;stock=[];tavern=None;stock_function=None
    for n,line in enumerate(lines,1):
        if line.startswith('function '):current=line.split()[1]
        m=re.match(r"set \w+\[(\d+)\]=CreateUnit(?:AtLoc)?\(p,'([^']{4})'",line)
        if m:tavern=m[2]
        m=re.match(r"call AddUnitToStock\(u,'([^']{4})',0,1\)",line)
        if m:stock.append({'id':m[1],'tavern':tavern,'source_function':current,'source_line':n})
    # Initial stock setup is the function with the largest number of explicit heroes.
    fn=collections.Counter(x['source_function'] for x in stock).most_common(1)[0][0]
    heroes=[x|{k:units[x['id']].get(k) for k in ['display_name','HP','realHP','manaN','realM','STR','AGI','INT','STRplus','AGIplus','INTplus','Primary','spd','def','dmgplus1','dice1','sides1','cool1','rangeN1','heroAbilList','abilList']} for x in stock if x['source_function']==fn]
    csvfile(out/'selectable-heroes.csv',heroes);js(out/'selectable-heroes.json',heroes)
    links=[]
    for h in heroes:
        for role in ('heroAbilList','abilList'):
            for ability in str(h.get(role) or '').split(','):
                if ability in ('','_'):continue
                a=abilities.get(ability,{})
                links.append({'hero_id':h['id'],'hero_name':h['display_name'],'role':role,'ability_id':ability,'ability_name':a.get('display_name'),'base_code':a.get('code'),'levels':a.get('levels'),'source_line':h['source_line'],'jass_functions':a.get('jass_functions',[]),'ability_sources':a.get('sources',[]),'binary_overrides':a.get('binary_overrides',[])})
    csvfile(out/'hero-abilities.csv',links)
    itemlinks=[]
    for oid,ob in items.items():
        for aid in str(ob.get('abilList','')).split(','):
            if aid and aid!='_':itemlinks.append({'item_id':oid,'item_name':ob['display_name'],'ability_id':aid,'ability_name':abilities.get(aid,{}).get('display_name'),'ability_found':aid in abilities,'jass_functions':abilities.get(aid,{}).get('jass_functions',[])})
    csvfile(out/'item-abilities.csv',itemlinks)
    recipes=[];conversions=[];registrations=[]
    for n,line in enumerate(lines,1):
        if args.version=='3.4':
            m=re.match(r'call cM\(Nx,(.*)\)',line)
            c=re.match(r'call gM\(bx,(.*)\)',line)
        else:
            m=re.match(r'call zm\((.*)\)',line)
            c=re.match(r'call oM\((.*)\)',line)
        if m:
            vals=atoms(m[1]);parts=[x for x in vals[1:] if x!='0'];counts=dict(collections.Counter(parts))
            recipes.append({'result_id':vals[0],'result_name':items.get(vals[0],{}).get('display_name'),'ingredients':counts,'ingredient_names':{i:items.get(i,{}).get('display_name') for i in counts},'source_line':n,'source_function':'cM' if args.version=='3.4' else 'zm'})
        if c:
            vals=atoms(c[1]);conversions.append({'inventory_id':vals[0],'inventory_name':items.get(vals[0],{}).get('display_name'),'shop_id':vals[1],'shop_name':items.get(vals[1],{}).get('display_name'),'argument3':vals[2],'source_line':n,'argument3_semantics':'purchasable boolean' if args.version=='3.4' else 'mode/charge code; see oM body'})
        m=re.match(r'call xH\((.*)\)',line) if args.version=='3.4' else None
        if m:
            vals=atoms(m[1]);registrations.append({'item_id':vals[0],'registered_price':int(vals[1]),'extra_rawcodes':vals[2:],'source_line':n})
    js(out/'recipes.json',recipes);csvfile(out/'recipes.csv',recipes);csvfile(out/'shop-conversions.csv',conversions)
    if registrations:csvfile(out/'item-price-registry.csv',registrations)
    edges=[]
    for r in recipes:
        for ing,num in r['ingredients'].items():edges.append({'ingredient_id':ing,'result_id':r['result_id'],'quantity':num,'source_line':r['source_line']})
    csvfile(out/'recipe-edges.csv',edges)
    wave_assign=[];array_assign=[];current=None
    for n,line in enumerate(lines,1):
        if line.startswith('function '):current=line.split()[1]
        m=re.match(r'set (\w+)\[(\d+)\]=(.*)',line)
        if m:
            val=m[3];array_assign.append({'array':m[1],'index':int(m[2]),'value':val,'function':current,'line':n})
            if re.fullmatch(r"'[^']{4}'",val) and val.strip("'") in units:
                oid=val.strip("'")
                wave_assign.append({'array':m[1],'index':int(m[2]),'unit_id':oid,'unit_name':units[oid]['display_name'],'function':current,'line':n})
    csvfile(out/'jass-array-assignments.csv',array_assign);csvfile(out/'unit-roster-assignments.csv',wave_assign)
    wave_text=[]
    for m in re.finditer(r'set (\w+)\[(\d+)\]="((?:\\.|[^"\\])*)"',text):
        if m[1] in ('Cv','VA','ff','Sv') or ('Заклинания' in m[3] and int(m[2])<=30):
            wave_text.append({'array':m[1],'index':int(m[2]),'text':m[3],'source_line':text.count('\n',0,m.start())+1,'evidence_type':'tooltip_not_runtime'})
    js(out/'wave-texts.json',wave_text)
    strings=[{'line':text.count('\n',0,m.start())+1,'text':m[1]} for m in re.finditer(r'"((?:\\.|[^"\\])*)"',text) if re.search('[А-Яа-я]',m[1])]
    js(out/'jass-russian-texts.json',strings)
    candidates=[]
    for f in funcs:
        body=lines[f['start_line']-1:f['end_line']]
        candidates.append({'name':f['name'],'start_line':f['start_line'],'end_line':f['end_line'],'rawcodes':f['rawcodes'],'calls':f['calls'],'callbacks':f.get('callbacks',[]),'numeric_lines':sum(bool(re.search(r'\d',x)) for x in body)})
    csvfile(out/'function-index.csv',candidates)
    if args.version=='3.4':
        waves=[]
        # gs contains the ordinary 20-stage configuration. Alternative functions remain separate.
        primary=[x for x in wave_assign if 13480<=x['line']<=13523]
        for stage in range(1,21):
            for role,array in [('normal','jx'),('enhanced','Jx')]:
                found=[r for r in primary if r['array']==array and r['index']==stage]
                if stage==20:
                    if role=='enhanced':continue
                    r={'unit_id':'O006','unit_name':units['O006']['display_name'],'line':13465}
                elif found:
                    if stage%5==0 and role=='enhanced':continue
                    r=found[0]
                else:continue
                u=units[r['unit_id']]
                waves.append({'stage':stage,'role':'megaboss' if stage%5==0 else role,'unit_id':r['unit_id'],'unit_name':r['unit_name'],'HP_field':u.get('HP'),'armor_field':u.get('def'),'damage_base':u.get('dmgplus1'),'dice':u.get('dice1'),'sides':u.get('sides1'),'attack_interval':u.get('cool1'),'speed':u.get('spd'),'bounty_base':u.get('bountyplus'),'bounty_dice':u.get('bountydice'),'bounty_sides':u.get('bountysides'),'ability_ids':u.get('abilList'),'source_line':r['line']})
        csvfile(out/'waves-default.csv',waves)
        wavemd=['# Волны Warcraft 3.4, стандартный survival roster','','Число обычных врагов 2*CQ(Px), затем 2 усиленных. На этапах 5/10/15/20 - по одному основному мегабоссу до дополнительных призывов. Поля HP/armor/damage не включают runtime-модификаторы. Экстрим заменяет rawcodes через up; все замены находятся в unit-roster-assignments.csv.','','| Этап | Роль | ID / имя | HP field | armor field | base+dice*sides | interval | speed | abilities |','| ---: | --- | --- | ---: | ---: | --- | ---: | ---: | --- |']
        for w in waves:
            wavemd.append(f'| {w["stage"]} | {w["role"]} | {w["unit_id"]} {w["unit_name"]} | {w.get("HP_field", "")} | {w.get("armor_field", "")} | {w.get("damage_base", "")} + {w.get("dice", "")}d{w.get("sides", "")} | {w.get("attack_interval", "")} | {w.get("speed", "")} | {w.get("ability_ids", "")} |')
        (out/'waves.md').write_text('\n'.join(wavemd)+'\n',encoding='utf-8')
    csvfile(out/'gameplay-constants.csv',[{'section':s,**row,'source':'war3mapMisc.txt','section_line':d['_line']} for s,d in prof.get('war3mapMisc.txt',{}).items() for row in d['_assignments']])
    itemmd=['# Предметы Warcraft '+args.version,'','Все записи ItemData, включая базовые, расходуемые, recipe и shop surrogate objects. goldcost - явное поле, не гарантированная полная стоимость сборки. Tooltip - текст карты, не runtime-доказательство.','', '| ID | Название | goldcost | abilities | описание |','| --- | --- | ---: | --- | --- |']
    for oid,i in items.items():
        itemmd.append('| '+oid+' | '+i['display_name'].replace('|','\\|')+' | '+str(i.get('goldcost',''))+' | '+str(i.get('abilList',''))+' | '+clean(i.get('Ubertip',i.get('Description',''))).replace('|','\\|').replace('\n',' / ')+' |')
    (out/'items.md').write_text('\n'.join(itemmd)+'\n',encoding='utf-8')
    hero_md=['# Выбираемые герои Warcraft '+args.version,'','Статические поля таблицы карты; HP/armor/damage показаны как поля, без недоказанных engine-derived итогов. Пустые значения не означают 0. Полные поля: units.json; связи: hero-abilities.csv; способности: abilities.json.','', '| ID | Герой | Таверна | HP field | STR/AGI/INT | Прирост STR/AGI/INT | primary | speed | hero abilities |','| --- | --- | --- | ---: | --- | --- | --- | ---: | --- |']
    for h in heroes:hero_md.append(f'| {h["id"]} | {h["display_name"]} | {h["tavern"]} | {h.get("HP", "")} | '+ '/'.join(str(h.get(k) or '') for k in ('STR','AGI','INT'))+' | '+ '/'.join(str(h.get(k) if h.get(k) is not None else '') for k in ('STRplus','AGIplus','INTplus'))+f' | {h.get("Primary")} | {h.get("spd")} | {h.get("heroAbilList")} |')
    (out/'heroes.md').write_text('\n'.join(hero_md)+'\n',encoding='utf-8')
    recipe_md=['# Рецепты Warcraft '+args.version,'','Строки JASS обозначают реальные регистрации крафта. Повтор ингредиента сохраняется как количество. Число предметов и число рецептов - разные показатели.','', '| Результат | Состав | JASS line |','| --- | --- | ---: |']
    for r in recipes:recipe_md.append('| '+r['result_id']+' '+str(r['result_name'])+' | '+' + '.join(f'{n} x {i} {r["ingredient_names"].get(i)}' for i,n in r['ingredients'].items())+' | '+str(r['source_line'])+' |')
    (out/'recipes.md').write_text('\n'.join(recipe_md)+'\n',encoding='utf-8')
    # A searchable per-hero ledger with all ability tables, tooltips, and raw field overrides.
    ledger=['# Герои и способности '+args.version,'','Автоматическая детализация явных полей. Tooltip не считается подтверждением runtime-эффекта. Profile conflicts не разрешены движком: merged view показывает последнее посещенное присваивание, все варианты находятся в profile-duplicates.json и profile-conflicts.json. Любая логика JASS требует чтения указанных функций и их вызываемых функций.','']
    for h in heroes:
        ledger+=['## '+h['id']+' '+str(h['display_name']),'', 'Источник выбора: JASS:'+str(h['source_line'])+'. Поля героя: `units.json`.', '']
        for link in [x for x in links if x['hero_id']==h['id']]:
            a=abilities.get(link['ability_id'],{});ledger+=['### '+link['ability_id']+' '+str(a.get('display_name','не найдено'))+' ('+link['role']+')','']
            ledger+=['| Поле | Значение |','| --- | --- |']
            for k,v in a.items():
                if k in ['id','sources','jass_functions','binary_overrides']:continue
                if any(s in k.lower() for s in ['art','sound','missile','attachment','anim','button','effect','researchhotkey','researchtip']):continue
                ledger.append('| '+str(k)+' | '+clean(v).replace('|','\\|').replace('\n',' / ')+' |')
            ledger+=['','Источники: '+', '.join(a.get('sources',[])), '','Функции с прямой ссылкой на rawcode: '+', '.join(a.get('jass_functions',[])), '']
            if a.get('binary_overrides'):ledger+=['Двоичные overrides (применяются поверх SLK):','', '```json',json.dumps(a['binary_overrides'],ensure_ascii=False,indent=2),'```','']
    (out/'hero-ability-ledger.md').write_text('\n'.join(line.rstrip() for line in ledger).rstrip()+'\n',encoding='utf-8')
    summary={'version':args.version,'units':len(units),'items':len(items),'abilities':len(abilities),'upgrades':len(upgrades),'buffs':len(buffs),'selectable_hero_stock_calls':len(heroes),'unique_selectable_hero_ids':len({x['id'] for x in heroes}),'taverns':len({x['tavern'] for x in heroes}),'hero_ability_links':len(links),'recipes':len(recipes),'unique_recipe_results':len({x['result_id'] for x in recipes}),'recipe_edges':len(edges),'shop_conversions':len(conversions),'explicit_object_fields':len(allfields),'unknown_recipe_ingredients':sorted({i for r in recipes for i in r['ingredients'] if i not in items}),'unknown_selectable_abilities':sorted({r['ability_id'] for r in links if r['ability_id'] not in abilities})}
    summary.update(profile_duplicate_assignments=len(duplicates),profile_conflicting_duplicates=sum(r['previous_value']!=r['value'] for r in duplicates),cross_profile_conflicts=len(profile_conflicts))
    js(out/'catalog-validation.json',summary);print(json.dumps(summary,ensure_ascii=False,indent=2))

if __name__=='__main__':main()
