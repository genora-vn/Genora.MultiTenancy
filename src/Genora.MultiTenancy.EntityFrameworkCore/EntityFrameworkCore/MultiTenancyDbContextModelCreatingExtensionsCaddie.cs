using Genora.MultiTenancy.DomainModels.AppCaddie;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Genora.MultiTenancy.EntityFrameworkCore;

public static class MultiTenancyDbContextModelCreatingExtensionsCaddie
{
    public static void ConfigureCaddieModule(this ModelBuilder builder)
    {
        builder.Entity<AppLanguage>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "Languages");
            b.ConfigureByConvention();

            b.Property(x => x.LanguageCode).IsRequired().HasMaxLength(20);
            b.Property(x => x.LanguageName).IsRequired().HasMaxLength(100);
            b.Property(x => x.NativeName).HasMaxLength(100);
            b.Property(x => x.Status).HasDefaultValue((byte)1);

            b.HasIndex(x => new { x.TenantId, x.LanguageCode })
                .IsUnique()
                .HasDatabaseName("IX_AppLanguages_TenantId_Code");
        });

        builder.Entity<AppCaddieSkill>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CaddieSkills");
            b.ConfigureByConvention();

            b.Property(x => x.SkillCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.SkillName).IsRequired().HasMaxLength(255);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.Status).HasDefaultValue((byte)1);

            b.HasIndex(x => new { x.TenantId, x.SkillCode })
                .IsUnique()
                .HasDatabaseName("IX_AppCaddieSkills_TenantId_Code");
        });

        builder.Entity<AppCaddie>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "Caddies");
            b.ConfigureByConvention();

            b.Property(x => x.CaddieCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.CaddieName).IsRequired().HasMaxLength(255);
            b.Property(x => x.Avatar).HasMaxLength(500);
            b.Property(x => x.Phone).HasMaxLength(20);
            b.Property(x => x.RatingAvg).HasPrecision(3, 1);
            b.Property(x => x.Status).HasDefaultValue((byte)1);

            b.HasIndex(x => new { x.TenantId, x.CaddieCode })
                .IsUnique()
                .HasDatabaseName("IX_AppCaddies_TenantId_Code");

            b.HasIndex(x => new { x.TenantId, x.GolfCourseId })
                .HasDatabaseName("IX_AppCaddies_TenantId_GolfCourseId");
        });

        builder.Entity<AppCaddieLanguage>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CaddieLanguages");
            b.ConfigureByConvention();

            b.HasOne(x => x.Caddie)
                .WithMany(x => x.Languages)
                .HasForeignKey(x => x.CaddieId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Language)
                .WithMany()
                .HasForeignKey(x => x.LanguageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.CaddieId, x.LanguageId })
                .IsUnique()
                .HasDatabaseName("IX_AppCaddieLanguages_TenantId_Caddie_Language");
        });

        builder.Entity<AppCaddieVoiceRegion>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CaddieVoiceRegions");
            b.ConfigureByConvention();

            b.HasOne(x => x.Caddie)
                .WithMany(x => x.VoiceRegions)
                .HasForeignKey(x => x.CaddieId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.CaddieId, x.VoiceRegion })
                .IsUnique()
                .HasDatabaseName("IX_AppCaddieVoiceRegions_TenantId_Caddie_Region");
        });

        builder.Entity<AppCaddieSchedule>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CaddieSchedules");
            b.ConfigureByConvention();

            b.Property(x => x.Note).HasMaxLength(1000);
            b.Property(x => x.SlotStatus).HasDefaultValue((byte)1);

            b.HasOne(x => x.Caddie)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.CaddieId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.TenantId, x.CaddieId, x.WorkDate, x.ShiftCode })
                .IsUnique()
                .HasDatabaseName("IX_AppCaddieSchedules_TenantId_Caddie_Date_Shift");
        });

        builder.Entity<AppCaddieBooking>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CaddieBookings");
            b.ConfigureByConvention();

            b.Property(x => x.BookingCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.CustomerName).IsRequired().HasMaxLength(255);
            b.Property(x => x.Phone).IsRequired().HasMaxLength(20);
            b.Property(x => x.Note).HasMaxLength(1000);
            b.Property(x => x.CancelReason).HasMaxLength(1000);
            b.Property(x => x.Status).HasDefaultValue((byte)1);
            b.Property(x => x.PaymentStatus).HasDefaultValue((byte)1);
            b.Property(x => x.CheckinStatus).HasDefaultValue((byte)1);
            b.Property(x => x.PaymentMethod).HasDefaultValue((byte)0);
            b.Property(x => x.TotalCaddieFee).HasColumnType("decimal(18,2)").HasDefaultValue(0m);

            b.HasIndex(x => new { x.TenantId, x.BookingCode })
                .IsUnique()
                .HasDatabaseName("IX_AppCaddieBookings_TenantId_Code");

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .HasDatabaseName("IX_AppCaddieBookings_TenantId_CustomerId");
        });

        builder.Entity<AppCaddieRating>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CaddieRatings");
            b.ConfigureByConvention();

            b.Property(x => x.Comment).HasMaxLength(2000);
            b.Property(x => x.RejectReason).HasMaxLength(1000);
            b.Property(x => x.ApprovalStatus).HasDefaultValue((byte)1);

            b.HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Caddie)
                .WithMany()
                .HasForeignKey(x => x.CaddieId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.BookingId })
                .IsUnique()
                .HasDatabaseName("IX_AppCaddieRatings_TenantId_BookingId");

            b.HasIndex(x => new { x.TenantId, x.CaddieId, x.ApprovalStatus })
                .HasDatabaseName("IX_AppCaddieRatings_TenantId_Caddie_Status");
        });

        builder.Entity<AppCaddieRatingDetail>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CaddieRatingDetails");
            b.ConfigureByConvention();

            b.HasOne(x => x.Rating)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.RatingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Skill)
                .WithMany()
                .HasForeignKey(x => x.SkillId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.RatingId, x.SkillId })
                .IsUnique()
                .HasDatabaseName("IX_AppCaddieRatingDetails_TenantId_Rating_Skill");
        });

        builder.Entity<AppCaddieBookingDetail>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "CaddieBookingDetails");
            b.ConfigureByConvention();

            b.Property(x => x.Note).HasMaxLength(500);
            b.Property(x => x.Status).HasDefaultValue((byte)1);

            b.HasOne(x => x.CaddieBooking)
                .WithMany()
                .HasForeignKey(x => x.CaddieBookingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Caddie)
                .WithMany()
                .HasForeignKey(x => x.CaddieId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Schedule)
                .WithMany()
                .HasForeignKey(x => x.ScheduleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.CaddieBookingId })
                .HasDatabaseName("IX_AppCaddieBookingDetails_TenantId_BookingId");

            b.HasIndex(x => new { x.TenantId, x.CaddieId })
                .HasDatabaseName("IX_AppCaddieBookingDetails_TenantId_CaddieId");
        });
    }
}
