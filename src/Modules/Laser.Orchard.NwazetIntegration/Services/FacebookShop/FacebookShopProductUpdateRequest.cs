using Newtonsoft.Json;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.MediaLibrary.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer;

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