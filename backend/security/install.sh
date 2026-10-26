#!/bin/bash
# نصب ماژول امنیت ورود: کپچای سمت سرور، OTP با کاوه‌نگار، محدودکننده‌ی درخواست
# به سیستم احراز هویت موجود دست نمی‌زند. صدور توکن بعد از بررسی کد فعلی به IAuthTokenIssuer وصل می‌شود.
set -e
ROOT="${1:-/home/ghestine/domains/ghestine.ir/Ghestine}"
HERE="$(cd "$(dirname "$0")" && pwd)"; SRC="$ROOT/src"
[ -d "$SRC" ] || { echo "پوشه src پیدا نشد: $SRC"; exit 1; }
B="$ROOT/.sec-backup-$(date +%Y%m%d%H%M%S)"; mkdir -p "$B"
DBF=$(grep -rl "class AppDbContext" "$SRC" --include=*.cs | head -1); [ -n "$DBF" ] || { echo "AppDbContext پیدا نشد"; exit 1; }
grep -rq "interface IOcrProvider" "$SRC" --include=*.cs || { echo "ابتدا ماژول هسته (core) را نصب کنید؛ ثبت‌نام با عکس مدرک به آن وابسته است."; exit 1; }
cp "$DBF" "$B/AppDbContext.cs.bak"; cp "$SRC/Ghestine.API/Program.cs" "$B/Program.cs.bak"; echo "بکاپ: $B"
BE=$(grep -rl "class BaseEntity" "$SRC" --include=*.cs | head -1); [ -n "$BE" ] || { echo "کلاس BaseEntity پیدا نشد؛ موجودیت‌های جدید به آن وابسته‌اند. نتیجه را برای من بفرستید."; exit 1; }
grep -q "Guid Id" "$BE" || { echo "BaseEntity کلید Guid ندارد ($BE)؛ نتیجه را برای من بفرستید."; exit 1; }
BENS=$(grep -h "^namespace" "$BE" | head -1 | sed 's/namespace //; s/;//; s/\r//')
NEED(){ grep -q "$2" "$1" || { echo "درج خودکار در $1 انجام نشد (الگوی «$2»). فایل را باز کنید و طبق README دستی اضافه کنید. برای برگشت: بکاپ در $B"; exit 1; }; }
DBNS=$(grep -h "^namespace" "$DBF" | head -1 | sed 's/namespace //; s/;//; s/\r//')
cp "$HERE/files/SecurityEntities.cs" "$SRC/Ghestine.Domain/Entities/SecurityEntities.cs"
sed -i "s/^using Ghestine.Domain.Common;/using $BENS;/" "$SRC/Ghestine.Domain/Entities/SecurityEntities.cs"
mkdir -p "$SRC/Ghestine.API/Security"
for f in SecurityServices.cs SecurityController.cs AuthAdapter.cs; do cp "$HERE/files/$f" "$SRC/Ghestine.API/Security/$f"; done
for f in SecurityServices.cs AuthAdapter.cs; do sed -i "s/__DBNS__/$DBNS/" "$SRC/Ghestine.API/Security/$f"; done
if grep -q "OtpChallenges" "$DBF"; then echo "DbSet قبلاً هست"; else
  grep -q "protected override void OnModelCreating" "$DBF" || { echo "OnModelCreating پیدا نشد؛ DbSet را دستی اضافه کنید"; exit 1; }
  sed -i '0,/protected override void OnModelCreating/s//public DbSet<Ghestine.Domain.Entities.OtpChallenge> OtpChallenges => Set<Ghestine.Domain.Entities.OtpChallenge>();\n\n    protected override void OnModelCreating/' "$DBF"
fi
PG="$SRC/Ghestine.API/Program.cs"
if grep -q "ICaptchaService" "$PG"; then echo "سرویس‌ها قبلاً ثبت شده"; else
  sed -i '0,/var app = builder.Build();/s//builder.Services.AddMemoryCache();\nbuilder.Services.AddSingleton<Ghestine.API.Security.ICaptchaService, Ghestine.API.Security.CaptchaService>();\nbuilder.Services.AddSingleton<Ghestine.API.Security.IRateLimiter, Ghestine.API.Security.RateLimiter>();\nbuilder.Services.AddHttpClient<Ghestine.API.Security.ISmsSender, Ghestine.API.Security.KavenegarSender>();\nbuilder.Services.AddScoped<Ghestine.API.Security.IAuthTokenIssuer, Ghestine.API.Security.ReflectiveTokenIssuer>();\nbuilder.Services.AddScoped<Ghestine.API.Security.AuthAdapter>();\nbuilder.Services.AddScoped<Ghestine.API.Security.IOtpService, Ghestine.API.Security.OtpService>();\n\nvar app = builder.Build();/' "$PG"
fi
if grep -q "auth-doctor" "$PG"; then echo "ابزار خط فرمان قبلاً ثبت شده"; else
  sed -i '0,/var app = builder.Build();/s//var app = builder.Build();\nif (args.Contains("--auth-doctor") || args.Contains("--create-superadmin")) { await Ghestine.API.Security.AuthTools.RunAsync(app.Services, args); return; }/' "$PG"
fi
NEED "$DBF" OtpChallenges
NEED "$PG" ICaptchaService
NEED "$PG" auth-doctor
cd "$ROOT"
if dotnet build 2>&1 | tee /tmp/sec-build.log | tail -15 | grep -q "Build succeeded"; then echo "ساخت موفق."; else
  echo "خطای build؛ ۱۵ خط آخر بالا را بفرستید. برگرداندن:"; echo "  cp $B/AppDbContext.cs.bak $DBF ; cp $B/Program.cs.bak $PG"; exit 1; fi
cat <<'MSG'

=== مراحل بعد ===
1) کلید کاوه‌نگار را در فایل محرمانه‌ی سرور بگذارید (نه در گیت و نه در wwwroot):
   sudo mkdir -p /etc/ghestine && sudo nano /etc/ghestine/secrets.env
     KAVENEGAR_API_KEY=کلید_واقعی
     KAVENEGAR_SENDER=200053467
     OTP_EXPIRES_SECONDS=120
     OTP_RESEND_SECONDS=120
     OTP_PEPPER=<خروجی دستور: openssl rand -hex 32>   (الزامی؛ بدون آن سرویس امنیت بالا نمی‌آید)
   sudo chmod 600 /etc/ghestine/secrets.env
   در ghestine.service زیر [Service] اضافه کنید:  EnvironmentFile=/etc/ghestine/secrets.env
   sudo systemctl daemon-reload
2) بکاپ دیتابیس، سپس:
   dotnet ef migrations add AddOtpChallenge -p src/Ghestine.Infrastructure -s src/Ghestine.API
   dotnet ef database update -p src/Ghestine.Infrastructure -s src/Ghestine.API
3) /home/ghestine/deploy.sh
4) تست: curl https://ghestine.ir/api/v1/security/captcha
5) تشخیص ورود فعلی (بدون چاپ رمز): bash run-tool.sh --auth-doctor
6) ساخت سوپرادمین (اطلاعات را همان‌جا وارد می‌کنید): bash run-tool.sh --create-superadmin
نکته: اتصال به کاربران فعلی از روی مدل EF به‌صورت خودکار انجام می‌شود. اگر auth-doctor مبهم بودن یا ناسازگاری گزارش داد، خروجی را بفرستید.
MSG
