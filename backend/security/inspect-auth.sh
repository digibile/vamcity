#!/bin/bash
# بررسی فقط‌خواندنی احراز هویت، کاربران، تم قدیمی و فایل‌های ورود. هیچ چیزی تغییر نمی‌کند و رمزها چاپ نمی‌شوند.
ROOT="${1:-/home/ghestine/domains/ghestine.ir/Ghestine}"; W="${2:-$ROOT/src/Ghestine.API/wwwroot}"
O="$HOME/auth-inspect-$(date +%Y%m%d%H%M).txt"
red(){ sed -E 's/("?(password|secret|key|token|apikey|connectionstring)[A-Za-z]*"?\s*[:=]\s*)"[^"]{3,}"/\1"***"/Ig'; }
{
echo "## کنترلرهای احراز هویت"; grep -rlE "Auth|Login|Register" "$ROOT/src" --include=*.cs | grep -v /obj/ | head -20
echo; echo "## مسیرها و ورودی‌های مدل"; for f in $(grep -rlE "class .*Auth.*Controller" "$ROOT/src" --include=*.cs | grep -v /obj/); do echo "-- $f"; cat "$f" | red; done
echo; echo "## سرویس احراز هویت"; for f in $(grep -rlE "class .*(AuthService|TokenService|JwtService)" "$ROOT/src" --include=*.cs | grep -v /obj/); do echo "-- $f"; cat "$f" | red; done
echo; echo "## موجودیت کاربر و نقش"; for f in $(grep -rlE "class (User|AppUser|Role|UserRole|Permission)\b" "$ROOT/src" --include=*.cs | grep -v /obj/); do echo "-- $f"; cat "$f"; done
echo; echo "## هش رمز"; grep -rnE "BCrypt|PasswordHasher|Argon2|Rfc2898|SHA256" "$ROOT/src" --include=*.cs | grep -v /obj/ | cut -c1-160 | head
echo; echo "## تنظیمات JWT در Program.cs (بدون مقدار)"; grep -nE "AddAuthentication|AddJwtBearer|TokenValidation|AddAuthorization|AddPolicy|UseAuthentication|UseAuthorization|RateLimiter|Cors" "$ROOT/src/Ghestine.API/Program.cs"
echo; echo "## فایل‌های ورود و ثبت‌نام و استایل"; ls -la "$W" | head -40; ls "$W"/css "$W"/assets 2>/dev/null | head -30
echo; echo "## تاریخچه‌ی git برای تم تیره"; cd "$ROOT" && git branch -a 2>&1 | head -20; git tag 2>&1 | tail -10
git log --oneline -30 -- src/Ghestine.API/wwwroot 2>&1 | head -30
git log --oneline --all -i --grep="dark\|theme\|تیره\|قالب" 2>&1 | head -15
echo; echo "## کاربران فعلی (فقط تعداد و نقش، بدون شماره)"; echo "از دستور SQL زیر استفاده کنید و نتیجه را بدون شماره بفرستید:"
echo "SELECT Role, COUNT(*) FROM Users GROUP BY Role;  -- نام جدول/ستون را با کد بالا تطبیق دهید"
} > "$O" 2>&1
echo "ذخیره شد: $O"; echo "یک بار مرور کنید که رمز یا کلیدی در آن نیست، بعد برای من بفرستید."
