using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Enums.Hlg;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Hlg;

/// <summary>
/// Hồ sơ người chơi Gamification. Tái dùng dbo.AppCustomers (zalo/phone/code/BonusPoint)
/// + HLG.AppHlgUserProfiles cho field game (customerType, isRegistered).
/// Point history tái dùng ledger HL.AppHlPointTransactions (BonusPoint dùng chung).
/// Internal service — controller gọi trực tiếp.
/// </summary>
[RemoteService(false)]
[DisableValidation]
public class HlgProfileAppService : ApplicationService, IHlgProfileAppService
{
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<HlgUserProfile, Guid> _profileRepo;
    private readonly IRepository<HlPointTransaction, Guid> _pointTxnRepo;
    private readonly IRepository<HlgLearningProgress, Guid> _progressRepo;
    private readonly IRepository<HlgGameSession, Guid> _sessionRepo;
    private readonly IRepository<HlgProduct, Guid> _productRepo;
    private readonly IRepository<HlgRewardHistory, Guid> _rewardHistoryRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<HlgProfileAppService> _logger;

    public HlgProfileAppService(
        IRepository<Customer, Guid> customerRepo,
        IRepository<HlgUserProfile, Guid> profileRepo,
        IRepository<HlPointTransaction, Guid> pointTxnRepo,
        IRepository<HlgLearningProgress, Guid> progressRepo,
        IRepository<HlgGameSession, Guid> sessionRepo,
        IRepository<HlgProduct, Guid> productRepo,
        IRepository<HlgRewardHistory, Guid> rewardHistoryRepo,
        ICurrentTenant currentTenant,
        ILogger<HlgProfileAppService> logger)
    {
        LocalizationResource = typeof(Genora.MultiTenancy.Localization.MultiTenancyResource);
        _customerRepo = customerRepo;
        _profileRepo = profileRepo;
        _pointTxnRepo = pointTxnRepo;
        _progressRepo = progressRepo;
        _sessionRepo = sessionRepo;
        _productRepo = productRepo;
        _rewardHistoryRepo = rewardHistoryRepo;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<GamificationUserDto> UpsertCustomerAsync(HlgCustomerUpsertPayloadDto payload, CancellationToken ct = default)
    {
        ValidatePharmacyCode(payload.PharmacyCode ?? payload.VgaCode);
        ValidateCustomerType(payload.CustomerType);
        var phone = NormalizePhone(payload.Phone);
        if (string.IsNullOrWhiteSpace(phone))
            throw new UserFriendlyException("Thiếu số điện thoại");

        var name = string.IsNullOrWhiteSpace(payload.FullName) ? "Zalo User" : payload.FullName.Trim();
        var existing = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == phone, ct);

        Customer customer;
        if (existing == null)
        {
            customer = new Customer(GuidGenerator.Create(), phone, name)
            {
                TenantId = _currentTenant.Id,
                AvatarUrl = NullIfBlank(payload.AvatarUrl),
                ZaloUserId = NullIfBlank(payload.ZaloUserId),
                IsFollower = payload.IsFollower ?? false,
                IsActive = true,
                Address = NullIfBlank(payload.Address),

                Gender = HlgEnumMapper.GenderStringToByte(payload.Gender),
                DateOfBirth = ParseDate(payload.Birthday),
                CustomerCode = await GenerateCustomerCodeAsync(),
                CustomerSource = CustomerSource.ZaloMiniApp
            };
            customer = await _customerRepo.InsertAsync(customer, autoSave: true, cancellationToken: ct);
            _logger.LogInformation("HLG upsert: tạo mới KH {Phone} code={Code}", phone, customer.CustomerCode);
        }
        else
        {
            customer = existing;
            if (!string.IsNullOrWhiteSpace(payload.FullName)) customer.FullName = name;
            customer.AvatarUrl = NullIfBlank(payload.AvatarUrl) ?? customer.AvatarUrl;
            customer.ZaloUserId = NullIfBlank(payload.ZaloUserId) ?? customer.ZaloUserId;
            if (payload.IsFollower.HasValue) customer.IsFollower = payload.IsFollower.Value;
            customer.Address = NullIfBlank(payload.Address) ?? customer.Address;

            var g = HlgEnumMapper.GenderStringToByte(payload.Gender);
            if (g.HasValue) customer.Gender = g;
            var b = ParseDate(payload.Birthday);
            if (b.HasValue) customer.DateOfBirth = b;
            customer = await _customerRepo.UpdateAsync(customer, autoSave: true, cancellationToken: ct);
        }

        // Tạo/cập nhật HLG profile — customerType gán khi register (quyết định nghiệp vụ #6).
        var profile = await _profileRepo.FirstOrDefaultAsync(x => x.CustomerId == customer.Id, ct);
        var customerType = HlgEnumMapper.CustomerTypeFromString(payload.CustomerType);
        if (profile == null)
        {
            profile = new HlgUserProfile(GuidGenerator.Create(), customer.Id, _currentTenant.Id)
            {
                ZaloId = customer.ZaloUserId,
                PharmacyCode = NullIfBlank(payload.PharmacyCode ?? payload.VgaCode),
                CustomerType = customerType,
                IsRegistered = customerType.HasValue
            };
            profile = await _profileRepo.InsertAsync(profile, autoSave: true, cancellationToken: ct);
        }
        else
        {
            profile.PharmacyCode = NullIfBlank(payload.PharmacyCode ?? payload.VgaCode) ?? profile.PharmacyCode;
            profile.ZaloId = customer.ZaloUserId ?? profile.ZaloId;
            if (customerType.HasValue) profile.CustomerType = customerType;
            if (customerType.HasValue) profile.IsRegistered = true;
            profile = await _profileRepo.UpdateAsync(profile, autoSave: true, cancellationToken: ct);
        }

        return MapToDto(customer, profile);
    }

