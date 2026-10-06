"""Build an isolated, visibly instrumented LiA 3.9c research copy, never the source.

The human authorized this local developer copy for observation on 2026-10-05.
No Warcraft cheat commands or anti-cheat changes are made. JASS is extracted and
patched as bytes only inside ignored .local; no original game code goes to Unity.

Official StormLib v9.40 sources for the narrowly scoped archive operation:
https://github.com/ladislav-zezula/StormLib/blob/v9.40/src/StormLib.h
https://github.com/ladislav-zezula/StormLib/blob/v9.40/src/SFileOpenArchive.cpp#L503
https://github.com/ladislav-zezula/StormLib/blob/v9.40/src/SBaseFileTable.cpp#L502

Original archive has a deliberately nonstandard four-byte header-size field.
Only that field is normalized in the COPY before SFileAddFileEx is allowed.
If StormLib still reports MALFORMED, stop; never clear internal library flags.
The library may update internal listfile/attributes; all other payloads and every
original hash-table identity are verified. Archive compaction is not used.
"""
from __future__ import annotations

import argparse
import ctypes
import hashlib
import json
import re
import struct
import sys
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools/research"))
from warcraft_extract import Archive, StormReader  # noqa: E402

SOURCE = ROOT / ".local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x"
OUTPUT_DIR = ROOT / ".local/lia-port/research-map"
OUTPUT = OUTPUT_DIR / "LiA39c_RESEARCH.w3x"
DLL = ROOT / ".local/research/lia/warcraft/stormlib/x64/StormLib.dll"
MANIFEST = ROOT / "research/lia/warcraft/3.9c/extraction-manifest.json"
SOURCE_SHA = "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34"
SCRIPT_NAME = "Scripts\\war3map.j"
SCRIPT_SHA = "fe69d5ec5087303ac93a696c602b746565ee5e9e52917f18a9de3ee47d6824e4"
MPQ_OFFSET = 512
HEADER_SIZE_OFFSET = MPQ_OFFSET + 4
ORIGINAL_HEADER_SIZE = 0x504F7856
SFILE_MPQ_FLAGS = 39  # enum SFileInfoClass in the pinned official header
MPQ_MALFORMED = 0x4
MPQ_READ_ONLY = 0x1
PREFIX = b"LiaResearch"


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def require(condition: bool, message: str) -> None:
    if not condition:
        raise ValueError(message)


def script_function(script: bytes, name: str) -> bytes:
    # The original uses mixed CR, LF and CRLF. Preserve all existing bytes.
    pattern = rb"(?:\A|(?<=[\r\n]))function " + re.escape(name.encode()) + rb" takes [^\r\n]*[\r\n]+.*?(?<=[\r\n])endfunction(?=[\r\n]|\Z)"
    matches = list(re.finditer(pattern, script, re.S))
    require(len(matches) == 1, f"Expected exactly one function {name}")
    return matches[0].group()


