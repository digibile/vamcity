#!/bin/bash
# نصب ماژول عملیات: اعلام واریزها، طرف‌حساب‌ها، بازاریاب‌ها، تنظیمات اپلیکیشن
set -e
ROOT="${1:-/home/ghestine/domains/ghestine.ir/Ghestine}"
HERE="$(cd "$(dirname "$0")" && pwd)"; SRC="$ROOT/src"
[ -d "$SRC" ] || { echo "پوشه src پیدا نشد: $SRC"; exit 1; }
B="$ROOT/.ops-backup-$(date +%Y%m%d%H%M%S)"; mkdir -p "$B"
DBF=$(grep -rl "class AppDbContext" "$SRC" --include=*.cs | head -1); [ -n "$DBF" ] || { echo "AppDbContext پیدا نشد"; exit 1; }
cp "$DBF" "$B/AppDbContext.cs.bak"; echo "بکاپ: $B"
BE=$(grep -rl "class BaseEntity" "$SRC" --include=*.cs | head -1); [ -n "$BE" ] || { echo "کلاس BaseEntity پیدا نشد."; exit 1; }
grep -q "Guid Id" "$BE" || { echo "BaseEntity کلید Guid ندارد ($BE)."; exit 1; }
BENS=$(grep -h "^namespace" "$BE" | head -1 | sed 's/namespace //; s/;//; s/\r//')
DBNS=$(grep -h "^namespace" "$DBF" | head -1 | sed 's/namespace //; s/;//; s/\r//')
grep -rq "class TenantMiddleware" "$SRC" --include=*.cs || { echo "ابتدا ماژول چندمستاجری (tenancy) را نصب کنید."; exit 1; }
cp "$HERE/files/OpsModule.cs" "$SRC/Ghestine.Domain/Entities/OpsModule.cs"
sed -i "s/^using Ghestine.Domain.Common;/using $BENS;/" "$SRC/Ghestine.Domain/Entities/OpsModule.cs"
mkdir -p "$SRC/Ghestine.API/Ops"; cp "$HERE/files/OpsControllers.cs" "$SRC/Ghestine.API/Ops/OpsControllers.cs"
sed -i "s/__DBNS__/$DBNS/" "$SRC/Ghestine.API/Ops/OpsControllers.cs"
addset(){ grep -q "DbSet<Ghestine.Domain.Entities.$1>" "$DBF" && return 0
  grep -q "protected override void OnModelCreating" "$DBF" || { echo "OnModelCreating پیدا نشد؛ DbSet ها را دستی اضافه کنید"; exit 1; }
  sed -i "0,/protected override void OnModelCreating/s//public DbSet<Ghestine.Domain.Entities.$1> $2 => Set<Ghestine.Domain.Entities.$1>();\n\n    protected override void OnModelCreating/" "$DBF"; }
addset PaymentDeclaration PaymentDeclarations; addset CounterParty CounterParties; addset Marketer Marketers; addset AppSetting AppSettings; addset InquiryRequest InquiryRequests; addset PurchaseOrder PurchaseOrders
for n in PaymentDeclaration CounterParty Marketer AppSetting InquiryRequest PurchaseOrder; do grep -q "DbSet<Ghestine.Domain.Entities.$n>" "$DBF" || { echo "درج DbSet $n انجام نشد؛ دستی اضافه کنید. بکاپ: $B"; exit 1; }; done
cd "$ROOT"
if dotnet build 2>&1 | tee /tmp/ops-build.log | tail -15 | grep -q "Build succeeded"; then echo "ساخت موفق."; else
  echo "خطای build؛ ۱۵ خط آخر را بفرستید. برگرداندن: cp $B/AppDbContext.cs.bak $DBF"; exit 1; fi
cat <<'MSG'

=== مراحل بعد ===
مایگریشن (بعد از بکاپ): dotnet ef migrations add AddOps -p src/Ghestine.Infrastructure -s src/Ghestine.API ; dotnet ef database update -p src/Ghestine.Infrastructure -s src/Ghestine.API
برای پشتیبان‌گیری از پنل: در appsettings بخش "Backup": { "Directory": "/var/opt/mssql/backups" } را بگذارید (پوشه باید برای سرویس SQL Server قابل نوشتن و برای برنامه قابل خواندن باشد).
سپس deploy.sh. تست: curl https://ghestine.ir/api/v1/app/config
توجه: تأیید «اعلام واریز» فقط وضعیت آن را عوض می‌کند؛ ثبت در اقساط وام بعد از دیدن ساختار وام انجام می‌شود.
MSG
