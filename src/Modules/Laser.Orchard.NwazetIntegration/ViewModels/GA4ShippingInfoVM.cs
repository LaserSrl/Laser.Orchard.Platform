using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class GA4ShippingInfoVM {
        public IEnumerable<IGAProductVM> ProductList { get; set; }
        [DefaultValue("")]
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("value")]
        public decimal Value { get; set; }
        [DefaultValue("")]
        [JsonProperty("coupon")]
        public string Coupon { get; set; }
        [DefaultValue("")]
        [JsonProperty("shipping_tier")]
        public string ShippingTier { get; set; }
    }
}