def instrumentation() -> bytes:
    # Own instrumentation, using natives present in the local Warcraft 1.26
    # War3Patch.mpq/Scripts/common.j. No Blz* APIs or inferred damage/armor getters.
    return r'''function LiaResearchHero takes nothing returns unit
local integer i=xK(Player(0))
if i>=1 and i<=8 then
if ar[i]!=null and GetOwningPlayer(ar[i])==Player(0) then
return ar[i]
endif
endif
return null
endfunction
function LiaResearchChar takes integer n returns string
if n>=48 and n<=57 then
return SubString("0123456789",n-48,n-47)
elseif n>=65 and n<=90 then
return SubString("ABCDEFGHIJKLMNOPQRSTUVWXYZ",n-65,n-64)
elseif n>=97 and n<=122 then
return SubString("abcdefghijklmnopqrstuvwxyz",n-97,n-96)
endif
return "?"
endfunction
function LiaResearchRawcode takes integer n returns string
local integer i=0
local integer b=0
local string s=""
loop
exitwhen i==4
set b=n-(n/256)*256
set s=LiaResearchChar(b)+s
set n=n/256
set i=i+1
endloop
return s
endfunction
function LiaResearchBanner takes nothing returns nothing
if LiaResearchHeal then
call DisplayTextToPlayer(Player(0),0,0,"|cffff8800RESEARCH COPY - HEAL ON (0.03s), normal damage unchanged; lethal hits can still kill.|r")
else
call DisplayTextToPlayer(Player(0),0,0,"|cffff8800RESEARCH COPY - HEAL OFF. Level changes remain; this is not the original map.|r")
endif
endfunction
function LiaResearchSkills takes unit u,integer a,integer b,integer c,integer d returns nothing
call DisplayTextToPlayer(Player(0),0,0,"SKILLS "+LiaResearchRawcode(a)+"="+I2S(GetUnitAbilityLevel(u,a))+" "+LiaResearchRawcode(b)+"="+I2S(GetUnitAbilityLevel(u,b))+" "+LiaResearchRawcode(c)+"="+I2S(GetUnitAbilityLevel(u,c))+" "+LiaResearchRawcode(d)+"="+I2S(GetUnitAbilityLevel(u,d))+" A001="+I2S(GetUnitAbilityLevel(u,'A001')))
endfunction
function LiaResearchStats takes nothing returns nothing
local unit u=LiaResearchHero()
local item it=null
local integer i=0
local integer id=0
call LiaResearchBanner()
if u==null then
call DisplayTextToPlayer(Player(0),0,0,"RESEARCH: choose your hero first (Player 1/red only).")
return
endif
set id=GetUnitTypeId(u)
call DisplayTextToPlayer(Player(0),0,0,"HERO "+LiaResearchRawcode(id)+" "+GetUnitName(u)+" LV="+I2S(GetHeroLevel(u))+" XP="+I2S(GetHeroXP(u))+" X="+R2SW(GetUnitX(u),1,2)+" Y="+R2SW(GetUnitY(u),1,2))
call DisplayTextToPlayer(Player(0),0,0,"HP="+R2SW(GetUnitState(u,UNIT_STATE_LIFE),1,2)+"/"+R2SW(GetUnitState(u,UNIT_STATE_MAX_LIFE),1,2)+" MP="+R2SW(GetUnitState(u,UNIT_STATE_MANA),1,2)+"/"+R2SW(GetUnitState(u,UNIT_STATE_MAX_MANA),1,2)+" MOVE="+R2SW(GetUnitMoveSpeed(u),1,2))
call DisplayTextToPlayer(Player(0),0,0,"ATTR base/total STR="+I2S(GetHeroStr(u,false))+"/"+I2S(GetHeroStr(u,true))+" AGI="+I2S(GetHeroAgi(u,false))+"/"+I2S(GetHeroAgi(u,true))+" INT="+I2S(GetHeroInt(u,false))+"/"+I2S(GetHeroInt(u,true)))
call DisplayTextToPlayer(Player(0),0,0,"GOLD="+I2S(GetPlayerState(Player(0),PLAYER_STATE_RESOURCE_GOLD))+" LUMBER="+I2S(GetPlayerState(Player(0),PLAYER_STATE_RESOURCE_LUMBER))+" (not modified by research commands)")
if id=='H008' then
call LiaResearchSkills(u,'A05N','A05M','A102','A0E6')
elseif id=='N0A0' then
call LiaResearchSkills(u,'A15W','A0AS','A15X','A0AC')
elseif id=='H024' then
call LiaResearchSkills(u,'A0SJ','A0SP','A0AE','A0SM')
else
call DisplayTextToPlayer(Player(0),0,0,"SKILLS: level probes defined only for H008, N0A0, H024.")
endif
loop
exitwhen i==6
set it=UnitItemInSlot(u,i)
if it!=null then
call DisplayTextToPlayer(Player(0),0,0,"ITEM "+I2S(i+1)+" "+LiaResearchRawcode(GetItemTypeId(it))+" "+GetItemName(it)+" CHARGES="+I2S(GetItemCharges(it)))
else
call DisplayTextToPlayer(Player(0),0,0,"ITEM "+I2S(i+1)+" empty")
endif
set i=i+1
endloop
set it=null
set u=null
endfunction
function LiaResearchTick takes nothing returns nothing
local unit u=null
if LiaResearchHeal then
set u=LiaResearchHero()
if u!=null and GetUnitState(u,UNIT_STATE_LIFE)>.405 then
call SetUnitState(u,UNIT_STATE_LIFE,GetUnitState(u,UNIT_STATE_MAX_LIFE))
endif
endif
set u=null
endfunction
function LiaResearchChat takes nothing returns nothing
local string s=GetEventPlayerChatString()
local string value=""
local integer n=0
local unit u=null
if s=="-research-heal on" then
set LiaResearchHeal=true
call LiaResearchBanner()
elseif s=="-research-heal off" then
set LiaResearchHeal=false
call LiaResearchBanner()
elseif s=="-research-stats" then
call LiaResearchStats()
elseif SubString(s,0,16)=="-research-level " then
set value=SubString(s,16,StringLength(s))
set n=S2I(value)
set u=LiaResearchHero()
if n<1 or n>50 or I2S(n)!=value then
call DisplayTextToPlayer(Player(0),0,0,"RESEARCH: level must be an integer 1..50, e.g. -research-level 10")
elseif u==null then
call DisplayTextToPlayer(Player(0),0,0,"RESEARCH: choose your hero first.")
else
call DisplayTextToPlayer(Player(0),0,0,"|cffff8800RESEARCH LEVEL CHANGE requested="+I2S(n)+" via SetHeroLevel; native/map level events apply.|r")
call SetHeroLevel(u,n,false)
call LiaResearchStats()
endif
set u=null
else
call DisplayTextToPlayer(Player(0),0,0,"RESEARCH commands: -research-heal on / -research-heal off / -research-stats / -research-level N (1..50)")
endif
endfunction
function LiaResearchInit takes nothing returns nothing
local trigger t=CreateTrigger()
call TriggerRegisterPlayerChatEvent(t,Player(0),"-research",false)
call TriggerAddAction(t,function LiaResearchChat)
call TimerStart(CreateTimer(),.03,true,function LiaResearchTick)
call TimerStart(CreateTimer(),5.,true,function LiaResearchBanner)
call LiaResearchBanner()
set t=null
endfunction
'''.replace("\n", "\r\n").encode("ascii")


