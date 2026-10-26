#!/bin/bash
# بررسی فقط‌خواندنی ساختار پروژه برای طرح بتا. هیچ فایلی تغییر نمی‌کند و مقدار رمزها چاپ نمی‌شود.
# استفاده: bash inspect.sh [مسیر_backend]
B="${1:-/home/digibile/domains/digibile.ir/negar-azin-fadak/backend}"
UI="${2:-/home/digibile/domains/digibile.ir/ui/negar-azin-fadak}"
O="$HOME/beta-inspect-$(date +%Y%m%d%H%M).txt"
{
echo "## مسیر: $B"; ls "$B" 2>&1 | head -40
echo; echo "## پروژه‌ها و پکیج‌ها"; find "$B" -name "*.csproj" -not -path "*/bin/*" -not -path "*/obj/*" | while read f; do echo "-- $f"; grep -E "PackageReference|TargetFramework|ProjectReference" "$f" | sed 's/^ *//'; done
echo; echo "## کنترلرها"; grep -rl "ControllerBase\|: Controller" "$B" --include=*.cs 2>/dev/null | grep -v "/obj/" | head -80
echo; echo "## مسیرهای API (Route/Http)"; grep -rhoE "\[(Route|HttpGet|HttpPost|HttpPut|HttpDelete)\([^]]*\)\]" "$B" --include=*.cs 2>/dev/null | sort -u | head -250
echo; echo "## هرچه به Benefit/بتا/رفاه/Refah مربوط است (نام فایل و خط)"
grep -rniE "benefit|refah|رفاه|بتا|leasing|لیزینگ" "$B" "$UI" --include=*.cs --include=*.ts --include=*.tsx --include=*.js --include=*.json --include=*.md --include=*.yaml --include=*.yml -l 2>/dev/null | grep -vE "node_modules|/bin/|/obj/|\.next" | head -60
echo; echo "## DbContext و موجودیت‌ها"; grep -rl "DbContext" "$B" --include=*.cs | grep -v "/obj/" | head; grep -rhoE "DbSet<[A-Za-z0-9_]+>" "$B" --include=*.cs | sort -u
echo; echo "## مایگریشن‌ها (۴۰ مورد آخر)"; find "$B" -path "*Migrations*" -name "*.cs" -not -name "*Designer*" -not -name "*Snapshot*" 2>/dev/null | sort | tail -40
echo; echo "## احراز هویت و نقش‌ها"; grep -rnE "AddAuthentication|AddJwtBearer|AddPolicy|\[Authorize\(|IsInRole|RequireRole" "$B" --include=*.cs 2>/dev/null | grep -v "/obj/" | cut -c1-200 | head -40
echo; echo "## ذخیره‌سازی فایل و لاگ/ممیزی"; grep -rnEi "IFormFile|wwwroot|UploadPath|AuditLog|AuditEvent|IAudit" "$B" --include=*.cs 2>/dev/null | grep -v "/obj/" | cut -c1-180 | head -40
echo; echo "## کلیدهای appsettings (فقط نام، بدون مقدار)"; for f in "$B"/appsettings*.json "$B"/*/appsettings*.json; do [ -f "$f" ] && { echo "-- $f"; python3 - "$f" <<'PY'
import json,sys
def walk(d,p=''):
    for k,v in d.items():
        if isinstance(v,dict): walk(v,p+k+'.')
        else: print(p+k)
try: walk(json.load(open(sys.argv[1])))
except Exception as e: print('خواندن نشد',e)
PY
}; done
echo; echo "## Swagger / OpenAPI"; find "$B" "$UI" -maxdepth 4 \( -iname "swagger*.json" -o -iname "openapi*.json" -o -iname "*.postman_collection.json" \) -not -path "*/node_modules/*" 2>/dev/null
echo; echo "## منوی اصلی (Master Menu)"; grep -rln "menu/master\|MasterMenu" "$B" "$UI" --include=*.cs --include=*.ts --include=*.tsx 2>/dev/null | grep -v node_modules | head
echo; echo "## git"; (cd "$B" && git log --oneline -15 2>&1 | head -15)
echo; echo "## خدمات و دیتابیس"; systemctl list-units --type=service --no-pager 2>/dev/null | grep -iE "negar|fadak|digibile|postgres" ; psql --version 2>&1 | head -1
} > "$O" 2>&1
echo "خروجی ذخیره شد: $O"; echo "محتوای این فایل را برای من بفرستید (رمز و توکن در آن چاپ نمی‌شود، ولی یک بار مرورش کنید)."
