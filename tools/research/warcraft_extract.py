"""Read-only MPQ/object-data extraction for the acquired LiA maps.

No Warcraft script is executed. Third-party mpyq stays under ignored .local.
MPQ encrypted-sector adaptation follows StormLib SFileReadFile.cpp / MPQ format.
"""
from __future__ import annotations
import argparse, bz2, csv, ctypes, hashlib, io, json, re, struct, sys, zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
LOCAL = ROOT / '.local/research/lia/warcraft'
sys.path.insert(0, str(LOCAL / 'mpyq-source'))
import mpyq

def save_json(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

def decode(data):
    try: return data.decode('utf-8-sig')
    except UnicodeDecodeError: return data.decode('cp1251')

class StormReader:
    """Independent official StormLib 9.40 reader, no game-code execution."""
    def __init__(self, path):
        self.dll=ctypes.WinDLL(str(LOCAL/'stormlib/x64/StormLib.dll'))
        H=ctypes.c_void_p;D=ctypes.c_uint32;B=ctypes.c_bool
        self.dll.SFileOpenArchive.argtypes=[ctypes.c_wchar_p,D,D,ctypes.POINTER(H)]
        self.dll.SFileOpenArchive.restype=B
        self.dll.SFileOpenFileEx.argtypes=[H,ctypes.c_char_p,D,ctypes.POINTER(H)]
        self.dll.SFileOpenFileEx.restype=B
        self.dll.SFileGetFileSize.argtypes=[H,ctypes.POINTER(D)];self.dll.SFileGetFileSize.restype=D
        self.dll.SFileReadFile.argtypes=[H,H,D,ctypes.POINTER(D),H];self.dll.SFileReadFile.restype=B
        self.dll.SFileCloseFile.argtypes=[H];self.dll.SFileCloseArchive.argtypes=[H]
        self.handle=H()
        if not self.dll.SFileOpenArchive(str(path),0,0x100,ctypes.byref(self.handle)):raise ValueError('StormLib cannot open archive')
    def read_file(self,name):
        handle=ctypes.c_void_p()
        if not self.dll.SFileOpenFileEx(self.handle,name.encode(),0,ctypes.byref(handle)):return None
        try:
            high=ctypes.c_uint32();size=self.dll.SFileGetFileSize(handle,ctypes.byref(high))
            assert high.value==0 and size<100_000_000
            buf=ctypes.create_string_buffer(size);read=ctypes.c_uint32()
            if not self.dll.SFileReadFile(handle,buf,size,ctypes.byref(read),None):raise ValueError('StormLib read failure: '+name)
            assert read.value==size
            return buf.raw
        finally:self.dll.SFileCloseFile(handle)
    def close(self):self.dll.SFileCloseArchive(self.handle)

class Archive(mpyq.MPQArchive):
    def __init__(self, path):
        self.raw = path.read_bytes()
        self.base = next((o for o in range(0, len(self.raw)-32, 512) if self.raw[o:o+4] == b'MPQ\x1a'), None)
        if self.base is None: raise ValueError('No aligned MPQ header')
        super().__init__(io.BytesIO(self.raw[self.base:]), listfile=False)

    def read_file(self, name, force_decompress=False):
        h = self.get_hash_table_entry(name)
        if h is None: return None
        b = self.block_table[h.block_table_index]
        if not b.flags & 0x80000000: return None
        data = self.raw[self.base+b.offset:self.base+b.offset+b.archived_size]
        assert len(data) == b.archived_size
        encrypted = bool(b.flags & 0x10000)
        key = self._hash(name.split('\\')[-1], 'TABLE')
        if b.flags & 0x20000: key = ((key+b.offset)^b.size)&0xffffffff
        def decrypt(chunk, k):
            count = len(chunk)//4*4
            return self._decrypt(chunk[:count], k&0xffffffff)+chunk[count:]
        def unpack(chunk, size):
            if len(chunk) < size and b.flags & 0x200:
                if chunk[0] == 2: chunk = zlib.decompress(chunk[1:])
                elif chunk[0] == 16: chunk = bz2.decompress(chunk[1:])
                else: raise ValueError(f'Unsupported compression {chunk[0]} for {name}')
            if b.flags & 0x100: raise ValueError('PKWARE unsupported')
            assert len(chunk) == size, (name, len(chunk), size)
            return chunk
        if b.flags & 0x1000000:
            return unpack(decrypt(data,key) if encrypted else data,b.size)
        sector = 512 << self.header['sector_size_shift']
        n = (b.size+sector-1)//sector
        if b.flags & 0x200:
            table = data[:4*(n+1+(1 if b.flags&0x4000000 else 0))]
            if encrypted: table = decrypt(table,key-1)
            offsets = struct.unpack('<'+'I'*(len(table)//4),table)
            assert all(0 <= x <= len(data) for x in offsets), (name, offsets)
            chunks = [data[offsets[i]:offsets[i+1]] for i in range(n)]
        else: chunks = [data[i*sector:(i+1)*sector] for i in range(n)]
        return b''.join(unpack(decrypt(c,key+i) if encrypted else c,min(sector,b.size-i*sector)) for i,c in enumerate(chunks))

def candidates():
    names = ['(listfile)','(attributes)','(signature)','scripts\\war3map.j','war3map.j']
    names += ['war3map.'+x for x in ['w3u','w3t','w3a','w3b','w3d','w3h','w3q','wts','w3i','w3e','wpm','shd','doo','wtg','wct','mmp','imp','w3r','w3c','w3s','w3f']]
    names += ['war3mapUnits.doo','war3mapMisc.txt','war3mapExtra.txt','war3mapSkin.txt','war3mapMap.blp','war3mapPreview.tga','Units\\ItemFunc.txt','Units\\ItemStrings.txt','Units\\ItemAbilityFunc.txt','Units\\ItemAbilityStrings.txt','Splats\\LightningData.slk']
    for stem in ['UnitAbilities','UnitBalance','UnitData','UnitUI','UnitWeapons','ItemData','AbilityData','AbilityBuffData','AbilityBuffMetaData','AbilityMetaData','UnitMetaData','UpgradeData','UpgradeMetaData','DestructableData']:
        names.append('Units\\'+stem+'.slk')
    for race in ['Human','Orc','Undead','NightElf','Neutral','Campaign','Common']:
        for kind in ['Unit','Ability','Upgrade','Item']:
            for suffix in ['Strings','Func']:
                names.append('Units\\'+race+kind+suffix+'.txt')
    return names

def slk(text):
    cells={}; x=y=0
    for lineno,line in enumerate(text.splitlines(),1):
        if not line.startswith('C;'): continue
        parts=re.findall(r'(?:"(?:[^"]|"")*"|[^;])+',line)
        val=None
        for p in parts[1:]:
            if p.startswith('X'): x=int(p[1:])
            elif p.startswith('Y'): y=int(p[1:])
            elif p.startswith('K'):
                val=p[1:]
                if val.startswith('"') and val.endswith('"'): val=val[1:-1].replace('""','"')
                else:
                    try: val=float(val) if any(t in val for t in '.eE') else int(val)
                    except ValueError: pass
        if val is not None: cells[x,y]=(val,lineno)
    headers={x:v[0] for (x,y),v in cells.items() if y==1}
    rows=[]
    for y in sorted({y for x,y in cells if y>1}):
        row={headers.get(x,str(x)):v[0] for (x,yy),v in cells.items() if yy==y}
        row['_slk_row']=y
        row['_line']=min(v[1] for (x,yy),v in cells.items() if yy==y)
        rows.append(row)
    return rows

def ini(text):
    sections={}; current=None
    for n,line in enumerate(text.splitlines(),1):
        line=line.strip()
        if not line or line.startswith('//'): continue
        if line.startswith('[') and line.endswith(']'):
            current=line[1:-1]; sections.setdefault(current,{'_line':n,'_assignments':[]}); continue
        if '=' in line and current is not None:
            k,v=line.split('=',1); section=sections[current]
            if k in section:
                prior=next(row for row in reversed(section['_assignments']) if row['field']==k)
                section.setdefault('_duplicates',[]).append({'field':k,'previous_line':prior['line'],
                    'line':n,'previous_value':prior['value'],'value':v})
            section['_assignments'].append({'field':k,'value':v,'line':n})
            section[k]=v
    return sections

def mask_jass_text(text):
    """Blank double-quoted strings and // comments, retaining every source newline."""
    chars=list(text);quoted=False;i=0
    while i<len(text):
        char=text[i]
        if quoted:
            if char=='\\' and i+1<len(text):
                chars[i]=' ';i+=1
                if text[i]!='\n':chars[i]=' '
            elif char=='"':
                chars[i]=' ';quoted=False
            elif char!='\n':chars[i]=' '
        elif char=='"':
            chars[i]=' ';quoted=True
        elif text[i:i+2]=='//':
            while i<len(text) and text[i]!='\n':chars[i]=' ';i+=1
            continue
        i+=1
    return ''.join(chars)


def jass_calls(code):
    keywords={'return','if','elseif','not','and','or','true','false','set','call','local','function'}
    return set(re.findall(r'\b([A-Za-z_]\w*)\s*\(',code))-keywords


def objects(data, extended, name):
    p=0; rows=[]; counts=[]
    def read(fmt):
        nonlocal p
        size=struct.calcsize(fmt)
        if p+size>len(data): raise ValueError(f'{name}: truncated at {p}')
        value=struct.unpack_from(fmt,data,p);p+=size
        return value[0] if len(value)==1 else value
    version=read('<i')
    if version not in (1,2): raise ValueError(f'Unsupported object version {version}')
    for table in ('original','custom'):
        count=read('<i');counts.append(count)
        assert 0<=count<100000
        for _ in range(count):
            start=p; old=read('4s');new=read('4s');n=read('<i')
            assert 0<=n<100000
            for __ in range(n):
                offset=p;field=read('4s');kind=read('<i')
                level=read('<i') if extended else None
                pointer=read('<i') if extended else None
                value_offset=p
                if kind==0:value=read('<i')
                elif kind in (1,2):value=read('<f')
                elif kind==3:
                    end=data.index(0,p);value=decode(data[p:end]);p=end+1
                else:raise ValueError(f'Unknown object type {kind} at {offset}')
                tail=read('4s')
                if tail not in (b'\0'*4,old,new): raise ValueError(f'Invalid object tail at {p-4}: {tail!r}')
                rows.append(dict(table=table,old_id=old.decode('latin1'),new_id=new.decode('latin1').strip('\0'),object_id=(new if new!=b'\0'*4 else old).decode('latin1'),object_offset=start,offset=offset,field=field.decode('latin1'),type=kind,level=level,pointer=pointer,value_offset=value_offset,value=value,source=name))
    assert p==len(data), (name,p,len(data))
    return dict(version=version,original_objects=counts[0],custom_objects=counts[1],fields=rows,consumed_bytes=p)

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--version',choices=['3.4','3.9c'],required=True)
    ap.add_argument('--out',type=Path);ap.add_argument('--raw-out',type=Path);args=ap.parse_args()
    name='Life_in_Arena_v3_4.w3x' if args.version=='3.4' else 'Life_in_Arena_v3_9_c.w3x'
    path=LOCAL/name;archive=Archive(path);storm=StormReader(path);out=args.out or ROOT/'research/lia/warcraft'/args.version;rawout=args.raw_out or LOCAL/args.version/'extracted'
    names=candidates(); found={};failed={}; todo=list(names)
    while todo:
        n=todo.pop(0)
        if n.lower() in found or n in failed:continue
        try:data=storm.read_file(n)
        except Exception as e:failed[n]=str(e);continue
        if data is None:continue
        target=rawout/n.replace('\\','/')
        if rawout.resolve() not in target.resolve().parents:raise ValueError('Unsafe archive path')
        target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(data)
        h=archive.get_hash_table_entry(n);b=archive.block_table[h.block_table_index]
        found[n.lower()]={'name':n,'size':len(data),'sha256':hashlib.sha256(data).hexdigest(),'block_index':h.block_table_index,'block_offset':b.offset,'flags':hex(b.flags)}
        if n=='(listfile)':todo+=decode(data).splitlines()
        if n.lower().endswith(('.txt','.j','.slk')):
            for match in re.findall(rb'[A-Za-z0-9_\\ /.-]+\.(?:slk|txt|mdx|mdl|blp|wav|mp3|tga)',data):
                todo.append(match.decode('ascii').strip().replace('\\\\','\\'))
    slks={};inis={};objs={};wts={};jass=''
    for record in found.values():
        n=record['name'];data=(rawout/n.replace('\\','/')).read_bytes()
        if n.lower().endswith('.slk'):slks[n]=slk(decode(data))
        elif n.lower().endswith('.txt'):inis[n]=ini(decode(data))
        elif n.lower().endswith(('.w3u','.w3t','.w3b','.w3d','.w3a','.w3h','.w3q')):objs[n]=objects(data,n[-3:] in ('w3a','w3d','w3q'),n)
        elif n.lower().endswith('.wts'):wts={int(k):v for k,v in re.findall(r'STRING\s+(\d+)\s*\{\s*\n(.*?)\n\}',decode(data),re.S)}
        elif n.lower().endswith('.j'):
            jass=decode(data).replace('\r\n','\n').replace('\r','\n');(rawout/'war3map.normalized.j').write_text(jass,encoding='utf-8')
            def rawcode(m):
                b=bytes.fromhex(m[0][2:])
                return "'"+b.decode('ascii')+"'" if all(32<=c<127 and c!=39 for c in b) else m[0]
            (rawout/'war3map.rawcodes.j').write_text(re.sub(r'0x[0-9a-fA-F]{8}\b',rawcode,jass),encoding='utf-8')
    funcs=[];active=None;lines=jass.splitlines();code_lines=mask_jass_text(jass).splitlines();operations=[];constants=[];numeric=[]
    ops=re.compile(r'(?:Set(?:Unit|Hero|Player|Item)|AddHeroXP|Add(?:SpecialEffect|Item)|Unit(?:Damage|Add|Remove)|Create(?:Unit|Item)|ReviveHero|TimerStart|StartTimerBJ|TriggerRegister|AdjustPlayerState|SetPlayerTech|GetRandom)\w*')
    for n,line in enumerate(lines,1):
        code=code_lines[n-1]
        m=re.match(r'(?:constant\s+)?function\s+(\w+)\s+takes\s+(.*?)\s+returns\s+(\w+)',code)
        if m:active=dict(name=m[1],parameters=m[2],returns=m[3],start_line=n,calls=set(),callbacks=set(),rawcodes=set())
        if active:
            active['calls'].update(jass_calls(code));active['rawcodes'].update(re.findall(r"'([^']{4})'",code))
            active['callbacks'].update(re.findall(r'\bfunction\s+(\w+)\s*[,)]',code))
            active['rawcodes'].update(bytes.fromhex(h).decode('ascii') for h in re.findall(r'0x([0-9a-fA-F]{8})\b',code) if all(32<=c<127 for c in bytes.fromhex(h)))
        if ops.search(code):operations.append({'line':n,'function':active['name'] if active else None,'text':line})
        if re.match(r'(?:set |local |if |elseif |return|call |exitwhen)',code) and re.search(r'\d',code):
            numeric.append({'line':n,'function':active['name'] if active else None,'text':line})
        if re.match(r'(?:constant )?(?:integer|real|string|boolean) ',code): constants.append({'line':n,'text':line})
        if code.strip()=='endfunction' and active:
            active['end_line']=n;active['calls']=sorted(active['calls']);active['callbacks']=sorted(active['callbacks']);active['rawcodes']=sorted(active['rawcodes']);funcs.append(active);active=None
    save_json(out/'slk.json',slks);save_json(out/'profiles.json',inis);save_json(out/'objects.json',objs);save_json(out/'strings.json',wts)
    save_json(out/'jass-functions.json',funcs);save_json(out/'jass-balance-operations.json',operations);save_json(out/'jass-globals.json',constants)
    save_json(out/'jass-numeric-expressions.json',numeric)
    actual_hashes=[(i,h) for i,h in enumerate(archive.hash_table) if h.block_table_index< len(archive.block_table)]
    recognized={v['block_index'] for v in found.values()}
    manifest={'version':args.version,'map_name':name,'size':len(archive.raw),'sha256':hashlib.sha256(archive.raw).hexdigest(),'mpq_offset':archive.base,'mpq_header':{k:v.hex() if isinstance(v,bytes) else v for k,v in archive.header.items()},'active_hash_entries':len(actual_hashes),'unique_active_blocks':len({h.block_table_index for _,h in actual_hashes}),'recognized_files':list(found.values()),'failed_files':failed,'unresolved_hash_entries':[{'hash_index':i,**h._asdict(),'block':archive.block_table[h.block_table_index]._asdict()} for i,h in actual_hashes if h.block_table_index not in recognized],'jass_lines':len(lines),'jass_functions':len(funcs),'balance_operations':len(operations),'slk_rows':{n:len(r) for n,r in slks.items()},'object_tables':{n:{k:v for k,v in o.items() if k!='fields'}|{'fields':len(o['fields'])} for n,o in objs.items()},'notes':['Custom object binaries store overrides; missing fields may inherit engine defaults.','No script execution or live match validation.','An absent listfile means unresolved filenames cannot be enumerated from MPQ hashes alone.']}
    save_json(out/'extraction-manifest.json',manifest)
    checks=[]
    for record in found.values():
        n=record['name']
        if n.lower().endswith(('.txt','.j','.slk','.w3a','.w3b','.w3d','.w3h')):
            try:
                independent=archive.read_file(n)
                assert independent==(rawout/n.replace('\\','/')).read_bytes()
                checks.append({'file':n,'independent_reader_match':True})
            except Exception as e:checks.append({'file':n,'independent_reader_match':False,'reason':str(e)})
    save_json(out/'reader-validation.json',checks)
    storm.close()
    print(json.dumps({k:v for k,v in manifest.items() if k not in ('recognized_files','mpq_header','notes','unresolved_hash_entries')},ensure_ascii=False,indent=2))
    print('unresolved_hash_entries',len(manifest['unresolved_hash_entries']))

if __name__=='__main__':main()