def patch_script(original: bytes) -> tuple[bytes, list[dict]]:
    require(sha(original) == SCRIPT_SHA, "Original script hash mismatch")
    require(PREFIX not in original, "Research prefix already present")
    main = script_function(original, "main")
    main_start = original.index(main)
    end_main = main_start + main.rindex(b"endfunction")
    globals_end = re.search(rb"(?<=[\r\n])endglobals(?=[\r\n])", original)
    require(globals_end is not None, "Missing globals terminator")
    inserts = [
        (globals_end.start(), b"boolean LiaResearchHeal=true\r\n", "own globals"),
        (main_start, instrumentation(), "own functions"),
        (end_main, b"call LiaResearchInit()\r\n", "initialize at end of main"),
    ]
    result = original
    for offset, payload, _ in reversed(inserts):
        result = result[:offset] + payload + result[offset:]
    # Prove that deleting the exact inserted ranges recovers the original bytes.
    records = []
    shift = 0
    for offset, payload, purpose in inserts:
        records.append({"original_offset": offset, "patched_offset": offset + shift,
                        "bytes": len(payload), "sha256": sha(payload), "purpose": purpose})
        shift += len(payload)
    restored = result
    for entry in reversed(records):
        pos = entry["patched_offset"]
        restored = restored[:pos] + restored[pos + entry["bytes"]:]
    require(restored == original, "Patch modified original bytes")
    return result, records


def library() -> ctypes.WinDLL:
    manifest = json.loads((ROOT / "research/lia/warcraft/acquisition-manifest.json").read_text(encoding="utf-8"))
    expected = next(r["dll_sha256"] for r in manifest["readers"] if r["name"] == "StormLib")
    require(sha(DLL.read_bytes()) == expected, "Official DLL hash mismatch")
    lib = ctypes.WinDLL(str(DLL), use_last_error=True)
    h, d, b = ctypes.c_void_p, ctypes.c_uint32, ctypes.c_bool
    signatures = {
        "SFileOpenArchive": ([ctypes.c_wchar_p, d, d, ctypes.POINTER(h)], b),
        "SFileGetFileInfo": ([h, ctypes.c_int, h, d, ctypes.POINTER(d)], b),
        "SFileAddFileEx": ([h, ctypes.c_wchar_p, ctypes.c_char_p, d, d, d], b),
        "SFileFlushArchive": ([h], b), "SFileCloseArchive": ([h], b),
    }
    for name, (args, result) in signatures.items():
        fn = getattr(lib, name)
        fn.argtypes, fn.restype = args, result
    return lib


