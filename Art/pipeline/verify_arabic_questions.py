# -*- coding: utf-8 -*-
"""Standalone verifier for the Arabic question fields (LIGHT SHADOW task P4).

Checks, without Unity:
  1. every Q_*.asset still starts byte-identical to its HEAD (git) version -
     the Arabic fill was a pure append;
  2. the appended block is exactly questionAr / optionsAr(4) / explanationAr;
  3. questionAr and explanationAr are non-empty, all four optionsAr non-empty
     (mirrors QuestionData.HasArabic) and explanationAr also non-empty;
  4. factSource still NEEDS-FACT-CHECK, exactly once per file;
  5. the file parses as YAML (tag-tolerant loader) and the Arabic fields
     round-trip to non-empty strings.

Usage:  python -I verify_arabic_questions.py
"""

import glob
import io
import re
import subprocess
import sys

import yaml

FACT_SOURCE = "NEEDS-FACT-CHECK"


class UnityLoader(yaml.SafeLoader):
    pass


UnityLoader.add_multi_constructor(
    "tag:unity3d.com,2011:", lambda l, sfx, node: l.construct_mapping(node, deep=True))


def non_ws(s):
    return isinstance(s, str) and bool(s.strip())


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    files = sorted(glob.glob("Assets/_Project/Data/Questions/Q_*.asset"))
    if len(files) != 60:
        print("FAIL: expected 60 question assets, found " + str(len(files)))
        sys.exit(1)

    problems = []
    for p in files:
        posix = p.replace("\\", "/")
        new = io.open(p, encoding="utf-8", newline="").read()
        r = subprocess.run(["git", "show", "HEAD:" + posix], capture_output=True)
        if r.returncode != 0:
            problems.append(posix + ": not committed at HEAD (expected for the lead's run)")
            continue
        old = r.stdout.decode("utf-8")
        if not new.startswith(old):
            problems.append(posix + ": pre-existing bytes were modified (append-only violated)")
            continue

        for field in ("questionAr", "explanationAr"):
            m = re.search(r"^  " + field + r": (.+)$", new, re.M)
            if not m:
                problems.append(posix + ": missing " + field)
            elif m.group(1).strip() in ("''", '""'):
                problems.append(posix + ": empty " + field)
        # Dash lines: 4 EN options + 4 AR options per question asset.
        dash_count = len(re.findall(r"^  - ", new, re.M))
        if dash_count != 8:
            problems.append(posix + ": expected 8 option dash lines (4 EN + 4 AR), got "
                            + str(dash_count))
        if new.count("factSource: " + FACT_SOURCE) != 1:
            problems.append(posix + ": factSource marker count != 1")

        doc = yaml.load(new, Loader=UnityLoader)["MonoBehaviour"]
        if doc["m_Name"] != posix.rsplit("/", 1)[1][:-6]:
            problems.append(posix + ": m_Name mismatch")
        if not non_ws(doc.get("questionAr")) or not non_ws(doc.get("explanationAr")):
            problems.append(posix + ": Arabic text fields empty after YAML parse")
        ar = doc.get("optionsAr") or []
        if len(ar) != 4 or not all(non_ws(str(x)) for x in ar):
            problems.append(posix + ": optionsAr not 4 non-empty entries")
        if doc.get("factSource") != FACT_SOURCE:
            problems.append(posix + ": factSource changed")

    print("Verified " + str(len(files)) + " question assets.")
    if problems:
        print("PROBLEMS:")
        for p in problems:
            print("  - " + p)
        sys.exit(1)
    print("RESULT: PASS - append-only Arabic fill intact, all Arabic fields "
          "non-empty, factSource untouched.")


if __name__ == "__main__":
    main()
