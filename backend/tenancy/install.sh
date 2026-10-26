#!/bin/bash
# نصب هسته‌ی چندمستاجری (تشخیص مستأجر از روی دامنه + برند + مدیریت دامنه‌ها)
set -e
ROOT="${1:-/home/ghestine/domains/ghestine.ir/Ghestine}"
HERE="$(cd "$(dirname "$0")" && pwd)"; SRC="$ROOT/src"
[ -d "$SRC" ] || { echo "پوشه src پیدا نشد: $SRC"; exit 1; }
B="$ROOT/.tenancy-backup-$(date +%Y%m%d%H%M%S)"; mkdir -p "$B"
DBF=$(grep -rl "class AppDbContext" "$SRC" --include=*.cs | head -1); [ -n "$DBF" ] || { echo "AppDbContext پیدا نشد"; exit 1; }
PG="$SRC/Ghestine.API/Program.cs"; cp "$DBF" "$B/AppDbContext.cs.bak"; cp "$PG" "$B/Program.cs.bak"; echo "بکاپ: $B"
BE=$(grep -rl "class BaseEntity" "$SRC" --include=*.cs | head -1); [ -n "$BE" ] || { echo "کلاس BaseEntity پیدا نشد؛ موجودیت‌های جدید به آن وابسته‌اند. نتیجه را برای من بفرستید."; exit 1; }
grep -q "Guid Id" "$BE" || { echo "BaseEntity کلید Guid ندارد ($BE)؛ نتیجه را برای من بفرستید."; exit 1; }
BENS=$(grep -h "^namespace" "$BE" | head -1 | sed 's/namespace //; s/;//; s/\r//')
NEED(){ grep -q "$2" "$1" || { echo "درج خودکار در $1 انجام نشد (الگوی «$2»). فایل را باز کنید و طبق README دستی اضافه کنید. برای برگشت: بکاپ در $B"; exit 1; }; }
DBNS=$(grep -h "^namespace" "$DBF" | head -1 | sed 's/namespace //; s/;//; s/\r//')
cp "$HERE/files/TenancyEntities.cs" "$SRC/Ghestine.Domain/Entities/TenancyEntities.cs"
sed -i "s/^using Ghestine.Domain.Common;/using $BENS;/" "$SRC/Ghestine.Domain/Entities/TenancyEntities.cs"
mkdir -p "$SRC/Ghestine.API/Tenancy"; cp "$HERE/files/TenancyServices.cs" "$SRC/Ghestine.API/Tenancy/TenancyServices.cs"
sed -i "s/__DBNS__/$DBNS/" "$SRC/Ghestine.API/Tenancy/TenancyServices.cs"
if grep -q "TenantDomains" "$DBF"; then echo "DbSet قبلاً هست"; else
  grep -q "protected override void OnModelCreating" "$DBF" || { echo "OnModelCreating پیدا نشد؛ DbSet را دستی اضافه کنید"; exit 1; }
  sed -i '0,/protected override void OnModelCreating/s//public DbSet<Ghestine.Domain.Entities.Tenant> Tenants => Set<Ghestine.Domain.Entities.Tenant>();\n    public DbSet<Ghestine.Domain.Entities.TenantDomain> TenantDomains => Set<Ghestine.Domain.Entities.TenantDomain>();\n\n    protected override void OnModelCreating/' "$DBF"
fi
if grep -q "TenantMiddleware" "$PG"; then echo "میان‌افزار قبلاً ثبت شده"; else
  grep -q "AddMemoryCache" "$PG" || sed -i '0,/var app = builder.Build();/s//builder.Services.AddMemoryCache();\n\nvar app = builder.Build();/' "$PG"
  sed -i '0,/var app = builder.Build();/s//var app = builder.Build();\napp.UseMiddleware<Ghestine.API.Tenancy.TenantMiddleware>();/' "$PG"
fi
NEED "$DBF" TenantDomains
NEED "$PG" TenantMiddleware
cd "$ROOT"
if dotnet build 2>&1 | tee /tmp/ten-build.log | tail -15 | grep -q "Build succeeded"; then echo "ساخت موفق."; else
  echo "خطای build؛ ۱۵ خط آخر را بفرستید. برگرداندن: cp $B/AppDbContext.cs.bak $DBF ; cp $B/Program.cs.bak $PG"; exit 1; fi
cat <<'MSG'

=== مراحل بعد ===
1) بکاپ دیتابیس. 2) مایگریشن:
   dotnet ef migrations add AddTenancy -p src/Ghestine.Infrastructure -s src/Ghestine.API
   dotnet ef database update -p src/Ghestine.Infrastructure -s src/Ghestine.API
3) /home/ghestine/deploy.sh
4) تست: curl https://ghestine.ir/api/v1/tenant/config   (باید نام «دیجی‌بایل پی» را برگرداند)
5) برای هر دامنه‌ی جدید: DNS → IP سرور، افزودن دامنه در DirectAdmin/nginx (راهنما در README)، SSL، سپس افزودن در پنل (SuperAdmin).
MSG
