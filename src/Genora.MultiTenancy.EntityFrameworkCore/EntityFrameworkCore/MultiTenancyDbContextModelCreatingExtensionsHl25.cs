using Genora.MultiTenancy.DomainModels.AppHl25;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Genora.MultiTenancy.EntityFrameworkCore;

/// <summary>
/// Cấu hình EF Core cho module "Dược Phẩm Hoa Linh 25 Năm" (schema "hl25").
/// </summary>
public static class MultiTenancyDbContextModelCreatingExtensionsHl25
{
    public static void ConfigureHl25Module(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // ========== Hl25AppConfig ==========
        builder.Entity<Hl25AppConfig>(b =>
        {
            b.ToTable("AppHl25AppConfig", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.ProgramName).HasMaxLength(256);
            b.Property(x => x.Format).HasMaxLength(512);
            b.Property(x => x.GiftDeliveryTime).HasMaxLength(512);
            b.Property(x => x.Scope).HasMaxLength(256);
            b.Property(x => x.OrganizerName).HasMaxLength(256);

            b.HasIndex(x => new { x.TenantId })
                .HasDatabaseName("IX_AppHl25AppConfig_TenantId");
        });

        // ========== Hl25FrameCampaign ==========
        builder.Entity<Hl25FrameCampaign>(b =>
        {
            b.ToTable("AppHl25FrameCampaigns", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.Status).HasConversion<byte>();

            b.HasMany(x => x.Templates)
                .WithOne(x => x.Campaign)
                .HasForeignKey(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.Status })
                .HasDatabaseName("IX_AppHl25FrameCampaigns_TenantId_Status");
        });

        // ========== Hl25FrameTemplate ==========
        builder.Entity<Hl25FrameTemplate>(b =>
        {
            b.ToTable("AppHl25FrameTemplates", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.ImageUrl).IsRequired().HasMaxLength(1024);
            b.Property(x => x.ThumbnailUrl).HasMaxLength(1024);

            b.HasIndex(x => new { x.TenantId, x.CampaignId })
                .HasDatabaseName("IX_AppHl25FrameTemplates_TenantId_CampaignId");
        });

        // ========== Hl25FrameCreation ==========
        builder.Entity<Hl25FrameCreation>(b =>
        {
            b.ToTable("AppHl25FrameCreations", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.ResultImageUrl).IsRequired().HasMaxLength(1024);
            b.Property(x => x.WishMessage).HasMaxLength(250);
            b.Property(x => x.ShareLink).HasMaxLength(1024);
            b.Property(x => x.SharePlatform).HasConversion<byte>();

            b.HasOne(x => x.Participant)
                .WithMany()
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.ParticipantId })
                .HasDatabaseName("IX_AppHl25FrameCreations_TenantId_ParticipantId");

            b.HasIndex(x => new { x.TenantId, x.ParticipantId, x.CreatedTime })
                .HasDatabaseName("IX_AppHl25FrameCreations_TenantId_ParticipantId_CreatedTime");

            b.HasIndex(x => new { x.TenantId, x.CampaignId })
                .HasDatabaseName("IX_AppHl25FrameCreations_TenantId_CampaignId");

