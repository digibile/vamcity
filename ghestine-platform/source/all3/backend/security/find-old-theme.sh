#!/bin/bash
# جست‌وجوی فقط‌خواندنی تم تیره‌ی قبلی در تاریخچه‌ی git. هیچ فایلی در پروژه تغییر نمی‌کند؛ نسخه‌های قدیمی در ~/old-theme استخراج می‌شوند.
ROOT="${1:-/home/ghestine/domains/ghestine.ir/Ghestine}"; cd "$ROOT" || exit 1
OUT="$HOME/old-theme"; mkdir -p "$OUT"
echo "== شاخه‌ها و تگ‌ها =="; git branch -a | head -20; git tag | tail -10
echo; echo "== کامیت‌هایی که پوشه‌ی wwwroot/css را تغییر داده‌اند =="
git log --date=short --pretty='%h %ad %s' -40 -- 'src/Ghestine.API/wwwroot' | head -40
echo; echo "== کامیت‌هایی که پیام یا تغییرشان به تیره/theme/dark مربوط است =="
git log --all --date=short --pretty='%h %ad %s' -i --grep='dark\|theme\|تیره\|قالب' | head -20
git log --all --date=short --pretty='%h %ad %s' -S'--bg:#' -- 'src/Ghestine.API/wwwroot' | head -20
echo; echo "== استخراج نسخه‌های قدیمی admin.html (۵ کامیت آخر) =="
for h in $(git log --pretty=%h -5 -- 'src/Ghestine.API/wwwroot/admin.html'); do mkdir -p "$OUT/$h"; git show "$h:src/Ghestine.API/wwwroot/admin.html" > "$OUT/$h/admin.html" 2>/dev/null && echo "$OUT/$h/admin.html"; done
echo; echo "برای دیدن هر نسخه آن را در مرورگر باز کنید و شناسه‌ی کامیت نسخه‌ی دلخواه را بفرستید."
