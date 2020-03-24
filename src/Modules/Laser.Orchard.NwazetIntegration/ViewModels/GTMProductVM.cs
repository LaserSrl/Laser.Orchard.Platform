using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services.JsonConverters;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class GTMProductVM {
        public GTMProductVM() { }
        public GTMProductVM(
            GTMProductPart part) {
            PartId = part.Id;
            Id = part.ProductId;
            Name = part.Name;
            Brand = part.Brand;
            Category = part.Category;
            Variant = part.Variant;
            Price = part.Price;
        }
        [JsonProperty("partId")]
        public int PartId { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [DefaultValue("")]
        [JsonProperty("brand", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Brand { get; set; }
        [DefaultValue("")]
        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Category { get; set; }
        [DefaultValue("")]
        [JsonProperty("variant", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Variant { get; set; }
        [JsonProperty("price")]
        [JsonConverter(typeof(PriceFormatConverter))]
        public decimal Price { get; set; }
        [JsonProperty("quantity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Quantity { get; set; }
        [DefaultValue("")]
        [JsonProperty("coupon", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Coupon { get; set; }
        [JsonProperty("position", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Position { get; set; }
        [DefaultValue("")]
        [JsonProperty("list", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ListName { get; set; }
    }
}