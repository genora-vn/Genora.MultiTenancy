using Genora.MultiTenancy.DomainModels.AppHlg;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Genora.MultiTenancy.EntityFrameworkCore;

/// <summary>
/// EF Core config cho module Hoa Linh Gamification (schema "HLG").
/// Tách biệt hoàn toàn với module Hoa Linh (schema "HL") hiện tại.
/// </summary>
public static class MultiTenancyDbContextModelCreatingExtensionsHlg
{
    public static void ConfigureHlgModule(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // ========== HlgUserProfile ==========
        builder.Entity<HlgUserProfile>(b =>
        {
            b.ToTable("AppHlgUserProfiles", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.ZaloId).HasMaxLength(100);
            b.Property(x => x.CustomerType).HasConversion<byte?>();

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .IsUnique()
                .HasDatabaseName("IX_AppHlgUserProfiles_TenantId_CustomerId");

            b.HasIndex(x => new { x.TenantId, x.ZaloId })
                .HasDatabaseName("IX_AppHlgUserProfiles_TenantId_ZaloId");
        });

        // ========== HlgKnowledgeCategory ==========
        builder.Entity<HlgKnowledgeCategory>(b =>
        {
            b.ToTable("AppHlgKnowledgeCategories", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(250);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.ImageUrl).HasMaxLength(1000);

            b.HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.DisplayOrder })
                .HasDatabaseName("IX_AppHlgKnowledgeCategories_TenantId_DisplayOrder");
        });

        // ========== HlgProduct ==========
        builder.Entity<HlgProduct>(b =>
        {
            b.ToTable("AppHlgProducts", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(250);
            b.Property(x => x.ThumbnailUrl).HasMaxLength(1000);
            b.Property(x => x.Summary).HasMaxLength(1000);

            b.HasIndex(x => new { x.TenantId, x.CategoryId })
                .HasDatabaseName("IX_AppHlgProducts_TenantId_CategoryId");
        });

        // ========== HlgLearningProgress ==========
        builder.Entity<HlgLearningProgress>(b =>
        {
            b.ToTable("AppHlgLearningProgress", "HLG");
            b.ConfigureByConvention();

            b.HasIndex(x => new { x.TenantId, x.CustomerId, x.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_AppHlgLearningProgress_TenantId_CustomerId_ProductId");
        });

        // ========== HlgGame ==========
        builder.Entity<HlgGame>(b =>
        {
            b.ToTable("AppHlgGames", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(250);
            b.Property(x => x.Type).HasConversion<byte>();
            b.Property(x => x.Status).HasConversion<byte>();
            b.Property(x => x.ImageUrl).HasMaxLength(1000);

            b.HasMany(x => x.Questions)
                .WithOne(x => x.Game)
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.Status, x.DisplayOrder })
                .HasDatabaseName("IX_AppHlgGames_TenantId_Status_DisplayOrder");
        });

        // ========== HlgQuestion ==========
        builder.Entity<HlgQuestion>(b =>
        {
            b.ToTable("AppHlgQuestions", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.Content).IsRequired();
            b.Property(x => x.ImageUrl).HasMaxLength(1000);
            b.Property(x => x.ScoreMultiplier).HasColumnType("decimal(9,2)");
            b.Property(x => x.CorrectKey).HasConversion<byte>();

            b.HasMany(x => x.Options)
                .WithOne(x => x.Question)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.GameId, x.Index })
                .HasDatabaseName("IX_AppHlgQuestions_TenantId_GameId_Index");
        });

        // ========== HlgAnswerOption ==========
        builder.Entity<HlgAnswerOption>(b =>
        {
            b.ToTable("AppHlgAnswerOptions", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.Content).IsRequired();
            b.Property(x => x.Key).HasConversion<byte>();

            b.HasIndex(x => new { x.TenantId, x.QuestionId })
                .HasDatabaseName("IX_AppHlgAnswerOptions_TenantId_QuestionId");
        });

        // ========== HlgGameSession ==========
        builder.Entity<HlgGameSession>(b =>
        {
            b.ToTable("AppHlgGameSessions", "HLG");
            b.ConfigureByConvention();

            b.HasMany(x => x.Answers)
                .WithOne(x => x.Session)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.CustomerId, x.GameId })
                .HasDatabaseName("IX_AppHlgGameSessions_TenantId_CustomerId_GameId");
        });

        // ========== HlgSessionAnswer ==========
        builder.Entity<HlgSessionAnswer>(b =>
        {
            b.ToTable("AppHlgSessionAnswers", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.SelectedKey).HasConversion<byte>();

            b.HasIndex(x => new { x.TenantId, x.SessionId })
                .HasDatabaseName("IX_AppHlgSessionAnswers_TenantId_SessionId");
        });

        // ========== HlgReward ==========
        builder.Entity<HlgReward>(b =>
        {
            b.ToTable("AppHlgRewards", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(250);
            b.Property(x => x.ImageUrl).HasMaxLength(1000);
            b.Property(x => x.Type).HasConversion<byte>();
            b.Property(x => x.VoucherCode).HasMaxLength(100);

            b.HasIndex(x => new { x.TenantId, x.IsActive, x.DisplayOrder })
                .HasDatabaseName("IX_AppHlgRewards_TenantId_IsActive_DisplayOrder");
        });

        // ========== HlgRewardHistory ==========
        builder.Entity<HlgRewardHistory>(b =>
        {
            b.ToTable("AppHlgRewardHistories", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.RewardName).IsRequired().HasMaxLength(250);
            b.Property(x => x.RewardType).HasConversion<byte>();
            b.Property(x => x.Status).HasConversion<byte>();
            b.Property(x => x.VoucherCode).HasMaxLength(100);

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .HasDatabaseName("IX_AppHlgRewardHistories_TenantId_CustomerId");
        });

        // ========== HlgShippingAddress ==========
        builder.Entity<HlgShippingAddress>(b =>
        {
            b.ToTable("AppHlgShippingAddresses", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.ReceiverName).IsRequired().HasMaxLength(150);
            b.Property(x => x.Phone).IsRequired().HasMaxLength(20);
            b.Property(x => x.Address).IsRequired().HasMaxLength(500);
            b.Property(x => x.Note).HasMaxLength(500);

            b.HasIndex(x => new { x.TenantId, x.CustomerId })
                .HasDatabaseName("IX_AppHlgShippingAddresses_TenantId_CustomerId");
        });

        // ========== HlgRankingEvent ==========
        builder.Entity<HlgRankingEvent>(b =>
        {
            b.ToTable("AppHlgRankingEvents", "HLG");
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(250);

            b.HasIndex(x => new { x.TenantId, x.IsActive, x.StartAt })
                .HasDatabaseName("IX_AppHlgRankingEvents_TenantId_IsActive_StartAt");
        });
    }
}
