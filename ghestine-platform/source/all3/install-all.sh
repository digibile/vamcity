#!/bin/bash
# نصب پشت‌سرهم همه‌ی ماژول‌های بک‌اند. هر مرحله بکاپ می‌گیرد و با خطا متوقف می‌شود.
# استفاده: bash install-all.sh [مسیر_پروژه]
set -e
ROOT="${1:-/home/ghestine/domains/ghestine.ir/Ghestine}"; H="$(cd "$(dirname "$0")" && pwd)"
echo "### 1/3 چندمستاجری"; bash "$H/backend/tenancy/install.sh" "$ROOT"
echo "### 2/3 هسته (طرح‌ها، کارت مجازی، درگاه، مدارک)"; bash "$H/backend/core/install.sh" "$ROOT"
echo "### 3/3 امنیت ورود (کپچا، OTP)"; bash "$H/backend/security/install.sh" "$ROOT"
echo "همه‌ی ماژول‌ها ساخته شدند. حالا یک مایگریشن واحد بزنید (بعد از بکاپ دیتابیس):"
echo "  dotnet ef migrations add AddDigibilePayCore -p src/Ghestine.Infrastructure -s src/Ghestine.API"
echo "  dotnet ef database update -p src/Ghestine.Infrastructure -s src/Ghestine.API"
