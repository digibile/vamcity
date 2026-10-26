#!/bin/bash
# نصب پشت‌سرهم همه‌ی ماژول‌های بک‌اند. هر مرحله بکاپ می‌گیرد و با خطا متوقف می‌شود.
# استفاده: bash install-all.sh [مسیر_پروژه]
set -e
ROOT="${1:-/home/ghestine/domains/ghestine.ir/Ghestine}"; H="$(cd "$(dirname "$0")" && pwd)"
echo "### 1/5 چندمستاجری"; bash "$H/backend/tenancy/install.sh" "$ROOT"
echo "### 2/5 هسته (طرح‌ها، کارت مجازی، درگاه، مدارک)"; bash "$H/backend/core/install.sh" "$ROOT"
echo "### 3/5 امنیت ورود (کپچا، OTP)"; bash "$H/backend/security/install.sh" "$ROOT"
echo "### 4/5 عملیات (اعلام واریز، طرف‌حساب‌ها، بازاریاب‌ها، تنظیمات اپ)"; bash "$H/backend/ops/install.sh" "$ROOT"
echo "### 5/5 سازمانی (حسابرسی، دفتر کل، health، پشتیبان خودکار)"; bash "$H/backend/enterprise/install.sh" "$ROOT"
echo "همه‌ی ماژول‌ها ساخته شدند. حالا یک مایگریشن واحد بزنید (بعد از بکاپ دیتابیس):"
echo "  dotnet ef migrations add AddDigibilePayCore -p src/Ghestine.Infrastructure -s src/Ghestine.API"
echo "  dotnet ef database update -p src/Ghestine.Infrastructure -s src/Ghestine.API"
