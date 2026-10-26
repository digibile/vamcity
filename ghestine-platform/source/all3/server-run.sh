#!/bin/bash
# اجرای مرحله‌به‌مرحله‌ی نصب و بررسی روی سرور. همه‌ی خروجی‌ها در یک گزارش جمع می‌شود (بدون رمز و کلید).
# استفاده:  bash server-run.sh [مسیر_پروژه]
ROOT="${1:?مسیر واقعی پروژه را به‌عنوان آرگومان اول بدهید؛ مثال: bash "$0" /home/USER/domains/DOMAIN/PROJECT}"; H="$(cd "$(dirname "$0")" && pwd)"; R="$H/report-$(date +%Y%m%d-%H%M%S).txt"
ask(){ read -r -p "$1 [y/N] " a; [ "$a" = y ] || [ "$a" = Y ]; }
log(){ echo -e "\n##### $1" | tee -a "$R"; }
run(){ "$@" 2>&1 | sed -E 's/(KEY|PEPPER|SECRET|PASSWORD|Password)=[^ ]+/\1=***/g' | tee -a "$R"; return "${PIPESTATUS[0]}"; }
echo "گزارش در $R ذخیره می‌شود. در پایان همین فایل را برای من بفرستید." | tee "$R"
[ -d "$ROOT/src" ] || { echo "پوشه‌ی پروژه پیدا نشد: $ROOT"; exit 1; }

log "۱) پیش‌نیازها"
command -v dotnet >/dev/null && run dotnet --version || { echo "dotnet پیدا نشد"; exit 1; }
if [ -f /etc/ghestine/secrets.env ]; then
  for k in OTP_PEPPER KAVENEGAR_API_KEY KAVENEGAR_SENDER; do grep -q "^$k=." /etc/ghestine/secrets.env && echo "$k: تنظیم شده" || echo "$k: خالی یا ناموجود"; done | tee -a "$R"
  [ "$(stat -c %a /etc/ghestine/secrets.env)" = 600 ] && echo "سطح دسترسی فایل: 600 ✔" | tee -a "$R" || echo "هشدار: chmod 600 /etc/ghestine/secrets.env" | tee -a "$R"
else echo "فایل /etc/ghestine/secrets.env هنوز ساخته نشده (طبق README)" | tee -a "$R"; fi
systemctl cat ghestine 2>/dev/null | grep -q "EnvironmentFile=/etc/ghestine/secrets.env" && echo "EnvironmentFile در سرویس: ✔" | tee -a "$R" || echo "هشدار: EnvironmentFile در ghestine.service ثبت نشده" | tee -a "$R"
grep -q '"Cards"' "$ROOT/src/Ghestine.API/appsettings.json" 2>/dev/null && echo "Cards در appsettings: ✔" | tee -a "$R" || echo "هشدار: بخش Cards (HashPepper) در appsettings نیست" | tee -a "$R"

log "۲) نصب ماژول‌ها"
ask "آیا از دیتابیس و پوشه‌ی پروژه بکاپ گرفته‌اید؟" || { echo "ابتدا بکاپ بگیرید و دوباره اجرا کنید."; exit 1; }
run bash "$H/install-all.sh" "$ROOT" || { echo "نصب خطا داد؛ گزارش را بفرستید."; exit 1; }

log "۳) مایگریشن"
if ask "مایگریشن ساخته و اعمال شود؟"; then
  cd "$ROOT" && run dotnet ef migrations add AddDigibilePayCore -p src/Ghestine.Infrastructure -s src/Ghestine.API && run dotnet ef database update -p src/Ghestine.Infrastructure -s src/Ghestine.API || echo "مایگریشن ناموفق؛ گزارش را بفرستید." | tee -a "$R"
fi

log "۴) استقرار"
if ask "deploy.sh اجرا شود (سرویس چند ثانیه قطع می‌شود)؟"; then run bash /home/ghestine/deploy.sh; sleep 3; fi

log "۵) تشخیص ورود (بدون رمز)"
run bash "$H/backend/security/run-tool.sh" --auth-doctor

log "۶) آزمون خودکار امنیت"
run bash "$H/backend/security/smoke-test.sh"

log "۷) سوپرادمین"
if ask "همین الان سوپرادمین ساخته شود؟ (مشخصات را همین‌جا وارد می‌کنید و در گزارش نمی‌آید)"; then bash "$H/backend/security/run-tool.sh" --create-superadmin; echo "سوپرادمین: اجرا شد؛ نتیجه‌ی بررسی روی صفحه چاپ شد. ورود واقعی را در مرورگر تست کنید." >> "$R"; fi

log "۸) تم قدیمی"
run bash "$H/backend/security/find-old-theme.sh" "$ROOT"

log "۹) پروژه‌ی طرح بتا (فقط خواندن)"
ask "inspect.sh برای پروژه‌ی نگار آذین فدک اجرا شود؟" && run bash "$H/beta/inspect.sh"

echo; echo "تمام شد. فایل گزارش: $R"
