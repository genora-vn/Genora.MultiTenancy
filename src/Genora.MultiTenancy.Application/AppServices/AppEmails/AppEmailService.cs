using Genora.MultiTenancy.AppDtos.AppEmails;
using Genora.MultiTenancy.DomainModels.AppEmails;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppEmails;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppEmails;

[Authorize]
public class AppEmailService :
    FeatureProtectedCrudAppService<
        Email,
        AppEmailDto,
        Guid,
        GetEmailListInput,
        CreateUpdateEmailDto>,
    IAppEmailService
{
    protected override string FeatureName => AppEmailFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppEmails.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppEmails.Default;

    private readonly IAppEmailSenderService _sender;

    public AppEmailService(
        IRepository<Email, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IAppEmailSenderService sender)
        : base(repository, currentTenant, featureChecker)
    {
        _sender = sender;

        GetPolicyName = MultiTenancyPermissions.AppEmails.Default;
        GetListPolicyName = MultiTenancyPermissions.AppEmails.Default;
        CreatePolicyName = MultiTenancyPermissions.AppEmails.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppEmails.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppEmails.Delete;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<AppEmailDto>> GetListAsync(GetEmailListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var ft = input.FilterText.Trim();
            query = query.Where(x =>
                x.Subject.Contains(ft) ||
                x.ToEmails.Contains(ft) ||
                (x.BookingCode != null && x.BookingCode.Contains(ft)));
        }

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        if (!input.BookingCode.IsNullOrWhiteSpace())
            query = query.Where(x => x.BookingCode == input.BookingCode!.Trim());

        if (input.CreatedFrom.HasValue)
            query = query.Where(x => x.CreationTime >= input.CreatedFrom.Value);

        if (input.CreatedTo.HasValue)
            query = query.Where(x => x.CreationTime <= input.CreatedTo.Value);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(Email.CreationTime) + " desc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<AppEmailDto>(totalCount, ObjectMapper.Map<System.Collections.Generic.List<Email>, System.Collections.Generic.List<AppEmailDto>>(items));
    }

    public override async Task<AppEmailDto> CreateAsync(CreateUpdateEmailDto input)
    {
        await CheckCreatePolicyAsync();

        // tạo email record + enqueue gửi ngay
        var id = await _sender.EnqueueRawAsync(
            toEmails: input.ToEmails,
            subject: input.Subject,
            body: input.Body,
            cc: input.CcEmails,
            bcc: input.BccEmails,
            bookingId: input.BookingId,
            bookingCode: input.BookingCode
        );

        var entity = await Repository.GetAsync(id);
        return ObjectMapper.Map<Email, AppEmailDto>(entity);
    }

    public override async Task<AppEmailDto> UpdateAsync(Guid id, CreateUpdateEmailDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);

        // chỉ cho sửa khi chưa Sent (tuỳ rule)
        if (entity.Status == EmailStatus.Sent)
            throw new AbpValidationException("Email đã gửi thành công, không thể sửa.");

        entity.ToEmails = input.ToEmails;
        entity.CcEmails = input.CcEmails;
        entity.BccEmails = input.BccEmails;
        entity.Subject = input.Subject;
        entity.Body = input.Body;
        entity.BookingId = input.BookingId;
        entity.BookingCode = input.BookingCode;

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<Email, AppEmailDto>(entity);
    }

    // Resend: set Pending + enqueue
    public virtual async Task ResendAsync(Guid id)
    {
        // permission riêng
        if (CurrentTenant.IsAvailable)
            await AuthorizationService.CheckAsync(MultiTenancyPermissions.AppEmails.Resend);
        else
            await AuthorizationService.CheckAsync(MultiTenancyPermissions.HostAppEmails.Resend);

        var entity = await Repository.GetAsync(id);

        entity.Status = EmailStatus.Pending;
        entity.TryCount = 0;
        entity.LastError = null;
        entity.NextTryTime = null;

        await Repository.UpdateAsync(entity, autoSave: true);

        // enqueue lại
        await LazyServiceProvider.GetRequiredService<Volo.Abp.BackgroundJobs.IBackgroundJobManager>()
            .EnqueueAsync(new Genora.MultiTenancy.AppServices.AppEmails.Jobs.SendEmailJobArgs { EmailId = entity.Id });
    }

    // SendNow: enqueue lại mà không reset trycount
    public virtual async Task SendNowAsync(Guid id)
    {
        if (CurrentTenant.IsAvailable)
            await AuthorizationService.CheckAsync(MultiTenancyPermissions.AppEmails.Send);
        else
            await AuthorizationService.CheckAsync(MultiTenancyPermissions.HostAppEmails.Send);

        var entity = await Repository.GetAsync(id);

        if (entity.Status == EmailStatus.Sent)
            return;

        await LazyServiceProvider.GetRequiredService<Volo.Abp.BackgroundJobs.IBackgroundJobManager>()
        .EnqueueAsync(new Genora.MultiTenancy.AppServices.AppEmails.Jobs.SendEmailJobArgs
        {
            EmailId = entity.Id,
            TenantId = entity.TenantId
        });
    }
}