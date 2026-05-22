using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppSpecialDates;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCustomerTypes;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppCustomerTypeDto CustomerType { get; set; }

    public List<CustomerTypeOriginalPriceFieldDto> PriceFields { get; set; } = new();

    private readonly IAppCustomerTypeService _appCustomerTypeService;
    private readonly IAppSpecialDateService _appSpecialDateService;

    public CreateModalModel(
        IAppCustomerTypeService appCustomerTypeService,
        IAppSpecialDateService appSpecialDateService)
    {
        _appCustomerTypeService = appCustomerTypeService;
        _appSpecialDateService = appSpecialDateService;
    }

    public async Task OnGetAsync()
    {
        CustomerType = new CreateUpdateAppCustomerTypeDto();
        await LoadPriceFieldsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appCustomerTypeService.CreateAsync(CustomerType);
        return NoContent();
    }

    private async Task LoadPriceFieldsAsync()
    {
        var specialDates = await _appSpecialDateService.GetListAsync(new GetSpecialDateListInput
        {
            MaxResultCount = 100
        });

        PriceFields = BuildFields(specialDates, CustomerType);
    }

    public static List<CustomerTypeOriginalPriceFieldDto> BuildFields(
        PagedResultDto<SpecialDateDto> specialDates,
        CreateUpdateAppCustomerTypeDto? dto)
    {
        var seen = new HashSet<string>();
        var result = new List<CustomerTypeOriginalPriceFieldDto>();

        foreach (var sd in specialDates.Items.Where(x => x.IsActive))
        {
            var field = CustomerTypeOriginalPriceFieldMap.ResolveField(sd.Name);
            if (field == null) continue;
            if (!seen.Add(field)) continue;

            result.Add(new CustomerTypeOriginalPriceFieldDto
            {
                SpecialDateName = sd.Name,
                FieldName = field,
                Label = CustomerTypeOriginalPriceFieldMap.ResolveLabel(sd.Name),
                Value = CustomerTypeOriginalPriceFieldMap.GetValue(dto, field)
            });
        }

        // Luôn đảm bảo có Weekday (OriginalPrice) đứng đầu nếu có cấu hình SpecialDate
        var ordered = new[]
        {
            CustomerTypeOriginalPriceFieldMap.WeekdayField,
            CustomerTypeOriginalPriceFieldMap.WeekendField,
            CustomerTypeOriginalPriceFieldMap.HolidayField,
            CustomerTypeOriginalPriceFieldMap.MemberDayField
        };

        return result
            .OrderBy(x => System.Array.IndexOf(ordered, x.FieldName))
            .ToList();
    }
}
