using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Genora.MultiTenancy.EntityFrameworkCore;
public static class MultiTenancyDbContextModelCreatingExtensionsFnb
{
    public static void ConfigureFnbModule(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<FnbCategory>(b =>
        {
            b.ToTable("AppFnbCategories");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Code).HasMaxLength(64);
            b.Property(x => x.SortOrder).HasDefaultValue(0);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique()
                .HasFilter("[Code] IS NOT NULL")
                .HasDatabaseName("IX_AppFnbCategories_TenantId_Code");

            b.HasIndex(x => new { x.TenantId, x.SortOrder })
                .HasDatabaseName("IX_AppFnbCategories_TenantId_SortOrder");
        });

        builder.Entity<FnbItem>(b =>
        {
            b.ToTable("AppFnbItems");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.ImageUrl).HasMaxLength(500);
            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.IsAvailable).HasDefaultValue(true);
            b.Property(x => x.SortOrder).HasDefaultValue(0);

            b.HasOne(x => x.Category)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.CategoryId, x.SortOrder })
                .HasDatabaseName("IX_AppFnbItems_TenantId_CategoryId_SortOrder");

            b.HasIndex(x => new { x.TenantId, x.Name })
                .HasDatabaseName("IX_AppFnbItems_TenantId_Name");
        });

        builder.Entity<FnbOrder>(b =>
        {
            b.ToTable("AppFnbOrders");
            b.ConfigureByConvention();

            b.Property(x => x.OrderCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.BagTag).IsRequired().HasMaxLength(50);
            b.Property(x => x.CustomerName).HasMaxLength(150);
            b.Property(x => x.CustomerPhone).HasMaxLength(20);
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.ServiceStatus).HasConversion<byte>();
            b.Property(x => x.PaymentStatus).HasConversion<byte>();
            b.Property(x => x.PaymentMethod).HasConversion<byte?>();
            b.Property(x => x.CancelReason).HasConversion<byte?>();
            b.Property(x => x.CancelNote).HasMaxLength(500);

            b.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => new { x.TenantId, x.OrderCode })
                .IsUnique()
                .HasDatabaseName("IX_AppFnbOrders_TenantId_OrderCode");

            b.HasIndex(x => new { x.TenantId, x.CreationTime })
                .HasDatabaseName("IX_AppFnbOrders_TenantId_CreationTime");

            b.HasIndex(x => new { x.TenantId, x.ServiceStatus, x.PaymentStatus })
                .HasDatabaseName("IX_AppFnbOrders_TenantId_ServiceStatus_PaymentStatus");

            b.HasIndex(x => new { x.TenantId, x.BagTag })
                .HasDatabaseName("IX_AppFnbOrders_TenantId_BagTag");
        });

        builder.Entity<FnbOrderItem>(b =>
        {
            b.ToTable("AppFnbOrderItems");
            b.ConfigureByConvention();

            b.HasKey(x => x.Id);

            b.Property(x => x.ItemName).IsRequired().HasMaxLength(255);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.Quantity).IsRequired();

            b.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => x.OrderId)
                .HasDatabaseName("IX_AppFnbOrderItems_OrderId");

            b.HasIndex(x => x.ItemId)
                .HasDatabaseName("IX_AppFnbOrderItems_ItemId");
        });
    }
}
