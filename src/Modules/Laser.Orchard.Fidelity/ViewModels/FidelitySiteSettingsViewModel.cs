using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Fidelity.ViewModels
{
    public class FidelitySiteSettingsViewModel
    {
        [Required]
        public string DeveloperKey { get; set; }

        [Required]
        public string ApiURL { get; set; }

        [Required]
        public string MerchantUsername { get; set; }

        public string MerchantPwd { get; set; }

        [Required]
        public LoyalzooRegistrationEnum RegisterOnLogin { get; set; }
    }
}