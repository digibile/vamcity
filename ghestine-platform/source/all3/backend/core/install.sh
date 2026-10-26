#!/bin/bash
# نصب ماژول هسته‌ی قسطینه: محصولات وام، کارت مجازی، درگاه داخلی (پرداخت با کارت)، زونکن مدارک
# استفاده: bash install.sh [مسیر_پروژه]
set -e
ROOT="${1:?مسیر واقعی پروژه را به‌عنوان آرگومان اول بدهید؛ مثال: bash "$0" /home/USER/domains/DOMAIN/PROJECT}"
HERE="$(cd "$(dirname "$0")" && pwd)"
SRC="$ROOT/src"
[ -d "$SRC" ] || { echo "پوشه src پیدا نشد: $SRC"; exit 1; }

echo "=== 0) بکاپ از فایل‌هایی که تغییر می‌کنند ==="
B="$ROOT/.core-backup-$(date +%Y%m%d%H%M%S)"; mkdir -p "$B"
DBF=$(grep -rl "class AppDbContext" "$SRC" --include=*.cs | head -1)
[ -n "$DBF" ] || { echo "AppDbContext پیدا نشد"; exit 1; }
cp "$DBF" "$B/AppDbContext.cs.bak"; cp "$SRC/Ghestine.API/Program.cs" "$B/Program.cs.bak"
echo "بکاپ: $B"

echo "=== 1) کپی فایل‌ها ==="
BE=$(grep -rl "class BaseEntity" "$SRC" --include=*.cs | head -1); [ -n "$BE" ] || { echo "کلاس BaseEntity پیدا نشد؛ موجودیت‌های جدید به آن وابسته‌اند. نتیجه را برای من بفرستید."; exit 1; }
grep -q "Guid Id" "$BE" || { echo "BaseEntity کلید Guid ندارد ($BE)؛ نتیجه را برای من بفرستید."; exit 1; }
BENS=$(grep -h "^namespace" "$BE" | head -1 | sed 's/namespace //; s/;//; s/\r//')
NEED(){ grep -q "$2" "$1" || { echo "درج خودکار در $1 انجام نشد (الگوی «$2»). فایل را باز کنید و طبق README دستی اضافه کنید. برای برگشت: بکاپ در $B"; exit 1; }; }
DBNS=$(grep -h "^namespace" "$DBF" | head -1 | sed 's/namespace //; s/;//; s/\r//')
echo "DbContext: $DBF  ($DBNS)"
cp "$HERE/files/CoreModule.cs" "$SRC/Ghestine.Domain/Entities/CoreModule.cs"
sed -i "s/^using Ghestine.Domain.Common;/using $BENS;/" "$SRC/Ghestine.Domain/Entities/CoreModule.cs"
mkdir -p "$SRC/Ghestine.API/Core"
for f in CoreServices.cs VirtualCardService.cs CoreControllers.cs; do
  cp "$HERE/files/$f" "$SRC/Ghestine.API/Core/$f"
  sed -i "s/__DBNS__/$DBNS/" "$SRC/Ghestine.API/Core/$f"
done

echo "=== 2) افزودن DbSet ها ==="
if grep -q "LoanProducts" "$DBF"; then echo "قبلاً اضافه شده، رد شد."; else
  grep -q "protected override void OnModelCreating" "$DBF" || { echo "OnModelCreating در DbContext نیست؛ DbSet ها را دستی اضافه کنید (README)"; exit 1; }
  sed -i '0,/protected override void OnModelCreating/s//public DbSet<Ghestine.Domain.Entities.LoanProduct> LoanProducts => Set<Ghestine.Domain.Entities.LoanProduct>();\n    public DbSet<Ghestine.Domain.Entities.VirtualCard> VirtualCards => Set<Ghestine.Domain.Entities.VirtualCard>();\n    public DbSet<Ghestine.Domain.Entities.VirtualCardTxn> VirtualCardTxns => Set<Ghestine.Domain.Entities.VirtualCardTxn>();\n    public DbSet<Ghestine.Domain.Entities.CustomerDocument> CustomerDocuments => Set<Ghestine.Domain.Entities.CustomerDocument>();\n\n    protected override void OnModelCreating/' "$DBF"
fi

echo "=== 3) ثبت سرویس‌ها در Program.cs ==="
PG="$SRC/Ghestine.API/Program.cs"
if grep -q "IVirtualCardService" "$PG"; then echo "قبلاً اضافه شده، رد شد."; else
  mkdir -p /home/ghestine/dpkeys; chmod 700 /home/ghestine/dpkeys
  grep -q "using Microsoft.AspNetCore.DataProtection;" "$PG" || sed -i '1i using Microsoft.AspNetCore.DataProtection;' "$PG"
  sed -i '0,/var app = builder.Build();/s//builder.Services.AddDataProtection().SetApplicationName("ghestine").PersistKeysToFileSystem(new DirectoryInfo("\/home\/ghestine\/dpkeys"));\nbuilder.Services.AddSingleton<Ghestine.API.Core.ICardCrypto, Ghestine.API.Core.CardCrypto>();\nbuilder.Services.AddScoped<Ghestine.API.Core.IVirtualCardService, Ghestine.API.Core.VirtualCardService>();\nbuilder.Services.AddScoped<Ghestine.API.Core.IOcrProvider, Ghestine.API.Core.NullOcrProvider>();\n\nvar app = builder.Build();/' "$PG"
fi

echo "=== 4) ساخت پروژه ==="
NEED "$DBF" LoanProducts
NEED "$PG" IVirtualCardService
cd "$ROOT"
if dotnet build 2>&1 | tee /tmp/core-build.log | tail -15 | grep -q "Build succeeded"; then
  echo "ساخت موفق."
else
  echo "خطای build. ۱۵ خط آخر بالا را برای من بفرستید. فایل‌های اصلی از بکاپ $B برمی‌گردند:"
  echo "  cp $B/AppDbContext.cs.bak $DBF ; cp $B/Program.cs.bak $PG"; exit 1
fi

cat <<'MSG'

=== مراحل بعد (به ترتیب) ===
1) بکاپ دیتابیس بگیرید.
2) در appsettings.json (روی سرور) این کلیدها را اضافه کنید:
   "Cards": { "Bin": "910100", "ValidMonths": 36, "HashPepper": "یک-رشته-تصادفی-بلند" },
   "Gateway": { "MerchantKeys": [ "pk_live_یک-کلید-تصادفی" ] },
   "Documents": { "Path": "/home/ghestine/private-docs" }
3) مایگریشن:
   dotnet ef migrations add AddCoreModule -p src/Ghestine.Infrastructure -s src/Ghestine.API
   dotnet ef database update -p src/Ghestine.Infrastructure -s src/Ghestine.API
4) /home/ghestine/deploy.sh
5) ساخت سه طرح نمونه (با توکن مدیر):
   curl -X POST https://ghestine.ir/api/v1/admin/loan-products/seed -H "Authorization: Bearer <TOKEN>"
6) تست عمومی: curl https://ghestine.ir/api/v1/loan-products
مهم: پوشه‌ی /home/ghestine/dpkeys (کلیدهای رمزنگاری کارت و مدارک) را حتماً بکاپ بگیرید؛ با از دست رفتنش اطلاعات کارت و عکس مدارک قابل بازیابی نیست.
MSG
