# SportMarket

فروشگاه آنلاین پکیج‌های ورزشی — بک‌اند ASP.NET Core 10 با معماری Clean Architecture + CQRS (MediatR) و فرانت‌اند React 19 + Vite + TypeScript.

## ساختار پروژه

```
backend/
  SportMarket.slnx
  src/
    SportMarket.Domain/          موجودیت‌ها و enumها (بدون وابستگی)
    SportMarket.Application/     CQRS: Command/Query + Handler + Validator، DTOها، interfaceها
    SportMarket.Infrastructure/  EF Core (SQL Server)، Identity، JWT، ذخیرهٔ فایل
    SportMarket.Api/             کنترلرها، Middleware، Swagger، Seeder
frontend/                        React + Vite + Tailwind (RTL، فارسی)
```

وابستگی‌ها فقط به سمت داخل است: `Api → Infrastructure → Application → Domain`.

## پیش‌نیازها

- .NET SDK 10
- Node.js 20+ و npm
- SQL Server (LocalDB یا نسخهٔ کامل) — رشتهٔ اتصال در `backend/src/SportMarket.Api/appsettings.json`

## اجرا

**بک‌اند** (پورت `7200`):

یک‌بار کلید امضای JWT را در user-secrets تنظیم کنید (در ریپو ذخیره نمی‌شود):

```bash
cd backend/src/SportMarket.Api
dotnet user-secrets set "Jwt:SecretKey" "<یک-کلید-تصادفی-حداقل-۳۲-کاراکتری>"
```

سپس:

```bash
cd backend
dotnet run --project src/SportMarket.Api
```

اگر `Jwt:SecretKey` تنظیم نشده باشد، برنامه با پیام واضحی متوقف می‌شود.

Migrationها و دادهٔ اولیه هنگام شروع خودکار اعمال می‌شوند. Swagger: <http://localhost:7200/swagger>

**فرانت‌اند** (پورت `5173`):

```bash
cd frontend
npm install
npm run dev
```

Vite مسیرهای `/api` و `/uploads` را به `http://localhost:7200` پروکسی می‌کند؛ پس بک‌اند باید بالا باشد.

## نکات

- JWT با نقش `Admin` برای مسیرهای مدیریتی محافظت می‌شود.
- احراز هویت فرانت‌اند توکن را در `localStorage` نگه می‌دارد و از طریق interceptor به درخواست‌ها اضافه می‌کند.
- فایل‌های آپلودشده در `backend/src/SportMarket.Api/wwwroot/uploads` ذخیره می‌شوند و در Git نیستند.
- قبل از بیلد، اگر API در حال اجراست متوقفش کنید؛ در غیر این صورت فایل‌های `bin/` قفل می‌شوند.
