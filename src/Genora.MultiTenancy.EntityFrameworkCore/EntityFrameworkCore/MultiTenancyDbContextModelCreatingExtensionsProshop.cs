using Genora.MultiTenancy.DomainModels.AppProCategories;
using Genora.MultiTenancy.DomainModels.AppProItems;
using Genora.MultiTenancy.DomainModels.AppProOrderActivity;
using Genora.MultiTenancy.DomainModels.AppProOrders;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public static class MultiTenancyDbContextModelCreatingExtensionsProshop
{
    public static void ConfigureProshopModule(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<ProCategory>(b =>
        {
            b.ToTable("AppProCategories");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Code).HasMaxLength(64);
            b.Property(x => x.SortOrder).HasDefaultValue(0);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique()
                .HasFilter("[Code] IS NOT NULL")
                .HasDatabaseName("IX_AppProCategories_TenantId_Code");

            b.HasIndex(x => new { x.TenantId, x.SortOrder })
                .HasDatabaseName("IX_AppProCategories_TenantId_SortOrder");
        });

        builder.Entity<ProItem>(b =>
        {
            b.ToTable("AppProItems");
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
                .HasDatabaseName("IX_AppProItems_TenantId_CategoryId_SortOrder");

            b.HasIndex(x => new { x.TenantId, x.Name })
                .HasDatabaseName("IX_AppProItems_TenantId_Name");
        });

        builder.Entity<ProOrder>(b =>
        {
            b.ToTable("AppProOrders");
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

            // CustomerId là soft reference (golf: AppCustomers, salon: AppSalonBeautyCustomers)
            // Không khai báo HasOne để tránh FK cứng — service tự lookup theo domain.
            b.HasIndex(x => x.CustomerId)
                .HasDatabaseName("IX_AppProOrders_CustomerId");

            b.HasIndex(x => new { x.TenantId, x.OrderCode })
                .IsUnique()
                .HasDatabaseName("IX_AppProOrders_TenantId_OrderCode");

            b.HasIndex(x => new { x.TenantId, x.CreationTime })
                .HasDatabaseName("IX_AppProOrders_TenantId_CreationTime");

            b.HasIndex(x => new { x.TenantId, x.ServiceStatus, x.PaymentStatus })
                .HasDatabaseName("IX_AppProOrders_TenantId_ServiceStatus_PaymentStatus");

            b.HasIndex(x => new { x.TenantId, x.BagTag })
                .HasDatabaseName("IX_AppProOrders_TenantId_BagTag");
        });

        builder.Entity<ProOrderItem>(b =>
        {
            b.ToTable("AppProOrderItems");
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
                .HasDatabaseName("IX_AppProOrderItems_OrderId");

            b.HasIndex(x => x.ItemId)
                .HasDatabaseName("IX_AppProOrderItems_ItemId");
        });

        builder.Entity<ProOrderActivity>(b =>
        {
            b.ToTable("AppProOrderActivity");
            b.ConfigureByConvention();

            b.Property(x => x.ActionType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Title).IsRequired().HasMaxLength(255);
            b.Property(x => x.Description).HasMaxLength(1000);

            b.HasIndex(x => new { x.TenantId, x.OrderId })
                .HasDatabaseName("IX_AppProOrderActivity_TenantId_OrderId");

            b.HasIndex(x => new { x.TenantId, x.ActionTime })
                .HasDatabaseName("IX_AppProOrderActivity_TenantId_ActionTime");
        });
    }
}
