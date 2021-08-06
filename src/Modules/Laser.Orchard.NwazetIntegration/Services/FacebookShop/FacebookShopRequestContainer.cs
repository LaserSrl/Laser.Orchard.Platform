using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopRequestContainer {
        [JsonProperty("item_type", NullValueHandling = NullValueHandling.Ignore)]
        public string ItemType { get; set; }
        [JsonProperty("requests")]
        public List<IFacebookShopRequest> Requests { get; set; }

        public string ToJson() {
            return JsonConvert.SerializeObject(this);
        }

        public FacebookShopRequestContainer() {
            Requests = new List<IFacebookShopRequest>();
        }
    }
}