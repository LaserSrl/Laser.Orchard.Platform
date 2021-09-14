using Laser.Orchard.NwazetIntegration.Services.JsonConverters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class GA4PurchaseVM {
        public List<IGAProductVM> ProductList { get; set; }
        public GA4ActionField ActionField { get; set; }
    }

    public class GA4ActionField {
        [DefaultValue("")]
        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }
        [DefaultValue("")]
        [JsonProperty("affiliation")]
        public string Affiliation { get; set; }
        [JsonProperty("value")]
        [JsonConverter(typeof(PriceFormatConverter))]
        public decimal Value { get; set; }
        [JsonProperty("tax")]
        [JsonConverter(typeof(PriceFormatConverter))]
        public decimal Tax { get; set; }
        [JsonProperty("shipping")]
        [JsonConverter(typeof(PriceFormatConverter))]
        public decimal Shipping { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [DefaultValue("")]
        [JsonProperty("coupon")]
        public string Coupon { get; set; }
    }
}