#!/usr/bin/env python3
"""Validate Docs/ASSET_MANIFEST.md against the strict register schema.

Usage: python3 Art/pipeline/validate_asset_manifest.py [repo-root]
Prints one line per violation, then RESULT: PASS or RESULT: FAIL.
Exit code 0 on PASS, 1 on FAIL. Stdlib only.
"""
import os
import re
import sys

TYPES = {"sfx", "music", "ambience", "vo", "texture", "model", "material", "font", "decal", "other"}
ORIGINS = {"in-house", "procedural", "third-party"}
LICENSES = {"CC0-1.0", "CC-BY-3.0", "CC-BY-4.0", "OFL-1.1", "Apache-2.0",
            "Pixabay-Content", "Sonniss-GDC", "Fab-Standard", "other"}
STATUSES = {"candidate", "downloaded", "imported", "wired", "replaced"}
ATTRIBUTION = {"CC-BY-3.0", "CC-BY-4.0"}
COLS = ["id", "family", "path", "type", "origin", "source_url", "license", "author",
        "credit_line", "imported_at", "status", "replacement", "wired_to",
        "platform_overrides", "spec", "human_check", "notes"]
ID_RE = re.compile(r"^(SFX|MUS|AMB|VO|Mat|SM|T|FONT)_[A-Za-z0-9_]+$")
DATE_RE = re.compile(r"^\d{4}-\d{2}-\d{2}$")


def split_row(line):
    # markdown table row -> cells, honoring \| escapes
    body = line.strip().strip("|")
    cells, cur, esc = [], [], False
    for ch in body:
        if esc:
            cur.append(ch)
            esc = False
        elif ch == "\\":
            esc = True
        elif ch == "|":
            cells.append("".join(cur).strip())
            cur = []
        else:
            cur.append(ch)
    cells.append("".join(cur).strip())
    return cells


def main():
    root = sys.argv[1] if len(sys.argv) > 1 else os.getcwd()
    path = os.path.join(root, "Docs", "ASSET_MANIFEST.md")
    errors = []
    try:
        lines = open(path, encoding="utf-8").read().splitlines()
    except OSError as e:
        print("ERROR: cannot read %s: %s" % (path, e))
        print("RESULT: FAIL")
        return 1

    for field in ("register_version:", "generated_from_head:", "updated:"):
        if not any(l.startswith(field) for l in lines[:12]):
            errors.append("header missing %s" % field)

    table = [l for l in lines if l.strip().startswith("|")]
    if len(table) < 2:
        errors.append("no table found")
        table = []
    header = split_row(table[0]) if table else []
    if header != COLS:
        errors.append("column header mismatch: got %s" % header)

    seen = set()
    for ln, row in enumerate(table[2:], start=1):
        cells = split_row(row)
        if len(cells) != len(COLS):
            errors.append("row %d: %d cells, expected %d" % (ln, len(cells), len(COLS)))
            continue
        d = dict(zip(COLS, cells))
        for k, v in d.items():
            if v == "":
                errors.append("row %d (%s): empty cell %s" % (ln, d["id"], k))
        if not ID_RE.match(d["id"]):
            errors.append("row %d: bad id %r" % (ln, d["id"]))
        if not ID_RE.match(d["family"]):
            errors.append("row %d (%s): bad family %r" % (ln, d["id"], d["family"]))
        else:
            m = re.match(r"^(.*)_\d+$", d["id"])
            want = m.group(1) if m else d["id"]
            if d["family"] != want:
                errors.append("row %d (%s): family %r should be %r" % (ln, d["id"], d["family"], want))
        if d["id"] in seen:
            errors.append("row %d: duplicate id %s" % (ln, d["id"]))
        seen.add(d["id"])
        if d["type"] not in TYPES:
            errors.append("row %d (%s): bad type %r" % (ln, d["id"], d["type"]))
        if d["origin"] not in ORIGINS:
            errors.append("row %d (%s): bad origin %r" % (ln, d["id"], d["origin"]))
        if d["license"] not in LICENSES:
            errors.append("row %d (%s): bad license %r" % (ln, d["id"], d["license"]))
        if d["status"] not in STATUSES:
            errors.append("row %d (%s): bad status %r" % (ln, d["id"], d["status"]))
        if d["license"] in ATTRIBUTION and d["credit_line"] == "-":
            errors.append("row %d (%s): %s requires a credit_line" % (ln, d["id"], d["license"]))
        if d["license"] == "other" and d["notes"] == "-":
            errors.append("row %d (%s): license 'other' requires notes" % (ln, d["id"]))
        if d["imported_at"] != "-" and not DATE_RE.match(d["imported_at"]):
            errors.append("row %d (%s): bad imported_at %r" % (ln, d["id"], d["imported_at"]))
        if d["status"] in ("imported", "wired"):
            if not os.path.isfile(os.path.join(root, d["path"])):
                errors.append("row %d (%s): path missing for status %s: %s"
                              % (ln, d["id"], d["status"], d["path"]))
        if d["status"] == "replaced":
            if d["replacement"] == "-":
                errors.append("row %d (%s): replaced rows must name a replacement" % (ln, d["id"]))
        elif d["replacement"] != "-":
            errors.append("row %d (%s): replacement set but status is %s" % (ln, d["id"], d["status"]))
        if d["wired_to"] != "-" and d["status"] != "wired":
            errors.append("row %d (%s): wired_to set but status is %s" % (ln, d["id"], d["status"]))

    for e in errors:
        print(e)
    print("checked %d asset rows" % (len(table) - 2 if table else 0))
    print("RESULT: " + ("FAIL" if errors else "PASS"))
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
