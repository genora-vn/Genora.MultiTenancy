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

            b.HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.DisplayName })
                .HasDatabaseName("IX_AppSalonBeautyStylists_TenantId_DisplayName");

            b.HasIndex(x => new { x.TenantId, x.LocationId })
                .HasDatabaseName("IX_AppSalonBeautyStylists_TenantId_LocationId");
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

            b.HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.TimeSlot)
                .WithMany()
                .HasForeignKey(x => x.TimeSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.BookingCode })
                .IsUnique()
                .HasDatabaseName("IX_AppSalonBeautyBookings_TenantId_Code");

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .HasDatabaseName("IX_AppSalonBeautyBookings_TenantId_CustomerId");

            b.HasIndex(x => new { x.TenantId, x.BookingDate })
                .HasDatabaseName("IX_AppSalonBeautyBookings_TenantId_BookingDate");

            b.HasIndex(x => new { x.TenantId, x.LocationId })
                .HasDatabaseName("IX_AppSalonBeautyBookings_TenantId_LocationId");

            b.HasIndex(x => new { x.TenantId, x.TimeSlotId })
                .HasDatabaseName("IX_AppSalonBeautyBookings_TenantId_TimeSlotId");
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
            b.Property(x => x.BalanceBefore).HasDefaultValue(0);
            b.Property(x => x.BalanceAfter).HasDefaultValue(0);
            b.Property(x => x.ReferenceType).HasDefaultValue((byte)99);

            b.HasOne(x => x.Customer)
                .WithMany(x => x.LoyaltyTransactions)
                .HasForeignKey(x => x.CustomerId)
                .IsRequired();

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .HasDatabaseName("IX_AppSalonBeautyCustomerLoyaltyTransactions_TenantId_CustomerId");

            b.HasIndex(x => new { x.TenantId, x.ReferenceType, x.ReferenceId })
                .HasDatabaseName("IX_AppSalonBeautyCustomerLoyaltyTransactions_TenantId_Ref");
        });

        builder.Entity<SalonBeautyDepositTransaction>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyDepositTransactions", schema);
            b.ConfigureByConvention();

            b.Property(x => x.TransactionCode).IsRequired().HasMaxLength(30);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.ExchangeRate).HasPrecision(18, 4);
            b.Property(x => x.ReferenceCode).HasMaxLength(100);
            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.CancelReason).HasMaxLength(500);
            b.Property(x => x.Status).HasDefaultValue((byte)1);

            b.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasIndex(x => new { x.TenantId, x.TransactionCode })
                .IsUnique()
                .HasDatabaseName("IX_AppSalonBeautyDepositTransactions_TenantId_Code");

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .HasDatabaseName("IX_AppSalonBeautyDepositTransactions_TenantId_CustomerId");

            b.HasIndex(x => new { x.TenantId, x.Status })
                .HasDatabaseName("IX_AppSalonBeautyDepositTransactions_TenantId_Status");
        });

        builder.Entity<SalonBeautyLoyaltyBonusTier>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyLoyaltyBonusTiers", schema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.MinAmount).HasPrecision(18, 2);
            b.Property(x => x.Description).HasMaxLength(255);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.MinAmount })
                .HasDatabaseName("IX_AppSalonBeautyLoyaltyBonusTiers_TenantId_MinAmount");
        });

        builder.Entity<SalonBeautyLocation>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyLocations", schema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Address).IsRequired().HasMaxLength(500);
            b.Property(x => x.Phone).HasMaxLength(15);
            b.Property(x => x.ImageUrl).HasMaxLength(500);
            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.SlotDuration).HasDefaultValue(60);
            b.Property(x => x.BufferTime).HasDefaultValue(0);
            b.Property(x => x.MaxCapacityPerSlot).HasDefaultValue(1);

            b.HasIndex(x => new { x.TenantId, x.Name })
                .HasDatabaseName("IX_AppSalonBeautyLocations_TenantId_Name");
        });

        builder.Entity<SalonBeautyTimeSlot>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "SalonBeautyTimeSlots", schema);
            b.ConfigureByConvention();

            b.Property(x => x.Status)
                .HasDefaultValue(SalonBeautyTimeSlotStatus.On)
                .HasConversion<byte>();
            b.Property(x => x.IsShowOnApp).HasDefaultValue(true);
            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.Capacity).HasDefaultValue(1);
            b.Property(x => x.BookedCount).HasDefaultValue(0);
            b.Property(x => x.IsManualOverride).HasDefaultValue(false);

            b.HasOne(x => x.Location)
                .WithMany(x => x.TimeSlots)
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne(x => x.Stylist)
                .WithMany()
                .HasForeignKey(x => x.StylistId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasIndex(x => new { x.TenantId, x.StylistId, x.WorkDate })
                .HasDatabaseName("IX_AppSalonBeautyTimeSlots_TenantId_StylistId_WorkDate");

            b.HasIndex(x => new { x.TenantId, x.LocationId, x.WorkDate })
                .HasDatabaseName("IX_AppSalonBeautyTimeSlots_TenantId_LocationId_WorkDate");
        });
    }
}
