using Laser.Orchard.NwazetIntegration.Services.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class GTMPurchaseVM {
        public GTMActionField ActionField { get; set; }
        public List<GTMProductVM> ProductList { get; set; }
    }


    public class GTMActionField {
        [JsonProperty("id")]
        public string Id { get; set; }
        [DefaultValue("")]
        [JsonProperty("affiliation", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Affiliation { get; set; }
        [JsonProperty("revenue")]
        [JsonConverter(typeof(PriceFormatConverter))]
        public decimal Revenue { get; set; }
        [JsonProperty("tax", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(PriceFormatConverter))]
        public decimal Tax { get; set; }
        [JsonProperty("shipping", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(PriceFormatConverter))]
        public decimal Shipping { get; set; }
        [DefaultValue("")]
        [JsonProperty("coupon", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Coupon { get; set; }
    }
}