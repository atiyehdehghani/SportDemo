using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Domain.Entities;
using SportMarket.Infrastructure.Identity;

namespace SportMarket.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Slider> Sliders => Set<Slider>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // جدول‌های Identity با نام فارغ از پیشوند AspNet
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        builder.Entity<Package>(entity =>
        {
            entity.ToTable("Packages");
            entity.Property(p => p.Title).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Slug).HasMaxLength(250).IsRequired();
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.Description).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.DiscountedPrice).HasPrecision(18, 2);
        });

        builder.Entity<Article>(entity =>
        {
            entity.ToTable("Articles");
            entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Slug).HasMaxLength(250).IsRequired();
            entity.HasIndex(a => a.Slug).IsUnique();
            entity.Property(a => a.Summary).HasMaxLength(500).IsRequired();
            entity.Property(a => a.Content).IsRequired();
        });

        builder.Entity<Slider>(entity =>
        {
            entity.ToTable("Sliders");
            entity.Property(s => s.Title).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Subtitle).HasMaxLength(300);
            entity.Property(s => s.ImageUrl).HasMaxLength(500).IsRequired();
        });

        builder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.Property(o => o.UserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(o => o.UserId);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.Property(i => i.PackageTitle).HasMaxLength(200).IsRequired();
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });
    }
}
