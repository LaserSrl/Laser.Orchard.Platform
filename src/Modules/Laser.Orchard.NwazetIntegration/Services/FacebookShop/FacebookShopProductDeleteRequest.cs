using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductDeleteRequest : IFacebookShopRequest {
        public static string METHOD = "DELETE";
        [JsonIgnore]
        public bool Valid { get; set; }
        [JsonIgnore]
        public LocalizedString Message { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("retailer_id")]
        public string RetailerId { get; set; }

        /// <summary>
        /// Decodes every string to avoid html encoded characters (&quot;, &amp;, &egrave, ...).
        /// </summary>
        public void HtmlDecode() {
            // In the case of FacebookShopProductDeleteRequest, I don't need to do anything.
        }
    }
}