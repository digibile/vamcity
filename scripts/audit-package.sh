#!/usr/bin/env bash
set -uo pipefail

ROOT="${1:-.}"
cd "$ROOT" || exit 2

echo "=== VamCity Linux package audit ==="
echo "Root: $(pwd)"

source_count=$(find . -type f \
  \( -name '*.sln' -o -name '*.csproj' -o -name '*.cs' \
     -o -name '*.razor' -o -name '*.cshtml' \) \
  -not -path './.git/*' 2>/dev/null | wc -l)

echo "Source/solution files: $source_count"
if [ "$source_count" -eq 0 ]; then
  echo "BLOCKER: source files are missing; this appears to be a published package."
fi

runtimeconfig=$(find . -maxdepth 2 -type f \
  -name 'LoanSystemAvizhe.Site.runtimeconfig.json' -print -quit)

if [ -n "$runtimeconfig" ]; then
  python3 - "$runtimeconfig" <<'PY'
import json, sys
try:
    with open(sys.argv[1], encoding="utf-8") as f:
        data = json.load(f)
    options = data.get("runtimeOptions", {})
    print("Target framework:", options.get("tfm", "unknown"))
    framework = options.get("framework")
    if framework:
        print("Framework:", framework.get("name"), framework.get("version"))
    for item in options.get("frameworks", []):
        print("Framework:", item.get("name"), item.get("version"))
except Exception as exc:
    print("ERROR: invalid runtime config:", exc)
    sys.exit(1)
PY
else
  echo "Entry runtime config not found."
fi

echo
echo "Potentially sensitive files (review; contents are not printed):"
find . -type f \
  \( -iname 'appsettings*.json' -o -iname '*.pfx' \
     -o -iname '*.p12' -o -iname '*.key' -o -name '.env' \) \
  -not -path './.git/*' -print 2>/dev/null || true

echo
echo "Linux native libraries:"
find . -type f -path '*/runtimes/linux-*/native/*.so' \
  -print 2>/dev/null | head -50 || true

if [ "$source_count" -eq 0 ]; then
  echo "RESULT: source-based Linux migration is blocked until source is recovered or reconstructed."
  exit 1
fi

echo "RESULT: source files exist; a Linux build and integration tests are still required."
