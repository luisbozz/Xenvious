"""Reading the build data sources (offsets.ini) the way Xenvious means them.

Shared by gen.py (writes Xenvious/Generated/BuildData.*.g.cs) and the tests. Only the
standard library, so it runs wherever Python 3.8+ does, also on the GitHub runner.

Paths are read with the decompiler's own meaning, so offsets.ini says the whole truth
about an address and nothing has to be added in code:

    Global_4980736.f_71084[i /*631*/].f_14[i]
      4980736 + 71084        the struct and the field holding the array
      + 1                    every [..] skips the slot before element 0 (the array size)
      + i * 631              element i, added at runtime from the stride (*_NEXT keys)
      + 14 + 1               field 14 of the element is an array again (per team)

An index that is a number ([2 /*11*/]) is part of the address: 1 + 2 * 11; without a
stride comment an element is one slot. An index that is a name (i, j, team, bVar0) is
chosen at runtime and adds only the size slot.
"""
from __future__ import annotations

import re
from collections import OrderedDict

_TOKEN = re.compile(r"^(?:(?:Global|[a-z]?Local)_)?(?:f_)?(-?\d+)$", re.I)
_INDEX = re.compile(r"\[\s*([^\]/\s]+)\s*(?:/\*\s*(\d+)\s*\*/)?\s*\]")


class PathError(ValueError):
    pass


def parse_ini(text: str) -> "OrderedDict[tuple, str]":
    """(SECTION, key lower case) -> value, first occurrence wins, quotes and trailing comments
    removed. The Win32 ini semantics the app used to load with (IniText)."""
    out: "OrderedDict[tuple, str]" = OrderedDict()
    names: dict = {}
    section = ""
    for raw in text.lstrip("﻿").split("\n"):
        line = raw.strip()
        if not line or line[0] == ";":
            continue
        if line[0] == "[":
            end = line.find("]")
            section = (line[1:end] if end > 0 else line[1:]).strip().upper()
            continue
        eq = line.find("=")
        if eq <= 0:
            continue
        key = line[:eq].strip()
        k = (section, key.lower())
        if k in out:
            continue
        out[k] = unquote(line[eq + 1:].strip())
        names[k] = key
    out.names = names  # type: ignore[attr-defined]
    return out


def unquote(v: str) -> str:
    if len(v) >= 2 and v[0] in "\"'":
        close = v.find(v[0], 1)
        if close > 0:
            return v[1:close]
    return v


def is_path(v: str) -> bool:
    """A global, a local or a field path (not a hex number, a list or free text)."""
    v = v.strip()
    if not v or v.lower().startswith(("0x", "-0x")) or "," in v:
        return False
    try:
        eval_path(v)
        return True
    except PathError:
        return False


def eval_path(v: str) -> int:
    """The index a decompiler path names, with every array's size slot counted."""
    v = v.strip()
    if not v:
        return 0
    total = 0
    # Split on the dots outside brackets.
    depth, start, parts = 0, 0, []
    for i, ch in enumerate(v):
        if ch == "[":
            depth += 1
        elif ch == "]":
            depth -= 1
        elif ch == "." and depth == 0:
            parts.append(v[start:i])
            start = i + 1
    parts.append(v[start:])
    for part in parts:
        head = part.split("[", 1)[0].strip()
        m = _TOKEN.match(head)
        if not m:
            raise PathError(f"not a path element: {part!r} in {v!r}")
        total += int(m.group(1))
        rest = part[len(part.split("[", 1)[0]):]
        pos = 0
        while pos < len(rest):
            im = _INDEX.match(rest, pos)
            if not im:
                raise PathError(f"bad index in {v!r}")
            total += 1
            index, stride = im.group(1), im.group(2)
            if re.fullmatch(r"-?\d+", index):
                # The decompiler leaves the stride comment out when an element is one slot.
                total += int(index) * int(stride or 1)
            pos = im.end()
    return total


def old_eval(v: str) -> int:
    """How Xenvious read a path before (OffsetLoader.GetGlobalNumbers(PrepareGlobal(v))):
    brackets dropped, the digits of every dot part summed. Kept for the conversion check."""
    total = 0
    for s in re.sub(r"\[.*?\]", "", v).split("."):
        digits = re.sub(r"[^0-9]", "", s)
        if digits:
            total += int(digits)
    return total


def read_integer(v: str, default: int = 0) -> int:
    """IniText.ReadInteger: 0x... hex with an optional sign, else the leading integer."""
    if v is None or not v.strip():
        return default
    v = v.strip()
    neg = v.startswith("-")
    h = v[1:] if v[:1] in "+-" else v
    if h.lower().startswith("0x"):
        try:
            r = int(h[2:], 16)
        except ValueError:
            return default
        return -r if neg else r
    m = re.match(r"[+-]?\d+", v)
    return int(m.group(0)) if m else default


_HEX = re.compile(r"^[+-]?0x[0-9a-f]+$", re.I)


def is_number(v: str) -> bool:
    """A value gen.py stores as a number: a path, a plain integer or 0x... hex."""
    return bool(_HEX.match(v.strip())) or is_path(v)


def number(v: str) -> int:
    return read_integer(v) if _HEX.match(v.strip()) else eval_path(v)


def member_name(key: str) -> str:
    """The C# member for an ini key: OFFSET_actor_model -> actor_model."""
    name = re.sub(r"^offset_", "", key, flags=re.I)
    name = re.sub(r"[^A-Za-z0-9_]", "_", name)
    return "_" + name if name[:1].isdigit() else name


def section_kinds(inis: list, section: str) -> "OrderedDict[str, tuple]":
    """member -> ("long" | "string", ini key spelling) over all editions' files: a key is a
    number when every file that has it gives a number."""
    out: "OrderedDict[str, tuple]" = OrderedDict()
    seen: dict = {}
    for ini in inis:
        for (sec, low), v in ini.items():
            if sec != section:
                continue
            name = ini.names[(sec, low)]  # type: ignore[attr-defined]
            m = member_name(name)
            prev = seen.get(m.lower())
            numeric = is_number(v) or v.strip() == ""
            if prev is None:
                seen[m.lower()] = m
                out[m] = ("long" if numeric else "string", name)
            elif not numeric:
                out[prev] = ("string", out[prev][1])
    return out