    public async Task<GamificationUserDto> GetByPhoneAsync(string phone, CancellationToken ct = default)
    {
        var (customer, profile) = await ResolveAsync(phone, ct);
        return MapToDto(customer, profile);
    }

    public async Task<GamificationUserDto> UpdateProfileAsync(string phone, UpdateProfilePayloadDto payload, CancellationToken ct = default)
    {
        var (customer, profile) = await ResolveAsync(phone, ct);

        if (!string.IsNullOrWhiteSpace(payload.FullName))
            customer.FullName = payload.FullName.Trim();

        var genderByte = HlgEnumMapper.GenderStringToByte(payload.Gender);
        if (genderByte.HasValue) customer.Gender = genderByte;

        var bday = ParseDate(payload.Birthday);
        if (bday.HasValue) customer.DateOfBirth = bday;

        if (!string.IsNullOrWhiteSpace(payload.Address))
            customer.Address = payload.Address.Trim();

        ValidatePharmacyCode(payload.PharmacyCode ?? payload.VgaCode);
        ValidateCustomerType(payload.CustomerType);
        if (payload.PharmacyCode != null || payload.VgaCode != null) profile.PharmacyCode = NullIfBlank(payload.PharmacyCode ?? payload.VgaCode);
        if (payload.CustomerType != null) profile.CustomerType = HlgEnumMapper.CustomerTypeFromString(payload.CustomerType);
        await _profileRepo.UpdateAsync(profile, autoSave: true, cancellationToken: ct);

        // Phone là khóa đồng bộ; chỉ đổi khi khác và chưa bị chiếm bởi KH khác.
        var newPhone = NormalizePhone(payload.Phone);
        if (!string.IsNullOrWhiteSpace(newPhone) && newPhone != customer.PhoneNumber)
        {
            var taken = await _customerRepo.AnyAsync(x => x.PhoneNumber == newPhone && x.Id != customer.Id, ct);
            if (!taken) customer.PhoneNumber = newPhone;
        }

        await _customerRepo.UpdateAsync(customer, autoSave: true, cancellationToken: ct);

        // Đánh dấu đã đăng ký khi hồ sơ đủ thông tin cơ bản.
        if (!profile.IsRegistered && !string.IsNullOrWhiteSpace(customer.FullName))
        {
            profile.IsRegistered = true;
            await _profileRepo.UpdateAsync(profile, autoSave: true, cancellationToken: ct);
        }

        return MapToDto(customer, profile);
    }

