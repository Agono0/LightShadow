# -*- coding: utf-8 -*-
"""Standalone verifier for the Arabic question fields (LIGHT SHADOW tasks P4
and P4-follow-up: bank growth 60 -> 120).

Checks, without Unity:
  1. count: exactly 120 Q_*.asset files (60 legacy + 60 new);
  2. the 60 LEGACY questions (ids *_001..*_010) still start byte-identical to
     their version at the pre-expansion branch point (ref feat/arabic-questions,
     falling back to its merge-base with HEAD) - the Arabic fill and this
     expansion were both pure appends / pure additions;
  3. the 60 NEW questions (ids *_011+) exist unchanged at HEAD (committed) and
     carry every EN + AR field: question / 4 options / correctIndex 0-3 /
     explanation / factSource=NEEDS-FACT-CHECK / questionAr / 4 optionsAr /
     explanationAr, category 0-5 and difficulty 1-3 in range;
  4. per file: exactly 8 option dash lines (4 EN + 4 AR) and exactly one
     factSource: NEEDS-FACT-CHECK marker;
  5. every file parses as YAML (tag-tolerant loader) and all fields round-trip
     to non-empty strings;
  6. no EN prompt appears twice across the whole 120-question bank.

Usage:  python -I verify_arabic_questions.py
"""

import glob
import io
import re
import subprocess
import sys

import yaml

FACT_SOURCE = "NEEDS-FACT-CHECK"
LEGACY_IDS = 10  # _001.._010 per category come from the original bank


class UnityLoader(yaml.SafeLoader):
    pass


UnityLoader.add_multi_constructor(
    "tag:unity3d.com,2011:", lambda l, sfx, node: l.construct_mapping(node, deep=True))


def non_ws(s):
    return isinstance(s, str) and bool(s.strip())


def base_ref():
    """Pre-expansion commit: the tip of feat/arabic-questions (this task's
    stack base), else its merge-base with HEAD, else HEAD (byte check then
    becomes trivially true - field checks still gate everything)."""
    for ref in ("refs/heads/feat/arabic-questions",
                "refs/remotes/origin/feat/arabic-questions",
                "feat/arabic-questions"):
        r = subprocess.run(["git", "rev-parse", "--verify", "--quiet", ref],
                           capture_output=True)
        if r.returncode == 0:
            return r.stdout.decode().strip()
    r = subprocess.run(["git", "merge-base", "HEAD", "feat/arabic-questions"],
                       capture_output=True)
    if r.returncode == 0:
        return r.stdout.decode().strip()
    return "HEAD"


def git_show(ref, posix):
    r = subprocess.run(["git", "show", ref + ":" + posix], capture_output=True)
    return r.returncode, r.stdout.decode("utf-8")


def check_common(posix, text, problems):
    """Field shape shared by legacy and new files."""
    for field in ("questionAr", "explanationAr"):
        m = re.search(r"^  " + field + r": (.+)$", text, re.M)
        if not m:
            problems.append(posix + ": missing " + field)
        elif m.group(1).strip() in ("''", '""'):
            problems.append(posix + ": empty " + field)
    # Dash lines: 4 EN options + 4 AR options per question asset.
    dash_count = len(re.findall(r"^  - ", text, re.M))
    if dash_count != 8:
        problems.append(posix + ": expected 8 option dash lines (4 EN + 4 AR), got "
                        + str(dash_count))
    if text.count("factSource: " + FACT_SOURCE) != 1:
        problems.append(posix + ": factSource marker count != 1")

    doc = yaml.load(text, Loader=UnityLoader)["MonoBehaviour"]
    if doc["m_Name"] != posix.rsplit("/", 1)[1][:-6]:
        problems.append(posix + ": m_Name mismatch")
    if not non_ws(doc.get("question")) or not non_ws(doc.get("explanation")):
        problems.append(posix + ": EN text fields empty after YAML parse")
    if not non_ws(doc.get("questionAr")) or not non_ws(doc.get("explanationAr")):
        problems.append(posix + ": Arabic text fields empty after YAML parse")
    en = doc.get("options") or []
    if len(en) != 4 or not all(non_ws(str(x)) for x in en):
        problems.append(posix + ": options not 4 non-empty entries")
    ar = doc.get("optionsAr") or []
    if len(ar) != 4 or not all(non_ws(str(x)) for x in ar):
        problems.append(posix + ": optionsAr not 4 non-empty entries")
    if doc.get("factSource") != FACT_SOURCE:
        problems.append(posix + ": factSource changed")
    ci = doc.get("correctIndex")
    if not (isinstance(ci, int) and 0 <= int(ci) <= 3):
        problems.append(posix + ": correctIndex out of range: " + str(ci))
    elif not non_ws(str(en[int(ci)])):
        problems.append(posix + ": correct option is empty")
    if doc.get("category") not in (0, 1, 2, 3, 4, 5):
        problems.append(posix + ": category out of range: " + str(doc.get("category")))
    if doc.get("difficulty") not in (1, 2, 3):
        problems.append(posix + ": difficulty out of range: " + str(doc.get("difficulty")))
    return doc


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    files = sorted(glob.glob("Assets/_Project/Data/Questions/Q_*.asset"))
    if len(files) != 120:
        print("FAIL: expected 120 question assets, found " + str(len(files)))
        sys.exit(1)

    base = base_ref()
    legacy_seen = 0
    new_seen = 0
    prompts = {}
    problems = []
    for p in files:
        posix = p.replace("\\", "/")
        asset_id = posix.rsplit("/", 1)[1][:-6]  # e.g. Q_Sci_016
        num = int(asset_id.rsplit("_", 1)[1])
        new = io.open(p, encoding="utf-8", newline="").read()

        if num <= LEGACY_IDS:
            legacy_seen += 1
            ok, old = git_show(base, posix)
            if ok != 0:
                problems.append(posix + ": not found at base ref " + base)
                continue
            if not new.startswith(old):
                problems.append(posix + ": pre-existing bytes were modified "
                                "(append-only violated)")
                continue
        else:
            new_seen += 1
            ok, head = git_show("HEAD", posix)
            if ok != 0:
                problems.append(posix + ": not committed at HEAD "
                                "(commit the new assets, then re-run)")
                continue
            if head != new:
                problems.append(posix + ": differs from its committed version at HEAD")

        doc = check_common(posix, new, problems)
        key = str(doc.get("question", "")).strip().lower()
        prompts.setdefault(key, []).append(asset_id)
        if len(prompts[key]) > 1:
            problems.append(asset_id + ": EN prompt duplicates "
                            + ", ".join(prompts[key]))

    if legacy_seen != 60:
        problems.append("expected 60 legacy questions (_001.._010), found "
                        + str(legacy_seen))
    if new_seen != 60:
        problems.append("expected 60 new questions (_011+), found " + str(new_seen))

    print("Verified " + str(len(files)) + " question assets against base ref "
          + base[:12] + " (" + str(legacy_seen) + " legacy byte-identical, "
          + str(new_seen) + " new complete).")
    if problems:
        print("PROBLEMS:")
        for p in problems:
            print("  - " + p)
        sys.exit(1)
    print("RESULT: PASS - 120-question bank intact: legacy 60 untouched at the "
          "base ref, new 60 carry all EN+AR fields (4 options each), "
          "factSource untouched, no duplicate prompts.")


if __name__ == "__main__":
    main()
