
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppPromotionTypes
{
    public class CreateUpdatePromotionTypeDto
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public string? ColorCode { get; set; }
        public bool Status { get; set; }
        public bool IsUploadImage { get; set; }
        public IRemoteStreamContent? Images { get; set; }
    }
}
