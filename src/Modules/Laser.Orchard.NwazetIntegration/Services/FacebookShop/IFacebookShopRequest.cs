using Newtonsoft.Json;
using Orchard.Localization;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    public interface IFacebookShopRequest {
        [JsonIgnore]
        bool Valid { get; set; }
        [JsonIgnore]
        LocalizedString Message { get; set; }
    }
}
