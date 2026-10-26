بسته‌ی صفحه‌ی اصلی نسخه‌ی ۳ قسطینه
1) فایل template-v3.html را در wwwroot کنار template-v2.html کپی کنید.
2) در پنل: کلید SiteContent با نام homepage.template را روی v3 بگذارید (یا index.html را از آن کپی کنید).
3) فرم ورود/عضویت به این آدرس‌ها وصل است (در بالای اسکریپت CFG.endpoints قابل تغییر):
   /api/v1/Auth/login  /api/v1/Auth/register  /api/v1/Auth/verify-otp  /api/v1/Auth/me
   نام فیلدهای بدنه: phoneNumber, password, fullName, nationalCode, referralCode. اگر API شما فرق دارد، فقط همان بخش را اصلاح کنید.
4) طرح‌ها از GET /api/v1/loan-products خوانده می‌شود و فقط isActive=true نمایش داده می‌شود (تابع normPlan برای نگاشت فیلدها).
5) دکمه‌ی پنل مدیریت فقط وقتی نشان داده می‌شود که /Auth/me نقش admin برگرداند.

admin-preview.html: پیش‌نمایش پنل مدیریت (داده‌ها نمونه است و به API وصل نیست). برای دیدن، فایل را در مرورگر باز کنید.
نکته: این فایل مستقل است ولی فونت‌ها از Google Fonts بارگذاری می‌شوند؛ بدون اینترنت از فونت جایگزین استفاده می‌شود.
