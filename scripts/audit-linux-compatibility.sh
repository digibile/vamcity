#!/usr/bin/env bash
set -euo pipefail
ROOT="${1:-.}"
cd "$ROOT"
REPORT="${2:-linux-compatibility-audit.md}"
: > "$REPORT"
say() { printf '%s\n' "$*" | tee -a "$REPORT"; }

say "# VamCity Linux compatibility audit"
say
say "Generated: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
say
say "## Source recovery"
CS_COUNT="$(find recovered-source -type f -name '*.cs' 2>/dev/null | wc -l | tr -d ' ')"
PROJECT_COUNT="$(find recovered-source -type f -name '*.csproj' 2>/dev/null | wc -l | tr -d ' ')"
say "- C# files: $CS_COUNT"
say "- project files: $PROJECT_COUNT"
for required in \
  recovered-source/LoanSystemAvizhe.Site/LoanSystem.Site/Program.cs \
  recovered-source/LoanSystemAvizhe.Site/LoanSystem.Site/Startup.cs \
  recovered-source/LoanSystemAvizhe.Data/LoanSystem.Data/AppDbContext.cs; do
  if [[ -s "$required" ]]; then say "- PRESENT: $required"; else say "- MISSING: $required"; echo "Required source missing: $required" >&2; exit 1; fi
done
say
say "## Database/background-job blockers"
for pattern in 'UseSqlServer' 'Microsoft.EntityFrameworkCore.SqlServer' 'Hangfire.SqlServer' 'SqlConnection' 'SqlCommand'; do
  count="$(grep -RIl --include='*.cs' --include='*.csproj' "$pattern" recovered-source 2>/dev/null | wc -l | tr -d ' ')"
  say "- $pattern appears in $count source/project files"
done
say
say "## Platform-sensitive dependencies"
for pattern in 'System.Drawing' 'DinkToPdf' 'libwkhtmltox' 'Environment.SpecialFolder' 'File\.Open' 'Directory\.CreateDirectory' 'Path\.Combine'; do
  count="$(grep -RIlE --include='*.cs' --include='*.csproj' "$pattern" recovered-source 2>/dev/null | wc -l | tr -d ' ')"
  say "- $pattern appears in $count source/project files"
done
WIN_NATIVE="$(find . -type f \( -iname '*.exe' -o -iname '*win-x64*.dll' -o -iname 'libwkhtmltox.dll' -o -iname 'sni.dll' \) -not -path './.git/*' | wc -l | tr -d ' ')"
LINUX_NATIVE="$(find . -type f -path '*/runtimes/linux-*/native/*.so' -not -path './.git/*' | wc -l | tr -d ' ')"
say "- Windows executable/native candidates in repository: $WIN_NATIVE"
say "- Linux native shared libraries in repository: $LINUX_NATIVE"
say
say "## SPA route and API inventory"
BUNDLE="$(find wwwroot/ui/js -maxdepth 1 -type f -name 'app.*.js' -print -quit 2>/dev/null || true)"
if [[ -n "$BUNDLE" ]]; then
  python3 - "$BUNDLE" >> "$REPORT" <<'PY'
import re, sys
from pathlib import Path
text = Path(sys.argv[1]).read_text(encoding="utf-8", errors="replace")
routes = sorted(set(re.findall(r'["\x27]((?:/)?(?:admin|superadmin|agent|member|sponsor|cl|login|register|plans|plan|ourplans|pop)(?:/[^"\x27\\s]*)?)["\x27]', text)))
apis = sorted(set(re.findall(r'["\x27]((?:admin|superadmin|agent|member|sponsor|account|loan|userloan|userLoan|Loan)/[A-Za-z0-9_./{}:$-]+)["\x27]', text)))
print(f"- Bundle: {sys.argv[1]}")
print(f"- Extracted route-like strings: {len(routes)}")
for item in routes[:250]: print(f"  - Route candidate: {item}")
print(f"- Extracted API-like strings: {len(apis)}")
for item in apis[:250]: print(f"  - API candidate: {item}")
PY
else
  say "- No compiled SPA app bundle found under wwwroot/ui/js"
fi
say
say "## Interpretation"
say "- This inventory is not approval for production deployment."
say "- SQL Server/Hangfire findings must be resolved or explicitly retained by an approved architecture decision before claiming PostgreSQL migration."
say "- HTTP and browser tests must run separately against isolated staging with test accounts."
say "AUDIT_REPORT=$REPORT"
