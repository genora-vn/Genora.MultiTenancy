using Genora.MultiTenancy.DomainModels.AppHlApiLogs;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Genora.MultiTenancy.DomainModels.AppHlOrders;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public static class MultiTenancyDbContextModelCreatingExtensionsHoaLinh
{
    public static void ConfigureHoaLinhModule(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // ========== HlOrder ==========
        builder.Entity<HlOrder>(b =>
        {
            b.ToTable("AppHlOrders", "HL");
            b.ConfigureByConvention();

            b.Property(x => x.OrderCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.CustomerCode).HasMaxLength(50);
            b.Property(x => x.CustomerName).HasMaxLength(250);
            b.Property(x => x.CustomerPhone).HasMaxLength(20);
            b.Property(x => x.BranchCode).HasMaxLength(50);
            b.Property(x => x.BranchName).HasMaxLength(250);
            b.Property(x => x.DeliveryAddress).HasMaxLength(500);
            b.Property(x => x.ReceiverName).HasMaxLength(150);
            b.Property(x => x.ReceiverPhone).HasMaxLength(20);
            b.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            b.Property(x => x.DiscountCode).HasMaxLength(50);
            b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.SystemDiscount).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.DeliveryStatus).HasConversion<byte>();
            b.Property(x => x.PaymentStatus).HasConversion<byte>();
            b.Property(x => x.PaymentMethod).HasConversion<byte?>();
            b.Property(x => x.CancelNote).HasMaxLength(500);
            b.Property(x => x.ExternalOrderCode).HasMaxLength(50);

            b.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.OrderCode })
                .IsUnique()
                .HasDatabaseName("IX_AppHlOrders_TenantId_OrderCode");

            b.HasIndex(x => new { x.TenantId, x.CreationTime })
                .HasDatabaseName("IX_AppHlOrders_TenantId_CreationTime");

            b.HasIndex(x => new { x.TenantId, x.CustomerCode })
                .HasDatabaseName("IX_AppHlOrders_TenantId_CustomerCode");

            b.HasIndex(x => new { x.TenantId, x.DeliveryStatus, x.PaymentStatus })
                .HasDatabaseName("IX_AppHlOrders_TenantId_Status");
        });

        // ========== HlOrderItem ==========
        builder.Entity<HlOrderItem>(b =>
        {
            b.ToTable("AppHlOrderItems", "HL");
            b.ConfigureByConvention();

            b.Property(x => x.ProductCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.ProductName).IsRequired().HasMaxLength(500);
            b.Property(x => x.ProductGroupCode).HasMaxLength(50);
            b.Property(x => x.ProductGroupName).HasMaxLength(250);
            b.Property(x => x.BrandName).HasMaxLength(150);
            b.Property(x => x.ProductUnit).HasMaxLength(50);
            b.Property(x => x.ImageUrl).HasMaxLength(500);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.OriginalPrice).HasColumnType("decimal(18,2)");
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");

            b.HasIndex(x => new { x.TenantId, x.OrderId })
                .HasDatabaseName("IX_AppHlOrderItems_TenantId_OrderId");
        });

        // ========== HlGiftExchange ==========
        builder.Entity<HlGiftExchange>(b =>
        {
            b.ToTable("AppHlGiftExchanges", "HL");
            b.ConfigureByConvention();

            b.Property(x => x.ExchangeCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.CustomerCode).HasMaxLength(50);
            b.Property(x => x.CustomerName).HasMaxLength(250);
            b.Property(x => x.CustomerPhone).HasMaxLength(20);
            b.Property(x => x.GiftName).IsRequired().HasMaxLength(500);
            b.Property(x => x.GiftCode).HasMaxLength(100);
            b.Property(x => x.GiftImageUrl).HasMaxLength(500);
            b.Property(x => x.Status).HasConversion<byte>();
            b.Property(x => x.UrBoxVoucherCode).HasMaxLength(200);
            b.Property(x => x.DeliveryAddress).HasMaxLength(500);

            b.HasIndex(x => new { x.TenantId, x.ExchangeCode })
                .IsUnique()
                .HasDatabaseName("IX_AppHlGiftExchanges_TenantId_ExchangeCode");

            b.HasIndex(x => new { x.TenantId, x.CustomerCode })
                .HasDatabaseName("IX_AppHlGiftExchanges_TenantId_CustomerCode");

            b.HasIndex(x => new { x.TenantId, x.Status })
                .HasDatabaseName("IX_AppHlGiftExchanges_TenantId_Status");
        });

        // ========== HlApiLog ==========
        builder.Entity<HlApiLog>(b =>
        {
            b.ToTable("AppHlApiLogs", "HL");
            b.ConfigureByConvention();

            b.Property(x => x.HttpMethod).IsRequired().HasMaxLength(10);
            b.Property(x => x.RequestUrl).IsRequired().HasMaxLength(500);
            b.Property(x => x.DataType).HasMaxLength(50);
            b.Property(x => x.CallerSource).HasMaxLength(50);

            b.HasIndex(x => new { x.TenantId, x.CreationTime })
                .HasDatabaseName("IX_AppHlApiLogs_TenantId_CreationTime");

            b.HasIndex(x => new { x.TenantId, x.IsError })
                .HasDatabaseName("IX_AppHlApiLogs_TenantId_IsError");

            b.HasIndex(x => new { x.TenantId, x.DataType })
                .HasDatabaseName("IX_AppHlApiLogs_TenantId_DataType");
        });
    }
}
