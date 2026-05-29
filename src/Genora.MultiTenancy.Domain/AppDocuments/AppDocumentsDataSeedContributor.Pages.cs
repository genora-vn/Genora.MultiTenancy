using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetPagesForSection(string sectionSlug) => sectionSlug switch
    {
        "mini-app-setup" => GetMiniAppSetupPages(),
        "golf-tee-times" => GetGolfTeeTimesPages(),
        "salon-location-schedule" => GetSalonLocationSchedulePages(),
        "customer-booking" => GetCustomerBookingPages(),
        "customer-booking-salon" => GetCustomerBookingSalonPages(),
        "loyalty" => GetLoyaltyPages(),
        "fnb" => GetFnbPages(),
        "proshop" => GetProshopPages(),
        "salon-beauty" => GetSalonBeautyPages(),
        "news" => GetNewsPages(),
        "system-admin" => GetSystemAdminPages(),
        _ => new List<PageSeed>()
    };
}
