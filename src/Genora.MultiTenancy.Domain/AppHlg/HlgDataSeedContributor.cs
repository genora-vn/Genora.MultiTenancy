using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums.Hlg;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace Genora.MultiTenancy.AppHlg;

/// <summary>
/// Seed dữ liệu mẫu cho module Hoa Linh Gamification (chạy trong context tenant).
/// - Chỉ seed khi feature `Hlg.Management` bật cho tenant (tránh làm bẩn tenant khác).
/// - Idempotent: chỉ seed khi bảng Games rỗng.
/// - Dữ liệu mẫu: 1 danh mục kiến thức + 2 bài học, 1 game quiz + 3 câu hỏi, 2 quà, 1 sự kiện xếp hạng.
/// Feature name để string vì Domain layer không reference Application.Contracts (giống pattern AppDocuments).
/// </summary>
public class HlgDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string FeatHlg = "Hlg.Management";

    private readonly IRepository<HlgKnowledgeCategory, Guid> _categoryRepo;
    private readonly IRepository<HlgProduct, Guid> _productRepo;
    private readonly IRepository<HlgGame, Guid> _gameRepo;
    private readonly IRepository<HlgQuestion, Guid> _questionRepo;
    private readonly IRepository<HlgAnswerOption, Guid> _optionRepo;
    private readonly IRepository<HlgReward, Guid> _rewardRepo;
    private readonly IRepository<HlgRankingEvent, Guid> _rankingRepo;
    private readonly IFeatureChecker _featureChecker;
    private readonly IGuidGenerator _guid;
    private readonly IClock _clock;

    public HlgDataSeedContributor(
        IRepository<HlgKnowledgeCategory, Guid> categoryRepo,
        IRepository<HlgProduct, Guid> productRepo,
        IRepository<HlgGame, Guid> gameRepo,
        IRepository<HlgQuestion, Guid> questionRepo,
        IRepository<HlgAnswerOption, Guid> optionRepo,
        IRepository<HlgReward, Guid> rewardRepo,
        IRepository<HlgRankingEvent, Guid> rankingRepo,
        IFeatureChecker featureChecker,
        IGuidGenerator guid,
        IClock clock)
    {
        _categoryRepo = categoryRepo;
        _productRepo = productRepo;
        _gameRepo = gameRepo;
        _questionRepo = questionRepo;
        _optionRepo = optionRepo;
        _rewardRepo = rewardRepo;
        _rankingRepo = rankingRepo;
        _featureChecker = featureChecker;
        _guid = guid;
        _clock = clock;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Host (TenantId == null): seed luôn để test local trên tài khoản host.
        // Tenant: chỉ seed khi bật feature HLG (tránh làm bẩn tenant khác dùng chung DbMigrator).
        if (context.TenantId != null && !await _featureChecker.IsEnabledAsync(FeatHlg))
            return;

        // Idempotent: đã có game thì bỏ qua (không seed lại).
        if (await _gameRepo.AnyAsync()) return;

        var tenantId = context.TenantId; // null cho host — entity nhận TenantId null là đúng cho host

        // ── Knowledge base ──────────────────────────────────────────────────
        var category = await _categoryRepo.InsertAsync(
            new HlgKnowledgeCategory(_guid.Create(), "Kiến thức Dược phẩm", tenantId)
            {
                Description = "Kiến thức cơ bản về dược phẩm Hoa Linh",
                DisplayOrder = 1,
                IsActive = true
            }, autoSave: true);

        await _productRepo.InsertAsync(
            new HlgProduct(_guid.Create(), category.Id, "Sử dụng thuốc an toàn", tenantId)
            {
                Summary = "Hướng dẫn dùng thuốc đúng cách",
                Content = "<p>Nội dung bài học về sử dụng thuốc an toàn.</p>",
                DisplayOrder = 1,
                IsActive = true
            }, autoSave: true);

        await _productRepo.InsertAsync(
            new HlgProduct(_guid.Create(), category.Id, "Bảo quản dược phẩm", tenantId)
            {
                Summary = "Cách bảo quản thuốc đúng nhiệt độ",
                Content = "<p>Nội dung bài học về bảo quản dược phẩm.</p>",
                DisplayOrder = 2,
                IsActive = true
            }, autoSave: true);

        // ── Game quiz + câu hỏi ─────────────────────────────────────────────
        var game = await _gameRepo.InsertAsync(
            new HlgGame(_guid.Create(), "Đố vui Dược phẩm", HlgGameType.Quiz, tenantId)
            {
                Description = "Trả lời câu hỏi trắc nghiệm để tích điểm",
                Rules = "<p>Mỗi câu trả lời đúng được cộng điểm. Trả lời càng nhanh điểm càng cao.</p>",
                RewardDescription = "Tích điểm đổi quà hấp dẫn",
                Status = HlgGameStatus.Ongoing,
                StartAt = _clock.Now.Date,
                EndAt = _clock.Now.Date.AddMonths(1),
                BaseScorePerQuestion = 100,
                DisplayOrder = 1,
                IsActive = true
            }, autoSave: true);

        await SeedQuestionAsync(game.Id, tenantId, 1,
            "Thuốc nên được bảo quản ở đâu?",
            HlgAnswerKey.B,
            ("A", "Nơi ẩm ướt"), ("B", "Nơi khô ráo, thoáng mát"), ("C", "Dưới ánh nắng"), ("D", "Trong tủ lạnh đông"));

        await SeedQuestionAsync(game.Id, tenantId, 2,
            "Khi nào nên uống thuốc theo chỉ định?",
            HlgAnswerKey.C,
            ("A", "Khi nào nhớ thì uống"), ("B", "Uống gấp đôi nếu quên"), ("C", "Đúng liều, đúng giờ bác sĩ dặn"), ("D", "Chỉ uống khi đau"));

        await SeedQuestionAsync(game.Id, tenantId, 3,
            "Hạn sử dụng thuốc thể hiện điều gì?",
            HlgAnswerKey.A,
            ("A", "Thời điểm sau đó không nên dùng"), ("B", "Ngày sản xuất"), ("C", "Giá bán"), ("D", "Số lô"));

        // ── Rewards ─────────────────────────────────────────────────────────
        await _rewardRepo.InsertAsync(
            new HlgReward(_guid.Create(), "Voucher giảm giá 50k", HlgRewardType.Voucher, 500, tenantId)
            {
                PointCost = 500,
                StockQuantity = 100,
                DisplayOrder = 1,
                IsActive = true
            }, autoSave: true);

        await _rewardRepo.InsertAsync(
            new HlgReward(_guid.Create(), "Túi quà Hoa Linh", HlgRewardType.Physical, 1000, tenantId)
            {
                PointCost = 1000,
                StockQuantity = 50,
                DisplayOrder = 2,
                IsActive = true
            }, autoSave: true);

        // ── Ranking event ───────────────────────────────────────────────────
        await _rankingRepo.InsertAsync(
            new HlgRankingEvent(_guid.Create(), "Bảng xếp hạng tháng",
                _clock.Now.Date, _clock.Now.Date.AddMonths(1), tenantId)
            {
                Description = "Xếp hạng người chơi tích điểm cao nhất trong tháng",
                IsActive = true
            }, autoSave: true);
    }

    private async Task SeedQuestionAsync(
        Guid gameId, Guid? tenantId, int index, string content, HlgAnswerKey correctKey,
        params (string Key, string Content)[] options)
    {
        var question = await _questionRepo.InsertAsync(
            new HlgQuestion(_guid.Create(), gameId, index, content, tenantId)
            {
                TimeLimitSec = 30,
                ScoreMultiplier = 1m,
                CorrectKey = correctKey,
                IsActive = true
            }, autoSave: true);

        foreach (var (key, optContent) in options)
        {
            var answerKey = key switch
            {
                "A" => HlgAnswerKey.A,
                "B" => HlgAnswerKey.B,
                "C" => HlgAnswerKey.C,
                "D" => HlgAnswerKey.D,
                _ => HlgAnswerKey.A
            };
            await _optionRepo.InsertAsync(
                new HlgAnswerOption(_guid.Create(), question.Id, answerKey, optContent, tenantId),
                autoSave: true);
        }
    }
}