def archive_flags(lib, handle) -> int:
    result, size = ctypes.c_uint32(), ctypes.c_uint32()
    require(lib.SFileGetFileInfo(handle, SFILE_MPQ_FLAGS, ctypes.byref(result), 4, ctypes.byref(size)), "Cannot read archive flags")
    return result.value


def active_entries(archive: Archive) -> dict:
    result = {}
    for entry in archive.hash_table:
        if entry.block_table_index in (0xFFFFFFFF, 0xFFFFFFFE):
            continue
        block = archive.block_table[entry.block_table_index]
        require(block.flags & 0x80000000, "Active hash points to missing block")
        key = (entry.hash_a, entry.hash_b, entry.locale, entry.platform)
        require(key not in result, "Duplicate archive identity")
        result[key] = entry
    return result


def verify_copy(original_script: bytes, expected_script: bytes, output: Path = OUTPUT,
                preserve_script: bool = True) -> dict:
    require(sha(SOURCE.read_bytes()) == SOURCE_SHA, "ORIGINAL CHANGED")
    old, new = Archive(SOURCE), Archive(output)
    require(old.raw[:MPQ_OFFSET] == new.raw[:MPQ_OFFSET], "Warcraft prefix changed")
    require(struct.unpack_from("<I", new.raw, HEADER_SIZE_OFFSET)[0] == 32, "Research header size changed")
    a, b = active_entries(old), active_entries(new)
    require(len(a) == 1477, "Unexpected original entry count")
    require(a.keys() == b.keys(), "Archive identities added or lost")
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    names = {x["block_index"]: x["name"] for x in manifest["recognized_files"]}
    r1, r2 = StormReader(SOURCE), StormReader(output)
    records, changed = [], []
    try:
        # StormReader adds READ_ONLY by design; MALFORMED must remain absent.
        copy_flags = archive_flags(library(), r2.handle)
        require(not copy_flags & MPQ_MALFORMED, "Final copy is malformed")
        for key, e1 in a.items():
            e2 = b[key]
            name = names.get(e1.block_table_index)
            old_name = name or f"File{e1.block_table_index:08d}.xxx"
            new_name = name or f"File{e2.block_table_index:08d}.xxx"
            p1, p2 = r1.read_file(old_name), r2.read_file(new_name)
            require(p1 is not None and p2 is not None, f"Unreadable entry {old_name}")
            h1, h2 = sha(p1), sha(p2)
            record = {"key": list(key), "name": name, "old_block": e1.block_table_index,
                      "new_block": e2.block_table_index, "old_sha256": h1, "new_sha256": h2,
                      "same": p1 == p2}
            records.append(record)
            if p1 != p2:
                require(name is not None and name.lower() in (SCRIPT_NAME.lower(), "(listfile)", "(attributes)"), f"Unexpected payload change: {old_name}")
                changed.append(name)
        require(r2.read_file(SCRIPT_NAME) == expected_script, "Script read-back mismatch")
        require(r2.read_file("war3map.j") is None, "Unexpected higher-priority root script")
    finally:
        r1.close()
        r2.close()
    # Includes all anti-cheat functions and their original registrations, since
    # ALL source bytes survive the insertion proof, plus explicit function hashes.
    anti_cheat = {}
    anti_cheat_names = ("NCv", "Ndv", "NDv", "Nfv", "Ngv", "NGv", "Nhv", "Njv", "NJv", "Nkv", "Nlv", "NLv", "Nmv") if preserve_script else ()
    for name in anti_cheat_names:
        before, after = script_function(original_script, name), script_function(expected_script, name)
        require(before == after, f"Anti-cheat function changed: {name}")
        anti_cheat[name] = sha(before)
    return {"entries_verified": len(records), "identical_payloads": sum(x["same"] for x in records),
            "final_read_only_flags": copy_flags,
            "hash_keys_preserved": True, "warcraft_prefix_preserved": True,
            "changed_payloads": changed, "anti_cheat_function_sha256": anti_cheat, "entries": records}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--build", action="store_true", help="Create the approved isolated copy; refuses existing output")
    parser.add_argument("--verify", action="store_true", help="Read-only verification of an existing copy")
    args = parser.parse_args()
    require(args.build != args.verify, "Choose exactly one of --build or --verify")
    require(SOURCE.resolve() != OUTPUT.resolve(), "Source/output collision")
    require(OUTPUT.resolve().is_relative_to((ROOT / ".local").resolve()), "Output escaped .local")
    raw = SOURCE.read_bytes()
    require(sha(raw) == SOURCE_SHA, "Original map hash mismatch")
    require(raw[MPQ_OFFSET:MPQ_OFFSET + 4] == b"MPQ\x1a", "Unexpected MPQ position")
    require(struct.unpack_from("<I", raw, HEADER_SIZE_OFFSET)[0] == ORIGINAL_HEADER_SIZE, "Unexpected source header")
    reader = StormReader(SOURCE)
    try:
        script = reader.read_file(SCRIPT_NAME)
        require(script is not None, "Original JASS missing")
    finally:
        reader.close()
    patched, insertions = patch_script(script)
    lib = library()
    normalized_flags = None
    if args.build:
        require(not OUTPUT.exists(), "Output already exists; refusing replacement")
        OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
        script_path = OUTPUT_DIR / "war3map.research.j"
        script_path.write_bytes(patched)
        # Exclusive creation and explicit destination guard protect the original.
        with OUTPUT.open("xb") as dest:
            dest.write(raw)
        with OUTPUT.open("r+b") as dest:
            dest.seek(HEADER_SIZE_OFFSET)
            dest.write(struct.pack("<I", 32))
        normalized = OUTPUT.read_bytes()
        expected = raw[:HEADER_SIZE_OFFSET] + struct.pack("<I", 32) + raw[HEADER_SIZE_OFFSET + 4:]
        require(normalized == expected, "Header normalization changed extra bytes")
        handle = ctypes.c_void_p()
        require(lib.SFileOpenArchive(str(OUTPUT), 0, 0, ctypes.byref(handle)), f"Copy open failed: {ctypes.get_last_error()}")
        try:
            normalized_flags = archive_flags(lib, handle)
            require(not normalized_flags & (MPQ_MALFORMED | MPQ_READ_ONLY), f"Copy still malformed/read-only: {normalized_flags:#x}")
            # REPLACEEXISTING | COMPRESS, zlib first and subsequent sectors.
            require(lib.SFileAddFileEx(handle, str(script_path), SCRIPT_NAME.encode("ascii"), 0x80000200, 0x02, 0x02), f"Script replacement failed: {ctypes.get_last_error()}")
            require(lib.SFileFlushArchive(handle), f"Archive flush failed: {ctypes.get_last_error()}")
        finally:
            require(lib.SFileCloseArchive(handle), "Archive close failed")
    else:
        require(OUTPUT.is_file(), "Research copy missing")
    result = verify_copy(script, patched)
    report = {"verified_utc": datetime.now(timezone.utc).isoformat(), "source": str(SOURCE.relative_to(ROOT)),
              "source_sha256": sha(SOURCE.read_bytes()), "copy": str(OUTPUT.relative_to(ROOT)),
              "copy_sha256": sha(OUTPUT.read_bytes()), "original_script_sha256": sha(script),
              "research_script_sha256": sha(patched), "header_normalization": {"offset": HEADER_SIZE_OFFSET,
                  "before": ORIGINAL_HEADER_SIZE, "after": 32, "bytes": 4},
              "post_normalization_flags": normalized_flags, "insertions": insertions,
              "all_original_script_bytes_preserved": True,
              "commands": ["-research-heal on", "-research-heal off", "-research-stats", "-research-level N"],
              "heal_default": "ON", "level_bounds": [1, 50], "warcraft_executed": False,
              "limits": ["Periodic healing cannot prevent lethal single hits or direct KillUnit.",
                         "Level commands invoke native/map events; compare requested and actual level/XP.",
                         "No direct mana/gold/stat/damage/cooldown overrides, invulnerability or anti-cheat edits; native level changes still apply their normal effects.",
                         "No save-game support is added or claimed. Full normal-combat evidence requires original map.",
                         "Archive/insertion verification is not Warcraft JASS compilation or runtime validation."],
              **result}
    report_path = OUTPUT_DIR / "verification.json"
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in report.items() if k not in ("entries", "anti_cheat_function_sha256")}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
