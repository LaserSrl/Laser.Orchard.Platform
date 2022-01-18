using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    /// <summary>
    /// These classes parse the json template of the body and set its parameters before sending it to the create / update product api.
    /// </summary>
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductUpdateRequest : IFacebookShopRequest {
        // Default values.
        public static string METHOD = "UPDATE";
        [JsonIgnore]
        public bool Valid { get; set; }
        [JsonIgnore]
        public LocalizedString Message { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("retailer_id")]
        public string RetailerId { get; set; }
        [JsonProperty("data")]
        public FacebookServiceJsonContextData ProductData { get; set; }

        public static FacebookShopProductUpdateRequest From(string jsonBody) {
            var context = JsonConvert.DeserializeObject<FacebookShopProductUpdateRequest>(jsonBody);
            context.Valid = true;
            if (context.ProductData == null) {
                context.ProductData = new FacebookServiceJsonContextData();
            }
            context.ProductData.Valid = true;
            return context;
        }

        /// <summary>
        /// Decodes every string to avoid html encoded characters (&quot;, &amp;, &egrave, ...).
        /// </summary>
        public void HtmlDecode() {
            ProductData.Name = HttpUtility.HtmlDecode(ProductData.Name);
            // I need to do a double decode, because tinyMCE already encodes something, which is encoded again by tokenizer.
            // Example:
            // TextBox on backoffice:
            //      La classica pizza "margherita", per forno o forno microonde. Il prodotto é surgelato. Caratteri speciali: è é à ò ì ù & €
            // Text posted, after TinyMCE processing but before tokenizer management:
            //      <p>La classica pizza "margherita", per forno o forno microonde. Il prodotto &eacute; surgelato. Caratteri speciali: &egrave; &eacute; &agrave; &ograve; &igrave; &ugrave; &amp; &euro;</p>
            // Text result after tokenizer.Replace parsing:
            //      La classica pizza &quot;margherita&quot;, per forno o forno microonde. Il prodotto &amp;eacute; surgelato. Caratteri speciali: &amp;egrave; &amp;eacute; &amp;agrave; &amp;ograve; &amp;igrave; &amp;ugrave; &amp;amp; &amp;euro;
            //ProductData.Description = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(ProductData.Description));
            ProductData.Description = HttpUtility.HtmlDecode(ProductData.Description);
            ProductData.Brand = HttpUtility.HtmlDecode(ProductData.Brand);
        }
    }

    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookServiceJsonContextData {
        [JsonIgnore]
        public bool Valid { get; set; }
        [JsonIgnore]
        public LocalizedString Message { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("availability")]
        public string Availability { get; set; }
        [JsonProperty("condition")]
        public string Condition { get; set; }
        [JsonProperty("price")]
        public string Price { get; set; }
        [JsonProperty("sale_price", NullValueHandling = NullValueHandling.Ignore)]
        public string SalePrice { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
        [JsonProperty("brand")]
        public string Brand { get; set; }
    }
}