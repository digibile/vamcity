ماژول هسته‌ی قسطینه (بک‌اند)
================================
شامل:
- محصولات وام (طرح‌ها): GET /api/v1/loan-products (عمومی، فقط فعال‌ها) و GET /api/v1/loan-products/{slug}
  مدیر: GET/POST/PUT /api/v1/admin/loan-products ، POST .../{id}/toggle ، POST .../seed
- کارت مجازی: مشتری: GET /api/v1/me/cards ، POST .../{id}/reveal|freeze|unfreeze ، GET .../{id}/transactions
  مدیر: GET /api/v1/admin/virtual-cards ، POST .../issue ، POST .../{id}/freeze|unfreeze|close ، PUT .../{id}/limit
- درگاه داخلی با کارت: POST /api/v1/gateway/card-charge  (هدر X-Merchant-Key)
  بدنه: { "cardNumber","cvv","expiry":"1408/08","amount","orderId","merchantName" }
- زونکن مدارک: مشتری: GET/POST /api/v1/me/documents (multipart: kind, loanId, file=عکس)
  مدیر: GET /api/v1/admin/documents ، GET .../{id}/file ، PUT .../{id}/review

نصب: پوشه را روی سرور باز کنید و بزنید:  bash install.sh
(اسکریپت قبل از تغییر، از AppDbContext و Program.cs بکاپ می‌گیرد و اگر build خطا داد، دستور برگرداندن را چاپ می‌کند.)

صدور خودکار کارت برای وام:
  داخل LoanApplicationService بعد از تأیید/امضا، سرویس IVirtualCardService را تزریق کنید و صدا بزنید:
  await _cards.IssueForLoanAsync(loan.Id, loan.UserId, holderName, loan.Amount, ct);
  (نام فیلدهای واقعی را بعد از دیدن فایل LoanApplication.cs برایتان وصل می‌کنم.)

امنیت:
- شماره‌ی کامل کارت و CVV و عکس مدارک با ASP.NET Data Protection رمزنگاری می‌شوند. کلیدها در /home/ghestine/dpkeys است؛ بکاپ بگیرید.
- مدارک خارج از wwwroot ذخیره می‌شوند و فقط از مسیر مدیر قابل دانلودند.
- OCR فعلاً NullOcrProvider است (متن خالی، وضعیت «در انتظار بررسی»). با پیاده‌سازی IOcrProvider سرویس واقعی وصل می‌شود.
- برای ثبت در Audit، فعلاً گردش کارت در جدول VirtualCardTxn ثبت می‌شود (شامل نمایش اطلاعات کارت).

محدودیت‌ها (صادقانه):
- روی سیستم من .NET نبود و کد کامپایل‌شده تست نشده؛ ممکن است در build سرور خطای ناسازگاری با ساختار پروژه‌ی شما بیاید. خروجی را بفرستید تا اصلاح کنم.
- اعتبارسنجی نقش‌ها فرض می‌کند نقش مدیر "Admin" است (مثل ماژول پیامک).
- فیلدهای UserId/LoanId از نوع Guid فرض شده‌اند.
