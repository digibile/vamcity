#!/bin/bash
# اجرای ابزار خط فرمان برنامه (--auth-doctor یا --create-superadmin) با همان تنظیمات و متغیرهای محیطی سرویس.
# استفاده: bash run-tool.sh --auth-doctor [نام_سرویس=ghestine]
FLAG="${1:---auth-doctor}"; SVC="${2:-ghestine}"
UNIT=$(systemctl cat "$SVC.service" 2>/dev/null) || { echo "سرویس $SVC پیدا نشد"; exit 1; }
WD=$(echo "$UNIT" | grep -E "^WorkingDirectory=" | tail -1 | cut -d= -f2-)
EXEC=$(echo "$UNIT" | grep -E "^ExecStart=" | tail -1 | cut -d= -f2-)
DLL=$(echo "$EXEC" | grep -oE "[^ ]+\.dll" | head -1)
[ -n "$DLL" ] || { echo "مسیر dll در ExecStart پیدا نشد: $EXEC"; exit 1; }
[ -n "$WD" ] && cd "$WD"
set -a
for EF in $(echo "$UNIT" | grep -E "^EnvironmentFile=" | cut -d= -f2- | tr -d '-'); do [ -r "$EF" ] && . "$EF" || echo "(فایل محیطی خوانده نشد: $EF)"; done
for E in $(echo "$UNIT" | grep -E "^Environment=" | cut -d= -f2-); do export "$E" 2>/dev/null; done
set +a
echo "اجرا: dotnet $DLL $FLAG"
exec dotnet "$DLL" "$FLAG"
