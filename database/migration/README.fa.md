# بسته مهاجرت VamCity

## مبنای قطعی این مرحله
طبق تأیید کاربر، منبع اصلی و مبنای انتقال فقط دیتابیس **`vamcityc_vamcity`** است. مقصد لینوکسی یک دیتابیس MariaDB با نام **`vamcitydigibilepay`** خواهد بود. دو دیتابیس `vamcityc_bluvamdb` و `vamcityc_transferDb` در این مرحله وارد انتقال اصلی نمی‌شوند.

## مقصد
- MariaDB 10.6.17
- دیتابیس واحد: `vamcitydigibilepay`
- charset: `utf8mb4`
- collation: `utf8mb4_unicode_ci`
- دیتابیس مقصد ساخته شده و در زمان ساخت خالی بوده است.

## بکاپ اصلی و نتیجه آزمون
فایل مبدأ:
`vamcityc_vamcity_2026-10-29_19-20-04.zip`

مشخصات:
- نام داخلی در سربرگ بکاپ: `vamcityc_vamcity`
- نوع: Full database backup
- تاریخ بکاپ: 2026-10-28
- حجم ZIP: 65,432,316 bytes
- حجم فایل BAK پس از استخراج: 358,735,360 bytes
- SHA-256: `f7dab1ab7e4260b3937f01b94452f3bae25a2f6d9c6060bdb7e481370fbe82fc`
- نام منطقی فایل داده: `vamciydb`
- نام منطقی فایل لاگ: `vamciydb_log`
- ZIP integrity: موفق
- SQL Server `RESTORE VERIFYONLY`: موفق؛ پیام سرور: `The backup set on file 1 is valid.`
- `DBCC CHECKDB` روی دیتابیس زنده `vamcityc_vamcity`: موفق، بدون خطای گزارش‌شده
- backup checksum در فایل وجود ندارد؛ بنابراین VERIFYONLY قابلیت بررسی checksum ذخیره‌شده را نداشته است.

گزارش اجرا: https://github.com/digibile/vamcity/actions/runs/40994766360

## موجودی واقعی دیتابیس اصلی
| شِما | جدول | رکورد |
|---|---:|---:|
| dbo | 77 | 527,540 |
| HangFire | 11 | 3,272 |
| **مجموع** | **88** | **530,812** |

- ستون‌ها: 959
- ستون‌های کلید اصلی/Unique: 106
- ستون‌های Foreign Key: 188
- جدول خالی: 21
- View / Procedure / Trigger / Check Constraint / Sequence: همگی صفر

### جدول‌های کلیدی
| جدول | رکورد |
|---|---:|
| AspNetUsers | 54,621 |
| AspNetUserRoles | 54,618 |
| UserLoanRequests | 17,248 |
| UserLoanRequestStatusHistories | 49,144 |
| UserLoanRequestAuthorizationItems | 19,566 |
| UserLoanRequestAuthorizationItemAttachments | 19,342 |
| UserLoanPayments | 29,148 |
| UserLoans | 1,404 |
| Loans | 112 |
| Tickets | 54,197 |
| TicketResponses | 54,661 |
| AppSms | 138,461 |

## نگاشت نوع داده
انواع مشاهده‌شده در 959 ستون:
- nvarchar: 298
- uniqueidentifier: 232
- int: 108
- datetime2: 64
- decimal: 61
- bigint: 49
- bit: 84
- float: 10
- tinyint: 4
- time: 3
- datetimeoffset: 1

تبدیل نوع‌ها باید با حفظ مقدار، precision/scale، NULL، identity و مقادیر GUID انجام شود. برای GUIDها باید نمایش و مقایسه سازگار با برنامه حفظ شود. تاریخ‌ها باید با timezone/offset اصلی کنترل شوند.

## برنامه انتقال
1. کنترل محیط انتقال و ابزارهای اتصال SQL Server/MariaDB روی میزبان.
2. تهیه snapshot/backup از مقصد خالی و recheck فضای دیسک.
3. تولید DDL برای تمام 88 جدول، دو schema، کلیدها، identity و indexها.
4. اجرای ساختار در دیتابیس واحد `vamcitydigibilepay`.
5. انتقال داده‌ها با batch و حفظ NULL، Unicode، GUID، decimal و تاریخ.
6. بازسازی FKها و indexها و بررسی HangFire schema.
7. تطبیق تعداد رکورد هر جدول و کلیدها با مبدأ.
8. تغییر provider و connection string برنامه فقط بعد از قبولی تطبیق داده‌ها و آزمون برنامه.

## وضعیت فعلی
- منبع اصلی انتخاب شده: موفق.
- ZIP و فایل BAK: سالم.
- RESTORE VERIFYONLY: موفق.
- DBCC CHECKDB دیتابیس زنده: موفق.
- موجودی ساختار 88 جدول: استخراج شده.
- مقصد MariaDB: ساخته شده و خالی است.
- انتقال واقعی رکوردها: هنوز شروع نشده.
- برنامه هنوز به MariaDB cutover نشده است.

## محدودیت ایمنی
تا زمانی که اتصال عملیاتی SQL Server به MariaDB، ابزار انتقال و فضای کافی تأیید نشده‌اند، import را شروع نمی‌کنیم. فایل خام بکاپ یا اطلاعات مشتریان به مخزن عمومی GitHub منتقل نمی‌شود.
