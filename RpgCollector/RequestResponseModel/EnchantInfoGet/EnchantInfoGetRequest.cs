using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.EnchantInfoGet
{
    public class EnchantInfoGetRequest
    {
        [Required]
        public int PlayerItemId { get; set; }
    }
}
