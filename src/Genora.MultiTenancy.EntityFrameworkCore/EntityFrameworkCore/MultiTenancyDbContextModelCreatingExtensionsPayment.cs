using Genora.MultiTenancy.DomainModels.AppPaymentConfigurations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public static class MultiTenancyDbContextModelCreatingExtensionsPayment
{
    public static void ConfigurePaymentModule(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<PaymentConfiguration>(b =>
        {
            b.ToTable("AppPaymentConfigurations");
            b.ConfigureByConvention();

            b.HasKey(x => x.Id);

            b.Property(x => x.PaymentProviderName).IsRequired().HasMaxLength(100);
            b.Property(x => x.BankBin).HasMaxLength(20);
            b.Property(x => x.AccountNumber).HasMaxLength(50);
            b.Property(x => x.AccountName).HasMaxLength(200);
            b.Property(x => x.MerchantId).HasMaxLength(200);
            b.Property(x => x.ApiKey).HasMaxLength(500);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.LogoUrl).HasMaxLength(500);

            b.HasIndex(x => new { x.TenantId, x.IsActive })
                .HasDatabaseName("IX_AppPaymentConfigurations_TenantId_IsActive");

            b.HasIndex(x => new { x.TenantId, x.DisplayOrder })
                .HasDatabaseName("IX_AppPaymentConfigurations_TenantId_DisplayOrder");
        });
    }
}
