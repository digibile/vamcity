#!/bin/bash
# نصب لایه‌ی سازمانی: لاگ حسابرسی ضدتغییر، دفتر کل دوطرفه، health check، پشتیبان خودکار
set -e
ROOT="${1:-/home/ghestine/domains/ghestine.ir/Ghestine}"
HERE="$(cd "$(dirname "$0")" && pwd)"; SRC="$ROOT/src"
[ -d "$SRC" ] || { echo "پوشه src پیدا نشد: $SRC"; exit 1; }
B="$ROOT/.ent-backup-$(date +%Y%m%d%H%M%S)"; mkdir -p "$B"
DBF=$(grep -rl "class AppDbContext" "$SRC" --include=*.cs | head -1); [ -n "$DBF" ] || { echo "AppDbContext پیدا نشد"; exit 1; }
PG="$SRC/Ghestine.API/Program.cs"; cp "$PG" "$B/Program.cs.bak"; cp "$DBF" "$B/AppDbContext.cs.bak"; echo "بکاپ: $B"
BE=$(grep -rl "class BaseEntity" "$SRC" --include=*.cs | head -1); [ -n "$BE" ] || { echo "کلاس BaseEntity پیدا نشد."; exit 1; }
grep -q "Guid Id" "$BE" || { echo "BaseEntity کلید Guid ندارد ($BE)."; exit 1; }
BENS=$(grep -h "^namespace" "$BE" | head -1 | sed 's/namespace //; s/;//; s/\r//')
DBNS=$(grep -h "^namespace" "$DBF" | head -1 | sed 's/namespace //; s/;//; s/\r//')
grep -rq "class TenantMiddleware" "$SRC" --include=*.cs || { echo "ابتدا ماژول چندمستاجری (tenancy) را نصب کنید."; exit 1; }
cp "$HERE/files/EntModule.cs" "$SRC/Ghestine.Domain/Entities/EntModule.cs"
sed -i "s/^using Ghestine.Domain.Common;/using $BENS;/" "$SRC/Ghestine.Domain/Entities/EntModule.cs"
mkdir -p "$SRC/Ghestine.API/Enterprise"; cp "$HERE/files/EntControllers.cs" "$SRC/Ghestine.API/Enterprise/EntControllers.cs"
sed -i "s/__DBNS__/$DBNS/" "$SRC/Ghestine.API/Enterprise/EntControllers.cs"
addset(){ grep -q "DbSet<Ghestine.Domain.Entities.$1>" "$DBF" && return 0
  grep -q "protected override void OnModelCreating" "$DBF" || { echo "OnModelCreating پیدا نشد؛ DbSet ها را دستی اضافه کنید"; exit 1; }
  sed -i "0,/protected override void OnModelCreating/s//public DbSet<Ghestine.Domain.Entities.$1> $2 => Set<Ghestine.Domain.Entities.$1>();\n\n    protected override void OnModelCreating/" "$DBF"; }
addset AuditEntry AuditEntries; addset LedgerAccount LedgerAccounts; addset JournalEntry JournalEntries; addset JournalLine JournalLines
for n in AuditEntry LedgerAccount JournalEntry JournalLine; do grep -q "DbSet<Ghestine.Domain.Entities.$n>" "$DBF" || { echo "درج DbSet $n انجام نشد. بکاپ: $B"; exit 1; }; done
if ! grep -q "AuditFilter" "$PG"; then
  sed -i '0,/var app = builder.Build();/s//builder.Services.AddScoped<Ghestine.API.Enterprise.AuditFilter>();\nbuilder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(o => o.Filters.AddService<Ghestine.API.Enterprise.AuditFilter>());\nbuilder.Services.AddHostedService<Ghestine.API.Enterprise.DailyBackupService>();\n\nvar app = builder.Build();/' "$PG"
fi
grep -q "AuditFilter" "$PG" && grep -q "DailyBackupService" "$PG" || { echo "ثبت سرویس‌ها در Program.cs انجام نشد. برگرداندن: cp $B/Program.cs.bak $PG"; exit 1; }
cd "$ROOT"
if dotnet build 2>&1 | tee /tmp/ent-build.log | tail -15 | grep -q "Build succeeded"; then echo "ساخت موفق."; else
  echo "خطای build؛ ۱۵ خط آخر را بفرستید. برگرداندن: cp $B/AppDbContext.cs.bak $DBF ; cp $B/Program.cs.bak $PG"; exit 1; fi
cat <<'MSG'

=== مراحل بعد ===
۱) یک مایگریشن بزنید (بعد از بکاپ دیتابیس) و deploy کنید.
۲) برای جلوگیری از شماره‌ی سند تکراری، یک ایندکس یکتا روی JournalEntries(TenantId, Number) و روی AuditEntries(TenantId, Seq) در مایگریشن اضافه کنید:
     migrationBuilder.CreateIndex("UX_Journal_Number","JournalEntries",new[]{"TenantId","Number"},unique:true);
     migrationBuilder.CreateIndex("UX_Audit_Seq","AuditEntries",new[]{"TenantId","Seq"},unique:true);
۳) مانیتورینگ بیرونی را روی https://ghestine.ir/api/v1/health بگذارید (UptimeRobot رایگان کافی است).
۴) پشتیبان خودکار با "Backup": {"Directory": "...", "HourUtc": 23} روشن می‌شود؛ ۱۴ نسخه‌ی آخر می‌ماند. یک نسخه را هم بیرون از همین سرور نگه دارید.
MSG