    public async Task<ProfileStatsDto> GetStatsAsync(string phone, CancellationToken ct = default)
    {
        var (customer, _) = await ResolveAsync(phone, ct);

        // knowledgeLearned = số bài học đã hoàn thành.
        var knowledgeLearned = await _progressRepo.CountAsync(
            x => x.CustomerId == customer.Id && x.IsCompleted, ct);

        // Accuracy is derived exclusively from server-scored, completed sessions.
        // Do not use any score/accuracy value supplied by the Mini App client.
        var sessionQ = await _sessionRepo.GetQueryableAsync();
        var totals = await AsyncExecuter.FirstOrDefaultAsync(
            sessionQ.Where(x => x.CustomerId == customer.Id && x.IsFinished && x.TotalQuestions > 0)
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Correct = g.Sum(x => x.CorrectCount),
                        Questions = g.Sum(x => x.TotalQuestions)
                    }), ct);

        var accuracy = totals == null || totals.Questions == 0
            ? 0
            : (int)decimal.Round((decimal)totals.Correct * 100 / totals.Questions, 0, MidpointRounding.AwayFromZero);

        return new ProfileStatsDto
        {
            Points = (int)decimal.Round(customer.BonusPoint),
            KnowledgeLearned = knowledgeLearned,
            AccuracyPercent = accuracy
        };
    }

    public async Task<List<LearningHistoryItemDto>> GetLearningHistoryAsync(string phone, CancellationToken ct = default)
    {
        var (customer, _) = await ResolveAsync(phone, ct);

        var progressQ = await _progressRepo.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            progressQ.Where(x => x.CustomerId == customer.Id).OrderByDescending(x => x.LastViewedAt), ct);

        if (rows.Count == 0) return new List<LearningHistoryItemDto>();

        // Lấy tên bài học cho các ProductId liên quan (1 query).
        var productIds = rows.Select(x => x.ProductId).Distinct().ToList();
        var prodQ = await _productRepo.GetQueryableAsync();
        var products = await AsyncExecuter.ToListAsync(
            prodQ.Where(p => productIds.Contains(p.Id)).Select(p => new { p.Id, p.Name, p.ThumbnailUrl }), ct);
        var nameById = products.ToDictionary(x => x.Id, x => x.Name);

        return rows.Select(x => new LearningHistoryItemDto
        {
            ProductId = x.ProductId,
            ProductName = nameById.TryGetValue(x.ProductId, out var n) ? n : string.Empty,
            ThumbnailUrl = products.FirstOrDefault(p => p.Id == x.ProductId)?.ThumbnailUrl,
            ProgressPercent = x.ProgressPercent,
            LastViewedAt = x.LastViewedAt
        }).ToList();
    }

    public async Task<List<PointHistoryItemDto>> GetPointHistoryAsync(string phone, CancellationToken ct = default)
    {
        var (customer, _) = await ResolveAsync(phone, ct);

        var q = await _pointTxnRepo.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            q.Where(x => x.CustomerId == customer.Id).OrderByDescending(x => x.CreationTime), ct);

        return rows.Select(x => new PointHistoryItemDto
        {
            Id = x.Id,
            SourceName = x.Description ?? x.RefCode ?? "Điểm",
            PointDelta = (int)decimal.Round(x.Value),
            CreatedAt = x.CreationTime
        }).ToList();
    }

    public async Task<List<RewardHistoryItemDto>> GetRewardHistoryAsync(string phone, CancellationToken ct = default)
    {
        var (customer, _) = await ResolveAsync(phone, ct);

        var q = await _rewardHistoryRepo.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            q.Where(x => x.CustomerId == customer.Id).OrderByDescending(x => x.CreationTime), ct);

        var sessionIds = rows.Where(x=>x.SessionId.HasValue).Select(x=>x.SessionId!.Value).ToList();
        var sessions = await _sessionRepo.GetQueryableAsync();
        var games = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame,Guid>>().GetQueryableAsync();
        var origins = await AsyncExecuter.ToListAsync(from s in sessions join g in games on s.GameId equals g.Id
            where sessionIds.Contains(s.Id) && s.CustomerId == customer.Id select new { s.Id, g.Name }, ct);
        return rows.Select(x => new RewardHistoryItemDto
        {
            GameName = origins.FirstOrDefault(s=>s.Id==x.SessionId)?.Name,
            Id = x.Id,
            RewardName = x.RewardName,
            PointDelta = x.PointDelta,
            Status = HlgEnumMapper.RewardHistoryStatusToString(x.Status),
            CreatedAt = x.CreationTime
        }).ToList();
    }

    public async Task<List<GameHistoryDto>> GetGameHistoryAsync(string phone, int skip = 0, int take = 50)
    {
        var (customer, _) = await ResolveAsync(phone, default);
        var sessions = await _sessionRepo.GetQueryableAsync();
        var games = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame,Guid>>().GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync((from s in sessions join g in games on s.GameId equals g.Id
            where s.CustomerId == customer.Id && s.IsFinished && s.FinishedAt != null
            orderby s.FinishedAt descending, s.Id select new { s.Id, s.GameId, GameName = g.Name, s.Score, s.StartedAt, s.FinishedAt }).Skip(Math.Max(0,skip)).Take(Math.Clamp(take,1,100)));
        return rows.Select(x=>new GameHistoryDto { Id=x.Id,GameId=x.GameId,GameName=x.GameName,Score=x.Score,FinishedAt=x.FinishedAt!.Value,DurationSeconds=(int)Math.Max(0,(x.FinishedAt.Value-x.StartedAt).TotalSeconds) }).ToList();
    }
    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Tìm Customer theo phone + đảm bảo có HlgUserProfile (tạo nếu thiếu).</summary>
    private async Task<(Customer customer, HlgUserProfile profile)> ResolveAsync(string phone, CancellationToken ct)
    {
        var normalized = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new UserFriendlyException("Thiếu số điện thoại");

        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized, ct)
            ?? throw new UserFriendlyException("Không tìm thấy khách hàng. Vui lòng đăng ký trước.");

        var profile = await _profileRepo.FirstOrDefaultAsync(x => x.CustomerId == customer.Id, ct);
        if (profile == null)
        {
            profile = new HlgUserProfile(GuidGenerator.Create(), customer.Id, _currentTenant.Id)
            {
                ZaloId = customer.ZaloUserId,
                IsRegistered = false
            };
            profile = await _profileRepo.InsertAsync(profile, autoSave: true, cancellationToken: ct);
            _logger.LogInformation("HLG: tạo profile cho customer {CustomerId} phone {Phone}", customer.Id, normalized);
        }

        return (customer, profile);
    }

    private static GamificationUserDto MapToDto(Customer c, HlgUserProfile p)
    {
        return new GamificationUserDto
        {
            Id = p.Id,
            ZaloId = c.ZaloUserId ?? p.ZaloId,
            FullName = c.FullName,
            Phone = c.PhoneNumber,
            Gender = HlgEnumMapper.GenderByteToString(c.Gender),
            Birthday = HlgEnumMapper.DateToIso(c.DateOfBirth),
            Address = c.Address,
            PharmacyCode = p.PharmacyCode,
            VgaCode = p.PharmacyCode,
            AvatarUrl = c.AvatarUrl,
            CustomerType = HlgEnumMapper.CustomerTypeToString(p.CustomerType),
            Points = (int)decimal.Round(c.BonusPoint),
            IsRegistered = p.IsRegistered,
            CreatedAt = p.CreationTime
        };
    }

    private void ValidatePharmacyCode(string? code) { if (code?.Length > 100) throw new UserFriendlyException(L["Hlg:PharmacyCodeTooLong"]); }
    private void ValidateCustomerType(string? type) { if (type != null && HlgEnumMapper.CustomerTypeFromString(type) == null) throw new UserFriendlyException(L["Hlg:InvalidCustomerType"]); }
    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return Regex.Replace(phone.Trim(), @"\s+|-|\.", "");
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<string> GenerateCustomerCodeAsync()
    {
        const string prefix = "HLGKH";
        var queryable = await _customerRepo.GetQueryableAsync();

        var maxNumber = 0;
        foreach (var code in queryable
                     .Where(c => c.CustomerCode != null && c.CustomerCode.StartsWith(prefix))
                     .Select(c => c.CustomerCode!))
        {
            var numberPart = code.Substring(prefix.Length);
            if (int.TryParse(numberPart, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var n) && n > maxNumber)
                maxNumber = n;
        }

        var next = maxNumber + 1;
        var candidate = $"{prefix}{next.ToString("D6", System.Globalization.CultureInfo.InvariantCulture)}";

        while (await _customerRepo.AnyAsync(c => c.CustomerCode == candidate))
        {
            next++;
            candidate = $"{prefix}{next.ToString("D6", System.Globalization.CultureInfo.InvariantCulture)}";
        }

        return candidate;
    }

    private static DateTime? ParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-ddTHH:mm:ss", "dd-MM-yyyy" };
        if (DateTime.TryParseExact(raw.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
            return d.Date;
        if (DateTime.TryParse(raw.Trim(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out d))
            return d.Date;
        return null;
    }
}
