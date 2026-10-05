"""Acquire published LiA maps and pinned official/open-source readers locally.

Run only when downloads are authorized. HTTP 403 is a terminal failure; no
alternate credentials, spoofed user agents, or restriction workarounds exist.
"""
from __future__ import annotations
import datetime, hashlib, html, io, json, re, urllib.request, zipfile
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'.local/research/lia/warcraft'
MPYQ_COMMIT='6bfba18ec403f702666b4109db3d95f3b97b1dc5'
MAPS={
 '3.4':(313063,'Life_in_Arena_v3_4.w3x','525e3dfdf2d3926bfcaa92582d751dea03b19c458f6dbf6004e5c5f7b76ed63a'),
 '3.9c':(354243,'Life_in_Arena_v3_9_c.w3x','02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34'),
}
def fetch(url):
    with urllib.request.urlopen(url,timeout=60) as response:
        return response.read(),response.headers.get('Content-Type'),response.status
def sha(data):return hashlib.sha256(data).hexdigest()
def main():
    BASE.mkdir(parents=True,exist_ok=True);records=[]
    for version,(mapid,name,digest) in MAPS.items():
        path=BASE/name;page=f'https://www.epicwar.com/maps/{mapid}/'
        if path.exists():
            data=path.read_bytes()
            if sha(data)!=digest:raise ValueError('Existing file hash mismatch: '+name)
            status='existing_local_verified';ctype='not re-requested'
        else:
            markup,_,_=fetch(page)
            links=re.findall(r'href="([^"]+)"',markup.decode('utf-8'))
            url=next(html.unescape(u) for u in links if f'/maps/{mapid}/download/' in u)
            data,ctype,http=fetch(url)
            if sha(data)!=digest:raise ValueError(f'{name}: expected snapshot differs, preserve separately and review')
            if b'MPQ\x1a' not in data[:4096]:raise ValueError('Not an MPQ map')
            path.write_bytes(data);status=f'downloaded HTTP {http}'
        records.append({'version':version,'map_path':str(path.relative_to(ROOT)).replace('\\','/'),'page_url':page,'download_route':page+'download/','status':status,'content_type':ctype,'file_timestamp_utc':datetime.datetime.fromtimestamp(path.stat().st_mtime,datetime.timezone.utc).isoformat(),'checked_utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'size':len(data),'sha256':sha(data)})
    readerdir=BASE/'mpyq-source';readerdir.mkdir(exist_ok=True)
    for filename in ['mpyq.py','LICENSE']:
        path=readerdir/filename
        if not path.exists():path.write_bytes(fetch(f'https://raw.githubusercontent.com/eagleflo/mpyq/{MPYQ_COMMIT}/{filename}')[0])
    zip_path=BASE/'stormlib_dll.zip';zip_digest='b2c9635e7b63edee1bd7c82e7dc180d739f3accb2b8994804c7774e464ce89ae'
    zip_url='https://github.com/ladislav-zezula/StormLib/releases/download/v9.40/stormlib_dll.zip'
    if not zip_path.exists():zip_path.write_bytes(fetch(zip_url)[0])
    if sha(zip_path.read_bytes())!=zip_digest:raise ValueError('StormLib release digest mismatch')
    with zipfile.ZipFile(zip_path) as z:
        for filename in z.namelist():
            target=(BASE/'stormlib'/filename).resolve()
            if (BASE/'stormlib').resolve() not in target.parents:raise ValueError('Unsafe ZIP path')
        z.extractall(BASE/'stormlib')
    manifest={'acquisitions':records,'readers':[{'name':'mpyq','version':'0.2.5','repository':'https://github.com/eagleflo/mpyq','commit':MPYQ_COMMIT,'source_sha256':sha((readerdir/'mpyq.py').read_bytes()),'license':'BSD-2-Clause; source inspected; no global installation'},{'name':'StormLib','version':'9.40','url':zip_url,'zip_sha256':zip_digest,'publisher_digest_api':'https://api.github.com/repos/ladislav-zezula/StormLib/releases/latest','asset_id':466311987,'dll_sha256':sha((BASE/'stormlib/x64/StormLib.dll').read_bytes()),'license':'MIT; local native DLL called only for archive reading'}],'initial_attempts':[{'url':'https://wc3maps.com/api/download/181024','redirect':'https://storagebox.wc3maps.com/181024/Life_in_Arena_v3_4.w3x','outcome':'connection timed out after 60 seconds with 0 bytes; independent EpicWar publication used'},{'page':'EpicWar download anchor with HTML entity &amp;','outcome':'un-decoded anchor initially returned HTML200, not a map; standard HTML decoding yielded official signed download and valid octet-stream200 for both versions'}],'license_boundary':'Map/content reuse rights not established. Local analysis only. Do not copy maps, scripts, models, sounds, names or branding into Unity/game or publish without rights review.','execution':'Only archive decompression and static parsing. No JASS, triggers, game map or asset code executed.'}
    out=ROOT/'research/lia/warcraft/acquisition-manifest.json';out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(records,ensure_ascii=False,indent=2))
if __name__=='__main__':main()
