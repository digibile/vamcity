#!/bin/bash
# آزمون خودکار امنیت ورود روی سرور. هیچ پیامکی ارسال نمی‌کند و هیچ داده‌ای نمی‌نویسد.
# استفاده: bash smoke-test.sh [آدرس پایه]     پیش‌فرض: http://127.0.0.1:5200
B="${1:-http://127.0.0.1:5200}"; A="$B/api/v1/security"; P=0; F=0
t(){ # نام، شرط (کد انتظار)، کد واقعی
  if [ "$2" = "$3" ]; then echo "  ✔ $1"; P=$((P+1)); else echo "  ✘ $1 (انتظار $2، دریافت $3)"; F=$((F+1)); fi; }
code(){ curl -s -o /dev/null -m 15 -w '%{http_code}' "$@"; }
J='-H Content-Type:application/json'
echo "== کپچا =="
R=$(curl -s -m 15 "$A/captcha"); t "کپچا ساخته می‌شود" 1 "$(echo "$R" | grep -c '"image":"data:image/svg')"
t "کپچا cache نمی‌شود" 1 "$(curl -sI "$A/captcha" | grep -ic 'cache-control:.*no-store')"
echo "== OTP بدون/با کپچای غلط =="
t "OTP بدون کپچا رد می‌شود" 400 "$(code -X POST $J -d '{"phone":"09120000000","captchaId":"x","captchaAnswer":"00000"}' "$A/otp/request")"
t "شماره نامعتبر رد می‌شود" 400 "$(code -X POST $J -d '{"phone":"123","captchaId":"x","captchaAnswer":"1"}' "$A/otp/request")"
echo "== تأیید کد =="
t "کد غلط/بدون درخواست رد می‌شود" 400 "$(code -X POST $J -d '{"phone":"09120000000","code":"123456"}' "$A/otp/verify")"
t "کد ناقص رد می‌شود" 400 "$(code -X POST $J -d '{"phone":"09120000000","code":"12"}' "$A/otp/verify")"
echo "== ورود با رمز =="
t "ورود بدون کپچا رد می‌شود" 400 "$(code -X POST $J -d '{"username":"09120000000","password":"x","captchaId":"x","captchaAnswer":"1"}' "$A/login")"
echo "== ثبت‌نام =="
t "ثبت‌نام بدون تیکت رد می‌شود" 400 "$(code -X POST $J -d '{"ticket":"bad","fullName":"علی رضایی","nationalCode":"0499370899","password":"abcd1234","captchaId":"x","captchaAnswer":"1"}' "$A/register")"
echo "== دسترسی‌های محافظت‌شده =="
t "پنل مدیریت بدون توکن 401" 401 "$(code "$B/api/v1/admin/loan-products")"
t "مدیریت مستأجرها بدون توکن 401" 401 "$(code "$B/api/v1/admin/tenants")"
echo "== محدودیت نرخ (۴۵ تلاش پیاپی باید به ۴۲۹ برسد) =="
L=0; for i in $(seq 1 45); do c=$(code -X POST $J -d '{"username":"zz","password":"x","captchaId":"x","captchaAnswer":"1"}' "$A/login"); [ "$c" = 429 ] && L=1; done
t "محدودیت نرخ فعال می‌شود" 1 "$L"
echo; echo "قبول: $P | رد: $F"; echo "آزمون‌های نیازمند شما: ورود واقعی با رمز، OTP واقعی با موبایل خودتان، انقضای ۱۲۰ ثانیه، ورود سوپرادمین."
[ $F -eq 0 ]