            b.HasIndex(x => new { x.TenantId, x.CreatedTime })
                .HasDatabaseName("IX_AppHl25FrameCreations_TenantId_CreatedTime");
        });

        // ========== Hl25WheelConfig ==========
        builder.Entity<Hl25WheelConfig>(b =>
        {
            b.ToTable("AppHl25WheelConfig", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.Title).HasMaxLength(256);
            b.Property(x => x.SubTitle).HasMaxLength(512);
            b.Property(x => x.PrimaryColor).HasMaxLength(16);
            b.Property(x => x.SecondaryColor).HasMaxLength(16);
            b.Property(x => x.BackgroundImageUrl).HasMaxLength(1024);
            b.Property(x => x.PointerImageUrl).HasMaxLength(1024);

            b.HasMany(x => x.Slots)
                .WithOne(x => x.WheelConfig)
                .HasForeignKey(x => x.WheelConfigId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId })
                .HasDatabaseName("IX_AppHl25WheelConfig_TenantId");
        });

        // ========== Hl25WheelSlot ==========
        builder.Entity<Hl25WheelSlot>(b =>
        {
            b.ToTable("AppHl25WheelSlots", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.Label).HasMaxLength(256);
            b.Property(x => x.SlotImageUrl).HasMaxLength(1024);
            b.Property(x => x.WinRate).HasColumnType("decimal(9,4)");
            b.Property(x => x.ColorHex).HasMaxLength(16);

            b.HasIndex(x => new { x.TenantId, x.WheelConfigId })
                .HasDatabaseName("IX_AppHl25WheelSlots_TenantId_WheelConfigId");
        });

        // ========== Hl25Gift ==========
        builder.Entity<Hl25Gift>(b =>
        {
            b.ToTable("AppHl25Gifts", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.ImageUrl).HasMaxLength(1024);
            b.Property(x => x.WheelImageUrl).HasMaxLength(1024);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.Value).HasColumnType("decimal(18,2)");
            b.Property(x => x.Status).HasConversion<byte>();

            b.HasIndex(x => new { x.TenantId, x.Status })
                .HasDatabaseName("IX_AppHl25Gifts_TenantId_Status");
        });

        // ========== Hl25SpinTurnLog ==========
        builder.Entity<Hl25SpinTurnLog>(b =>
        {
            b.ToTable("AppHl25SpinTurnLogs", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.Source).HasConversion<byte>();
            b.Property(x => x.Note).HasMaxLength(512);

            b.HasOne(x => x.Participant)
                .WithMany()
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.ParticipantId })
                .HasDatabaseName("IX_AppHl25SpinTurnLogs_TenantId_ParticipantId");

            b.HasIndex(x => new { x.TenantId, x.GrantedTime })
                .HasDatabaseName("IX_AppHl25SpinTurnLogs_TenantId_GrantedTime");
        });

        // ========== Hl25SpinLog ==========
        builder.Entity<Hl25SpinLog>(b =>
        {
            b.ToTable("AppHl25SpinLogs", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.GiftNameSnapshot).HasMaxLength(256);
            b.Property(x => x.RewardStatus).HasConversion<byte>();
            b.Property(x => x.ReceiverAddressSnapshot).HasMaxLength(512);

            b.HasOne(x => x.Participant)
                .WithMany()
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.ParticipantId })
                .HasDatabaseName("IX_AppHl25SpinLogs_TenantId_ParticipantId");

            b.HasIndex(x => new { x.TenantId, x.ParticipantId, x.SpinTime })
                .HasDatabaseName("IX_AppHl25SpinLogs_TenantId_ParticipantId_SpinTime");

            b.HasIndex(x => new { x.TenantId, x.GiftId })
                .HasDatabaseName("IX_AppHl25SpinLogs_TenantId_GiftId");

            b.HasIndex(x => new { x.TenantId, x.SpinTime })
                .HasDatabaseName("IX_AppHl25SpinLogs_TenantId_SpinTime");

            b.HasIndex(x => new { x.TenantId, x.RewardStatus })
                .HasDatabaseName("IX_AppHl25SpinLogs_TenantId_RewardStatus");
        });

        // ========== Hl25Participant ==========
        builder.Entity<Hl25Participant>(b =>
        {
            b.ToTable("AppHl25Participants", "hl25");
            b.ConfigureByConvention();

            b.Property(x => x.ZaloUserId).HasMaxLength(64);
            b.Property(x => x.FullName).HasMaxLength(256);
            b.Property(x => x.PhoneNumber).HasMaxLength(13);
            b.Property(x => x.Gender).HasConversion<byte>();
            b.Property(x => x.ReceiveAddress).HasMaxLength(1024);
            b.Property(x => x.AvatarUrl).HasMaxLength(1024);

            b.HasIndex(x => new { x.TenantId, x.ZaloUserId })
                .IsUnique()
                .HasFilter("[TenantId] IS NOT NULL AND [ZaloUserId] IS NOT NULL")
                .HasDatabaseName("IX_AppHl25Participants_TenantId_ZaloUserId");

            b.HasIndex(x => new { x.TenantId, x.PhoneNumber })
                .HasDatabaseName("IX_AppHl25Participants_TenantId_PhoneNumber");

            b.HasIndex(x => new { x.TenantId, x.PhoneNumber, x.IsDeleted })
                .HasDatabaseName("IX_AppHl25Participants_TenantId_PhoneNumber_IsDeleted");

            b.HasIndex(x => new { x.TenantId, x.JoinedTime })
                .HasDatabaseName("IX_AppHl25Participants_TenantId_JoinedTime");
        });
    }
}
