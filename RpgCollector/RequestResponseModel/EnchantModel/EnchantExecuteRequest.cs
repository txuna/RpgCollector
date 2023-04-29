using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.EnchantModel
{
    public class EnchantExecuteRequest
    {
        [Required]
        public int PlayerItemId { get; set; }
    }
}
