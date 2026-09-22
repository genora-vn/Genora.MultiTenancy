using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Globalization;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Content;
using Volo.Abp.Validation;
namespace Genora.MultiTenancy.AppServices.Hlg.Admin;
[Authorize]
public class HlgQuestionAdminAppService : FeatureProtectedCrudAppService<HlgQuestion, HlgQuestionAdminDto, Guid, GetHlgAdminListInput, CreateHlgQuestionInput, UpdateHlgQuestionInput>, IHlgQuestionAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgGames.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgGames.Default;
    private readonly IRepository<HlgGame, Guid> _games;
    private readonly IRepository<HlgAnswerOption, Guid> _options;
    private readonly IRepository<HlgGameSession, Guid> _sessions;
    private readonly HlgQuestionExcelTemplateGenerator _excelTemplateGenerator;
    private readonly HlgQuestionExcelImporter _excelImporter;
    public HlgQuestionAdminAppService(IRepository<HlgQuestion, Guid> repository, ICurrentTenant tenant, IFeatureChecker features, IRepository<HlgGame, Guid> games, IRepository<HlgAnswerOption, Guid> options, IRepository<HlgGameSession, Guid> sessions, HlgQuestionExcelTemplateGenerator excelTemplateGenerator, HlgQuestionExcelImporter excelImporter) : base(repository, tenant, features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        _games = games;
        _options = options;
        _sessions = sessions;
        _excelTemplateGenerator = excelTemplateGenerator;
        _excelImporter = excelImporter;
        GetPolicyName = MultiTenancyPermissions.AppHlgGames.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHlgGames.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHlgGames.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHlgGames.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHlgGames.Delete;
    }
    public override async Task<PagedResultDto<HlgQuestionAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.FilterText)) { var term = input.FilterText.Trim(); query = query.Where(x => x.Content.Contains(term)); }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive.Value);
        if (input.ParentId.HasValue) query = query.Where(x => x.GameId == input.ParentId.Value);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Index).ThenBy(x => x.Id).Skip(Math.Max(0, input.SkipCount)).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        return new PagedResultDto<HlgQuestionAdminDto>(count, rows.Select(Map).ToList());
    }
    public override async Task<HlgQuestionAdminDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        return Map(await Repository.GetAsync(id));
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgQuestionAdminDto> CreateAsync(CreateHlgQuestionInput input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);
        var entity = new HlgQuestion(GuidGenerator.Create(), input.GameId, input.Index, input.Content.Trim(), CurrentTenant.Id);
        Apply(input, entity);
        await Repository.InsertAsync(entity, autoSave: true);
        await SaveOptionsAsync(entity.Id, input);
        return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgQuestionAdminDto> UpdateAsync(Guid id, UpdateHlgQuestionInput input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);
        await ValidateAsync(input, id);
        if (entity.GameId != input.GameId) throw new UserFriendlyException(L["Hlg:ParentCannotChange"]);
        Apply(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);
        await SaveOptionsAsync(entity.Id, input);
        return Map(entity);
    }
    private async Task ValidateAsync(HlgQuestionInput input, Guid? id, bool checkSessions = true)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        HlgContentValidation.Localized(() => HlgContentValidation.Url(input.ImageUrl), key => L[key]);
        await _games.GetAsync(input.GameId);
        if (checkSessions && await _sessions.AnyAsync(x => x.GameId == input.GameId)) throw new UserFriendlyException(L["Hlg:GameHasSessions"]);
        if (await Repository.AnyAsync(x => x.GameId == input.GameId && x.Index == input.Index && x.Id != id)) throw new UserFriendlyException(L["Hlg:QuestionIndexExists"]);
        await Task.CompletedTask;
    }
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var question = await Repository.GetAsync(id);
        if (await _sessions.AnyAsync(x => x.GameId == question.GameId)) throw new UserFriendlyException(L["Hlg:GameHasSessions"]);
        await Repository.DeleteAsync(id);
    }
    private static void Apply(HlgQuestionInput input, HlgQuestion entity)
    {
        entity.GameId = input.GameId;
        entity.Index = input.Index;
        entity.Content = input.Content.Trim();
        entity.ImageUrl = input.ImageUrl;
        entity.TimeLimitSec = input.TimeLimitSec;
        entity.ScoreMultiplier = input.ScoreMultiplier;
        entity.IsActive = input.IsActive;
        entity.CorrectKey = input.CorrectKey;
    }
    private static HlgQuestionAdminDto Map(HlgQuestion entity) => new()
    {
        Id = entity.Id,
        GameId = entity.GameId,
        Index = entity.Index,
        Content = entity.Content,
        ImageUrl = entity.ImageUrl,
        TimeLimitSec = entity.TimeLimitSec,
        ScoreMultiplier = entity.ScoreMultiplier,
        IsActive = entity.IsActive,
    };
    // Secret answer is available ONLY to editors, never on list/get or mini-app DTOs.
    public virtual async Task<HlgQuestionInput> GetEditorAsync(Guid id)
    {
        await CheckUpdatePolicyAsync();
        var e = await Repository.GetAsync(id);
        var options = await _options.GetListAsync(x => x.QuestionId == id);
        string? Option(HlgAnswerKey key) => options.FirstOrDefault(x => x.Key == key)?.Content;
        return new HlgQuestionInput { GameId = e.GameId, Index = e.Index, Content = e.Content, ImageUrl = e.ImageUrl,
            TimeLimitSec = e.TimeLimitSec, ScoreMultiplier = e.ScoreMultiplier, CorrectKey = e.CorrectKey, IsActive = e.IsActive,
            OptionA = Option(HlgAnswerKey.A) ?? "", OptionB = Option(HlgAnswerKey.B) ?? "", OptionC = Option(HlgAnswerKey.C), OptionD = Option(HlgAnswerKey.D) };
    }
    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync(Guid gameId)
    {
        await CheckGetListPolicyAsync();
        if (gameId == Guid.Empty) throw new UserFriendlyException(L["Hlg:ImportGameRequired"]);
        await _games.GetAsync(gameId);
        return _excelTemplateGenerator.Generate();
    }

    [DisableValidation]
    [UnitOfWork(isTransactional: true)]
    public async Task<int> ImportExcelAsync(ImportHlgQuestionExcelInput input)
    {
        await CheckCreatePolicyAsync();
        await CheckUpdatePolicyAsync();

        if (input.GameId == Guid.Empty) throw new UserFriendlyException(L["Hlg:ImportGameRequired"]);
        if (input.File == null) throw new UserFriendlyException(L["Hlg:ImportFileRequired"]);
        if (!(input.File.FileName ?? "").EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException(L["Hlg:ImportFileTypeInvalid"]);
        if ((input.File.ContentLength ?? 0) > 10 * 1024 * 1024)
            throw new UserFriendlyException(L["Hlg:ImportFileTooLarge"]);

        await _games.GetAsync(input.GameId);
        //if (await _sessions.AnyAsync(x => x.GameId == input.GameId))
        //    throw new UserFriendlyException(L["Hlg:GameHasSessions"]);

        List<HlgQuestionExcelRow> rows;
        try
        {
            using var stream = input.File.GetStream();
            rows = _excelImporter.Read(stream);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Cannot read HLG question import workbook for game {GameId}", input.GameId);
            throw new UserFriendlyException(L["Hlg:ImportFileInvalid"]);
        }

        if (rows.Count == 0) throw new UserFriendlyException(L["Hlg:ImportNoData"]);

        var success = 0;
        foreach (var row in rows)
        {
            try
            {
                var questionInput = ParseImportRow(input.GameId, row);
                var existing = await Repository.FirstOrDefaultAsync(x => x.GameId == input.GameId && x.Index == questionInput.Index);
                await ValidateAsync(questionInput, existing?.Id, checkSessions: false);

                if (existing == null)
                {
                    existing = new HlgQuestion(GuidGenerator.Create(), input.GameId, questionInput.Index, questionInput.Content.Trim(), CurrentTenant.Id);
                    Apply(questionInput, existing);
                    await Repository.InsertAsync(existing, autoSave: true);
                }
                else
                {
                    Apply(questionInput, existing);
                    await Repository.UpdateAsync(existing, autoSave: true);
                }

                await SaveOptionsAsync(existing.Id, questionInput);
                success++;
            }
            catch (UserFriendlyException ex)
            {
                throw new UserFriendlyException(L["Hlg:ImportRowInvalid", row.RowNumber, ex.Message]);
            }
            catch (ValidationException ex)
            {
                throw new UserFriendlyException(L["Hlg:ImportRowInvalid", row.RowNumber, ex.Message]);
            }
        }

        return success;
    }

    private HlgQuestionInput ParseImportRow(Guid gameId, HlgQuestionExcelRow row)
    {
        if (!int.TryParse(row.Index, out var index) || index < 0)
            throw new UserFriendlyException(L["Hlg:ImportIndexInvalid"]);
        if (string.IsNullOrWhiteSpace(row.Content))
            throw new UserFriendlyException(L["Hlg:ImportContentRequired"]);
        if (string.IsNullOrWhiteSpace(row.OptionA) || string.IsNullOrWhiteSpace(row.OptionB))
            throw new UserFriendlyException(L["Hlg:ImportOptionsRequired"]);

        var timeLimitSec = 30;
        if (!string.IsNullOrWhiteSpace(row.TimeLimitSec) &&
            (!int.TryParse(row.TimeLimitSec, out timeLimitSec) || timeLimitSec < 1 || timeLimitSec > 3600))
            throw new UserFriendlyException(L["Hlg:ImportTimeLimitInvalid"]);

        var scoreMultiplier = 1m;
        if (!string.IsNullOrWhiteSpace(row.ScoreMultiplier) &&
            (!TryParseDecimal(row.ScoreMultiplier, out scoreMultiplier) || scoreMultiplier < 0.01m || scoreMultiplier > 100m))
            throw new UserFriendlyException(L["Hlg:ImportScoreMultiplierInvalid"]);

        if (!TryParseAnswerKey(row.CorrectKey, out var correctKey))
            throw new UserFriendlyException(L["Hlg:ImportCorrectKeyInvalid"]);
        if (!TryParseBoolean(row.IsActive, true, out var isActive))
            throw new UserFriendlyException(L["Hlg:ImportIsActiveInvalid"]);

        return new HlgQuestionInput
        {
            GameId = gameId,
            Index = index,
            Content = row.Content,
            ImageUrl = string.IsNullOrWhiteSpace(row.ImageUrl) ? null : row.ImageUrl,
            TimeLimitSec = timeLimitSec,
            ScoreMultiplier = scoreMultiplier,
            OptionA = row.OptionA,
            OptionB = row.OptionB,
            OptionC = string.IsNullOrWhiteSpace(row.OptionC) ? null : row.OptionC,
            OptionD = string.IsNullOrWhiteSpace(row.OptionD) ? null : row.OptionD,
            CorrectKey = correctKey,
            IsActive = isActive
        };
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result) ||
               decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out result);
    }

    private static bool TryParseAnswerKey(string value, out HlgAnswerKey result)
    {
        var normalized = value.Trim();
        if (Enum.TryParse(normalized, true, out result) && Enum.IsDefined(typeof(HlgAnswerKey), result)) return true;
        if (byte.TryParse(normalized, out var number) && Enum.IsDefined(typeof(HlgAnswerKey), number))
        {
            result = (HlgAnswerKey)number;
            return true;
        }
        result = default;
        return false;
    }

    private static bool TryParseBoolean(string value, bool defaultValue, out bool result)
    {
        if (string.IsNullOrWhiteSpace(value)) { result = defaultValue; return true; }
        var normalized = value.Trim();
        if (bool.TryParse(normalized, out result)) return true;
        if (normalized == "1" || normalized.Equals("yes", StringComparison.OrdinalIgnoreCase) || normalized.Equals("có", StringComparison.OrdinalIgnoreCase)) { result = true; return true; }
        if (normalized == "0" || normalized.Equals("no", StringComparison.OrdinalIgnoreCase) || normalized.Equals("không", StringComparison.OrdinalIgnoreCase)) { result = false; return true; }
        return false;
    }

    private async Task SaveOptionsAsync(Guid id, HlgQuestionInput input)
    {
        var existing = await _options.GetListAsync(x => x.QuestionId == id);
        var texts = new[] { input.OptionA, input.OptionB, input.OptionC, input.OptionD };
        for (var i = 0; i < texts.Length; i++)
        {
            var key = (HlgAnswerKey)(i + 1);
            var option = existing.FirstOrDefault(x => x.Key == key);
            if (string.IsNullOrWhiteSpace(texts[i])) { if (option != null) await _options.DeleteAsync(option, autoSave: true); continue; }
            if (option == null) await _options.InsertAsync(new HlgAnswerOption(GuidGenerator.Create(), id, key, texts[i]!.Trim(), CurrentTenant.Id), autoSave: true);
            else { option.Content = texts[i]!.Trim(); await _options.UpdateAsync(option, autoSave: true); }
        }
    }
}
