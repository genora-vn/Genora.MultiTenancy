using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppBookingStatusHistories;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomerMemberships;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppNews;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public static class MultiTenancyDbContextModelCreatingExtensionsMiniApp
{
    public static void ConfigureMiniAppModule(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // ==== CustomerType ====
        builder.Entity<CustomerType>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CustomerTypes", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(500);

            // ColorCode dạng #RRGGBB
            b.Property(x => x.ColorCode).HasColumnType("char(7)");

            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique()
                .HasDatabaseName("IX_AppCustomerTypes_TenantId_Code");

            // Check constraint cho phép NULL hoặc mã màu hex hợp lệ
            b.HasCheckConstraint(
                "CK_AppCustomerTypes_ColorCode",
                @"[ColorCode] IS NULL 
          OR (
              LEN([ColorCode]) = 7 
              AND [ColorCode] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'
          )");
        });


        // ==== GolfCourse ====
        builder.Entity<GolfCourse>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "GolfCourses", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Address).HasMaxLength(255);
            b.Property(x => x.Province).HasMaxLength(100);
            b.Property(x => x.Phone).HasMaxLength(20);
            b.Property(x => x.Website).HasMaxLength(255);
            b.Property(x => x.FanpageUrl).HasMaxLength(255);
            b.Property(x => x.ShortDescription).HasMaxLength(500);
            b.Property(x => x.AvatarUrl).HasMaxLength(500);
            b.Property(x => x.BannerUrl).HasMaxLength(500);
            b.Property(x => x.BookingStatus).HasDefaultValue((byte)1);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique()
                .HasDatabaseName("IX_AppGolfCourses_TenantId_Code");
        });

        // ==== MembershipTier ====
        builder.Entity<MembershipTier>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "MembershipTiers", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.DisplayOrder).HasDefaultValue(0);

            b.HasIndex(x => new { x.TenantId, x.Code })
                .IsUnique()
                .HasDatabaseName("IX_AppMembershipTiers_TenantId_Code");
        });

        // ==== Customer ====
        builder.Entity<Customer>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "Customers", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            b.Property(x => x.AvatarUrl).HasMaxLength(500);
            b.Property(x => x.CustomerCode).HasMaxLength(50);
            b.Property(x => x.ZaloUserId).HasMaxLength(100);
            b.Property(x => x.ZaloFollowerId).HasMaxLength(100);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasOne(x => x.CustomerType)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.CustomerTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => new { x.TenantId, x.PhoneNumber })
                .IsUnique()
                .HasDatabaseName("IX_AppCustomers_TenantId_PhoneNumber");

            b.HasIndex(x => new { x.TenantId, x.CustomerCode })
                .IsUnique()
                .HasFilter("[CustomerCode] IS NOT NULL")
                .HasDatabaseName("IX_AppCustomers_TenantId_CustomerCode");
        });

        // ==== CalendarSlot ====
        builder.Entity<CalendarSlot>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CalendarSlots", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.InternalNote).HasMaxLength(500);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasOne(x => x.GolfCourse)
                .WithMany(x => x.CalendarSlots)
                .HasForeignKey(x => x.GolfCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.GolfCourseId, x.ApplyDate, x.TimeFrom, x.TimeTo })
                .IsUnique()
                .HasDatabaseName("IX_AppCalendarSlots_CourseDateTime");
        });

        // ==== CalendarSlotPrice ====
        builder.Entity<CalendarSlotPrice>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CalendarSlotPrices", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Price9).HasColumnType("decimal(18,2)").IsRequired(false);
            b.Property(x => x.Price18).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(x => x.Price27).HasColumnType("decimal(18,2)").IsRequired(false);
            b.Property(x => x.Price36).HasColumnType("decimal(18,2)").IsRequired(false);

            b.HasOne(x => x.CalendarSlot)
                .WithMany(x => x.Prices)
                .HasForeignKey(x => x.CalendarSlotId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.CustomerType)
                .WithMany(x => x.CalendarSlotPrices)
                .HasForeignKey(x => x.CustomerTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.CalendarSlotId, x.CustomerTypeId })
                .IsUnique()
                .HasDatabaseName("IX_AppCalendarSlotPrices_Slot_CustomerType");
        });

        // ==== News ====
        builder.Entity<News>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "News", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(255);
            b.Property(x => x.ContentHtml).IsRequired();
            b.Property(x => x.ThumbnailUrl).HasMaxLength(500);
            b.Property(x => x.Status).HasDefaultValue((byte)0);
            b.Property(x => x.DisplayOrder).HasDefaultValue(0);

            b.HasOne(x => x.GolfCourse)
                .WithMany(x => x.News)
                .HasForeignKey(x => x.GolfCourseId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ==== CustomerMembership ====
        builder.Entity<CustomerMembership>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CustomerMemberships", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.IsCurrent).HasDefaultValue(false);

            b.HasOne(x => x.Customer)
                .WithMany(x => x.Memberships)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.MembershipTier)
                .WithMany(x => x.CustomerMemberships)
                .HasForeignKey(x => x.MembershipTierId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.CustomerId, x.IsCurrent })
                .IsUnique()
                .HasFilter("[IsCurrent] = 1")
                .HasDatabaseName("IX_AppCustomerMemberships_Customer_IsCurrent");
        });

        // ==== Booking ====
        builder.Entity<Booking>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "Bookings", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.BookingCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.PricePerGolfer).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");

            b.HasOne(x => x.Customer)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.GolfCourse)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.GolfCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.CalendarSlot)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.CalendarSlotId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => new { x.TenantId, x.BookingCode })
                .IsUnique()
                .HasDatabaseName("IX_AppBookings_TenantId_BookingCode");

            b.HasIndex(x => new { x.CustomerId, x.PlayDate })
                .HasDatabaseName("IX_AppBookings_Customer_PlayDate");

            b.HasIndex(x => new { x.Status, x.PlayDate })
                .HasDatabaseName("IX_AppBookings_Status_PlayDate");
        });

        // ==== BookingPlayer ====
        builder.Entity<BookingPlayer>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "BookingPlayers", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.PlayerName).IsRequired().HasMaxLength(150);
            b.Property(x => x.Notes).HasMaxLength(500);

            b.HasOne(x => x.Booking)
                .WithMany(x => x.Players)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Customer)
                .WithMany(x => x.BookingPlayers)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ==== BookingStatusHistory ====
        builder.Entity<BookingStatusHistory>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "BookingStatusHistories", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ChangedAt).IsRequired();
            b.Property(x => x.ChangedBy).HasMaxLength(100);

            b.HasOne(x => x.Booking)
                .WithMany(x => x.StatusHistories)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.BookingId, x.ChangedAt })
                .HasDatabaseName("IX_AppBookingStatusHistories_BookingId_ChangedAt");
        });
    }
}
