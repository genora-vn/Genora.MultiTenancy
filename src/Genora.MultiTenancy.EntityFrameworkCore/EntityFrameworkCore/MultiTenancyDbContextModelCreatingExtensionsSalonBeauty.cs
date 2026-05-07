using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public static class MultiTenancyDbContextModelCreatingExtensionsSalonBeauty
{
    public static void ConfigureSalonBeautyModule(this ModelBuilder builder)
    {
        var schema = "Salon";

        builder.Entity<SalonBeautyCustomer>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyCustomers", schema);
            b.ConfigureByConvention();

            b.Property(x => x.CustomerCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Phone).HasMaxLength(15);
            b.Property(x => x.Email).HasMaxLength(255);
            b.Property(x => x.Avatar).HasMaxLength(500);
            b.Property(x => x.ZaloUserId).HasMaxLength(100);
            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.Status).HasDefaultValue((byte)1);

            b.HasIndex(x => new { x.TenantId, x.CustomerCode })
                .IsUnique()
                .HasDatabaseName("IX_AppSalonBeautyCustomers_TenantId_Code");

            b.HasIndex(x => new { x.TenantId, x.Phone })
                .HasDatabaseName("IX_AppSalonBeautyCustomers_TenantId_Phone");

            b.HasIndex(x => new { x.TenantId, x.Email })
                .HasDatabaseName("IX_AppSalonBeautyCustomers_TenantId_Email");
        });

        builder.Entity<SalonBeautyServiceCategory>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyServiceCategories", schema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.Status).HasDefaultValue((byte)1);

            b.HasIndex(x => new { x.TenantId, x.Name })
                .IsUnique()
                .HasDatabaseName("IX_AppSalonBeautyServiceCategories_TenantId_Name");
        });

        builder.Entity<SalonBeautyService>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyServices", schema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Price).HasPrecision(18, 2);
            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.Status).HasDefaultValue((byte)1);

            b.HasOne(x => x.Category)
                .WithMany(x => x.Services)
                .HasForeignKey(x => x.CategoryId)
                .IsRequired();

            b.HasIndex(x => new { x.TenantId, x.CategoryId })
                .HasDatabaseName("IX_AppSalonBeautyServices_TenantId_CategoryId");
        });

        builder.Entity<SalonBeautyStylist>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyStylists", schema);
            b.ConfigureByConvention();

            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(255);
            b.Property(x => x.Avatar).HasMaxLength(500);
            b.Property(x => x.Phone).HasMaxLength(15);
            b.Property(x => x.RatingAvg).HasPrecision(2, 1);
            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.Status).HasDefaultValue((byte)1);

            b.HasIndex(x => new { x.TenantId, x.DisplayName })
                .HasDatabaseName("IX_AppSalonBeautyStylists_TenantId_DisplayName");
        });

        builder.Entity<SalonBeautyBooking>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyBookings", schema);
            b.ConfigureByConvention();

            b.Property(x => x.BookingCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.TotalAmount).HasPrecision(18, 2);
            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.CancelNote).HasMaxLength(500);
            b.Property(x => x.Status).HasDefaultValue(SalonBeautyBookingStatus.New);
            b.Property(x => x.PaymentStatus).HasDefaultValue(SalonBeautyPaymentStatus.Unpaid);
            b.Property(x => x.CheckinStatus).HasDefaultValue(SalonBeautyCheckinStatus.NotCheckedIn);

            b.HasOne(x => x.Customer)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.CustomerId)
                .IsRequired();

            b.HasOne(x => x.Service)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.ServiceId)
                .IsRequired();

            b.HasOne(x => x.Stylist)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.StylistId)
                .IsRequired();

            b.HasIndex(x => new { x.TenantId, x.BookingCode })
                .IsUnique()
                .HasDatabaseName("IX_AppSalonBeautyBookings_TenantId_Code");

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .HasDatabaseName("IX_AppSalonBeautyBookings_TenantId_CustomerId");

            b.HasIndex(x => new { x.TenantId, x.BookingDate })
                .HasDatabaseName("IX_AppSalonBeautyBookings_TenantId_BookingDate");
        });

        builder.Entity<SalonBeautyBookingService>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyBookingServices", schema);
            b.ConfigureByConvention();

            b.Property(x => x.Price).HasPrecision(18, 2);

            b.HasOne(x => x.Booking)
                .WithMany(x => x.BookingServices)
                .HasForeignKey(x => x.BookingId)
                .IsRequired();

            b.HasOne(x => x.Service)
                .WithMany(x => x.BookingServices)
                .HasForeignKey(x => x.ServiceId)
                .IsRequired();

            b.HasIndex(x => x.BookingId)
                .HasDatabaseName("IX_AppSalonBeautyBookingServices_BookingId");
        });

        builder.Entity<SalonBeautyCustomerLoyaltyBalance>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyCustomerLoyaltyBalances", schema);
            b.ConfigureByConvention();

            b.Property(x => x.CurrentPoint).HasDefaultValue(0);

            b.HasOne(x => x.Customer)
                .WithMany(x => x.LoyaltyBalances)
                .HasForeignKey(x => x.CustomerId)
                .IsRequired();

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .IsUnique()
                .HasDatabaseName("IX_AppSalonBeautyCustomerLoyaltyBalances_TenantId_CustomerId");
        });

        builder.Entity<SalonBeautyCustomerLoyaltyTransaction>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyCustomerLoyaltyTransactions", schema);
            b.ConfigureByConvention();

            b.Property(x => x.Description).HasMaxLength(255);

            b.HasOne(x => x.Customer)
                .WithMany(x => x.LoyaltyTransactions)
                .HasForeignKey(x => x.CustomerId)
                .IsRequired();

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .HasDatabaseName("IX_AppSalonBeautyCustomerLoyaltyTransactions_TenantId_CustomerId");
        });
    }
}
