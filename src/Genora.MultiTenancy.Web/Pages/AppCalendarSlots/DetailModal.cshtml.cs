using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppPromotionTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCalendarSlots;

public class DetailModalModel : MultiTenancyPageModel
{
    // ----- Query args cho ModalManager -----
    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }               // Id slot (edit) - có thể null

    [BindProperty(SupportsGet = true)]
    public Guid? GolfCourseId { get; set; }     // từ JS truyền lên

    [BindProperty(SupportsGet = true)]
    public DateTime? ApplyDate { get; set; }    // từ JS truyền lên (yyyy-MM-dd)
    [BindProperty(SupportsGet = true)]
    public DateTime? ApplyDateTo { get; set; }    // từ JS truyền lên (yyyy-MM-dd)
    [BindProperty(SupportsGet = true)]
    public DateTime? ApplyDateFrom { get; set; }    // từ JS truyền lên (yyyy-MM-dd)
    [BindProperty(SupportsGet = true)]
    public string? TimeFrom { get; set; }       // "HH:mm"

    [BindProperty(SupportsGet = true)]
    public string? TimeTo { get; set; }
    [BindProperty]
    public List<SelectListItem> Promotions { get; set; }
    // ----- Form bind -----
    [BindProperty]
    public CreateUpdateAppCalendarSlotDto Slot { get; set; }

    public List<SelectListItem> CustomerTypeItems { get; set; } = new();

    private readonly IAppCalendarSlotService _slotService;
    private readonly IAppCustomerTypeService _customerTypeService;
    private readonly IMiniAppPromotionTypeService _promotionType;
    public DetailModalModel(
        IAppCalendarSlotService slotService,
        IAppCustomerTypeService customerTypeService,
        IMiniAppPromotionTypeService promotionType)
    {
        _slotService = slotService;
        _customerTypeService = customerTypeService;
        _promotionType = promotionType;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadCustomerTypesAsync();
        var promotion = await _promotionType.GetAllAsync();
        if(promotion != null && promotion.Count > 0)
        {
            Promotions = promotion.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
        }
        else
        {
            Promotions = new List<SelectListItem>();
        }
        // EDIT nếu Id có
        if (Id.HasValue)
        {
            var dto = await _slotService.GetAsync(Id.Value);

            var allTypes = CustomerTypeItems
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => Guid.Parse(x.Value))
                .ToList();

            var dtoPriceDict = dto.Prices.ToDictionary(p => p.CustomerTypeId, p => p.Price);

            Slot = new CreateUpdateAppCalendarSlotDto
            {
                GolfCourseId = dto.GolfCourseId,
                ApplyDate = dto.ApplyDate.Date,
                TimeFrom = dto.TimeFrom,
                TimeTo = dto.TimeTo,
                PromotionTypeId = dto.PromotionTypeId,
                MaxSlots = dto.MaxSlots,
                InternalNote = dto.InternalNote,
                IsActive = dto.IsActive,
                Prices = allTypes.Select(ctId => new CreateUpdateCalendarSlotPriceDto
                {
                    CustomerTypeId = ctId,
                    Price = dtoPriceDict.TryGetValue(ctId, out var price) ? price : 0
                }).ToList()
            };

            return Page();
        }

        // CREATE
        if (!GolfCourseId.HasValue || GolfCourseId.Value == Guid.Empty)
        {
            return BadRequest("Missing golfCourseId");
        }
        if (!ApplyDate.HasValue)
        {
            return BadRequest("Missing ApplyDate");
        }
        //if (!ApplyDateTo.HasValue)
        //{
        //    return BadRequest("Missing ApplyDateTo");
        //}

        var gcId = GolfCourseId ?? Guid.Empty;
        var date = ApplyDate?.Date ?? DateTime.Today;

        var tf = ParseTimeOrDefault(TimeFrom, new TimeSpan(6, 0, 0));
        var tt = ParseTimeOrDefault(TimeTo, tf.Add(TimeSpan.FromMinutes(30)));

        Slot = new CreateUpdateAppCalendarSlotDto
        {
            GolfCourseId = gcId,
            ApplyDate = date,
            TimeFrom = tf,
            TimeTo = tt,
            IsActive = true,
            Prices = CustomerTypeItems.Select(i => new CreateUpdateCalendarSlotPriceDto
            {
                CustomerTypeId = Guid.Parse(i.Value),
                Price = 0
            }).ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCustomerTypesAsync();
            return Page();
        }

        // đảm bảo ApplyDate Date-only
        Slot.ApplyDate = Slot.ApplyDate.Date;

        if (Id.HasValue)
            await _slotService.UpdateAsync(Id.Value, Slot);
        else
            await _slotService.CreateAsync(Slot);

        return NoContent();
    }

    private async Task LoadCustomerTypesAsync()
    {
        var result = await _customerTypeService.GetListAsync(
            new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1000,
                Sorting = "Name"
            });

        CustomerTypeItems = result.Items
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();
    }

    private static TimeSpan ParseTimeOrDefault(string? text, TimeSpan def)
    {
        if (text.IsNullOrWhiteSpace()) return def;

        // cho phép "HH:mm"
        if (TimeSpan.TryParse(text, out var t)) return t;

        // fallback: "HH:mm"
        if (text!.Length == 5 && TimeSpan.TryParse(text + ":00", out t)) return t;

        return def;
    }
}