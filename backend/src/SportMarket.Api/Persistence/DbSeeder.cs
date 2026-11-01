using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SportMarket.Infrastructure.Data;
using SportMarket.Infrastructure.Identity;

namespace SportMarket.Api.Persistence;

/// <summary>ایجاد داده‌های اولیه: نقش‌ها، ادمین پیش‌فرض و محتوای نمونه</summary>
public static class DbSeeder
{
    public const string AdminEmail = "admin@sportmarket.ir";
    public const string AdminPassword = "Admin@1234";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scoped = scope.ServiceProvider;

        var roleManager = scoped.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scoped.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scoped.GetRequiredService<ApplicationDbContext>();
        var env = scoped.GetRequiredService<IWebHostEnvironment>();

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "User");

        if (await userManager.FindByEmailAsync(AdminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FullName = "مدیر سیستم",
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        var seedFolder = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "uploads", "seed");
        Directory.CreateDirectory(seedFolder);

        if (!await db.Sliders.AnyAsync())
        {
            db.Sliders.AddRange(
                new Domain.Entities.Slider
                {
                    Title = "به اسپورت مارکت خوش آمدید",
                    Subtitle = "پکیج‌های تمرینی و تغذیه با طراحی مربیان حرفه‌ای",
                    ImageUrl = WriteSeedSvg(seedFolder, "slider-1.svg", "اسپورت مارکت", "#0ea5e9", "#4f46e5", 1200, 520, 64),
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Domain.Entities.Slider
                {
                    Title = "تخفیف ویژه پکیج‌های شروع تمرین",
                    Subtitle = "تا ۳۰٪ تخفیف روی پکیج‌های مقدماتی",
                    ImageUrl = WriteSeedSvg(seedFolder, "slider-2.svg", "تخفیف ویژه", "#f97316", "#dc2626", 1200, 520, 64),
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Domain.Entities.Slider
                {
                    Title = "پیشرفت واقعی با برنامه اختصاصی",
                    Subtitle = "برنامه تمرین و تغذیه متناسب با هدف شما",
                    ImageUrl = WriteSeedSvg(seedFolder, "slider-3.svg", "برنامه اختصاصی", "#059669", "#0d9488", 1200, 520, 64),
                    DisplayOrder = 3,
                    IsActive = true
                });
        }

        if (!await db.Packages.AnyAsync())
        {
            var packages = new (string Title, string Description, decimal Price, decimal? Discount)[]
            {
                ("برنامه تمرین بدنسازی مقدماتی", "برنامه ۱۲ هفته‌ای تمرین با وزن برای افراد مبتدی، همراه با ویدیوهای آموزشی فرم صحیح حرکات و جدول پیشرفت هفتگی.", 350000, 280000),
                ("رژیم غذایی حرفه‌ای عضله‌سازی", "برنامه تغذیه اختصاصی برای افزایش حجم عضلانی بر اساس کالری و میکروهای محاسبه‌شده برای وزن فعلی شما.", 500000, null),
                ("پکیج تمرین در خانه بدون وسیله", "تمرینات کامل بدن بدون نیاز به وسایل باشگاه؛ مناسب برای شروع سریع در منزل با کمترین فضا.", 190000, 149000),
                ("برنامه چربی‌سوزی ۱۲ هفته", "ترکیب تمرین هوازی و قدرتی همراه با برنامه غذایی کاهش چربی، با هدف‌گذاری هفتگی و ردیابی پیشرفت.", 420000, null),
                ("پکیج مکمل‌یاری علمی", "راهنمای علمی مصرف مکمل‌ها متناسب با هدف تمرینی، همراه با جدول زمان‌بندی مصرف در طول روز.", 650000, 540000),
                ("تمرین قدرتی پاورلیفتینگ", "برنامه تخصصی اسکوات، پرس سینه و ددلیفت برای افزایش قدرت با پریودایزیشن ۱۶ هفته‌ای.", 780000, null)
            };

            foreach (var (title, description, price, discount) in packages)
            {
                var slug = "package-" + Guid.NewGuid().ToString("N")[..8];
                db.Packages.Add(new Domain.Entities.Package
                {
                    Title = title,
                    Slug = slug,
                    Description = description,
                    Price = price,
                    DiscountedPrice = discount,
                    ImageUrl = WriteSeedSvg(seedFolder, $"{slug}.svg", title, "#1d4ed8", "#0ea5e9", 640, 480, 34),
                    IsActive = true
                });
            }
        }

        if (!await db.Articles.AnyAsync())
        {
            var articles = new (string Title, string Summary, string Content)[]
            {
                ("۱۰ اشتباه رایج در تمرین بدنسازی", "آشنایی با اشتباهاتی که پیشرفت شما را کند می‌کنند و راه‌حل‌های ساده برای اصلاح آن‌ها.",
                    "بسیاری از ورزشکاران مبتدی بدون برنامه مشخص و با فرم اشتباه تمرین می‌کنند. در این مقاله ده اشتباه رایج شامل گرم کردن نکردن، افزایش سریع وزنه، بی‌توجهی به استراحت و تغذیه نامناسب را بررسی می‌کنیم و برای هر مورد راه‌حل عملی ارائه می‌دهیم."),
                ("تغذیه قبل و بعد از تمرین چه باید باشد؟", "چه بخوریم تا انرژی تمرین بالا برود و ریکاوری سریع‌تر شود؟",
                    "وعده قبل از تمرین باید ترکیبی از کربوهیدرات مناسب و پروتئین متوسط باشد و حدود ۶۰ تا ۹۰ دقیقه قبل از تمرین مصرف شود. بعد از تمرین، پروتئین با کیفیت و کربوهیدرات سریع جذب به ریکاوری گلیکوژن و ساخت عضله کمک می‌کند."),
                ("راهنمای شروع تمرین در خانه برای مبتدیان", "با چند حرکت ساده و یک برنامه هفتگی، مسیر تناسب اندام را از خانه شروع کنید.",
                    "برای شروع تمرین در خانه به وسایل پیچیده نیاز نیست. سه جلسه در هفته با ترکیب اسکوات بدن، شنا، پلانک و لانج می‌تواند پایه‌ای قدرتمند بسازد. نکته کلیدی، افزایش تدریجی تعداد تکرارها و حفظ نظم تمرینی است.")
            };

            foreach (var (title, summary, content) in articles)
            {
                var slug = "article-" + Guid.NewGuid().ToString("N")[..8];
                db.Articles.Add(new Domain.Entities.Article
                {
                    Title = title,
                    Slug = slug,
                    Summary = summary,
                    Content = content,
                    IsPublished = true,
                    PublishedAtUtc = DateTime.UtcNow,
                    CoverImageUrl = WriteSeedSvg(seedFolder, $"{slug}.svg", title, "#7c3aed", "#db2777", 800, 450, 30)
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static string WriteSeedSvg(string folder, string fileName, string title, string from, string to, int width, int height, int fontSize)
    {
        var path = Path.Combine(folder, fileName);
        if (System.IO.File.Exists(path))
        {
            return "/uploads/seed/" + fileName;
        }

        var safeTitle = title.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">
              <defs>
                <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
                  <stop offset="0" stop-color="{from}"/>
                  <stop offset="1" stop-color="{to}"/>
                </linearGradient>
              </defs>
              <rect width="100%" height="100%" fill="url(#g)"/>
              <circle cx="{width * 0.85}" cy="{height * 0.2}" r="{height * 0.28}" fill="#ffffff" fill-opacity="0.12"/>
              <circle cx="{width * 0.12}" cy="{height * 0.85}" r="{height * 0.22}" fill="#ffffff" fill-opacity="0.10"/>
              <text x="50%" y="52%" text-anchor="middle" dominant-baseline="middle" font-family="Tahoma, 'Segoe UI', sans-serif" font-size="{fontSize}" font-weight="bold" fill="#ffffff" direction="rtl">{safeTitle}</text>
            </svg>
            """;

        System.IO.File.WriteAllText(path, svg);

        return "/uploads/seed/" + fileName;
    }
}
