using Loyalzoo.DomainObject;

namespace Laser.Orchard.Fidelity.ViewModels
{
    public class MerchantApiData
    {
        public string MerchantId { get; set; }
        public string PlaceId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class APIResult
    {
        public bool success { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
    }
}