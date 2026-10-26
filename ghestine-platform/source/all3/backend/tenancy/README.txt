هسته‌ی چندمستاجری با دامنه
==========================
هر دامنه به یک «مستأجر» (برند) وصل می‌شود: نام، لوگو، رنگ‌ها و داده‌های خودش.
دامنه‌ی ناشناس (مثل ghestine.ir تا وقتی ثبت نشده) به مستأجر پیش‌فرض «دیجی‌بایل پی» می‌رسد.

مسیرها
  GET  /api/v1/tenant/config                         عمومی: برند دامنه‌ی فعلی (صفحه‌ها نام و رنگ را از اینجا می‌گیرند)
  GET/POST /api/v1/admin/tenants                     فقط نقش SuperAdmin
  PUT  /api/v1/admin/tenants/{id}
  POST /api/v1/admin/tenants/{id}/domains            افزودن دامنه {host,isPrimary}
  DELETE /api/v1/admin/tenants/{id}/domains/{domainId}

افزودن دامنه‌ی جدید روی سرور (DirectAdmin + nginx)
  1) DNS دامنه: رکورد A به IP سرور
  2) در DirectAdmin دامنه را به‌عنوان Domain Pointer یا دامنه‌ی جدید اضافه کنید و Let's Encrypt بگیرید
  3) در nginx همان دامنه به پورت 5200 پراکسی شود و هدر Host/X-Forwarded-Host عبور کند:
       proxy_set_header Host $host;
       proxy_set_header X-Forwarded-Host $host;
       proxy_pass http://127.0.0.1:5200;
  4) دامنه را در پنل به مستأجر وصل کنید.

جداسازی داده: محصولات وام، کارت‌ها و مدارک ماژول هسته با TenantId جدا می‌شوند.
هنوز جدا نشده: کاربران و جدول‌های قدیمی پروژه (نیاز به دیدن ساختار فعلی؛ بعد از inspect-auth انجام می‌شود).
