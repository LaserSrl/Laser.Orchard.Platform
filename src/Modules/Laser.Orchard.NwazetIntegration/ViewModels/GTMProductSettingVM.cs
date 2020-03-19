using Laser.Orchard.NwazetIntegration.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class GTMProductSettingVM {
        /// <summary>
        /// In GTM product part is identifier with ProductId
        /// </summary>
        public string Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string Variant { get; set; }
        public string Price { get; set; }
        public string Quantity { get; set; }
        public string Coupon { get; set; }
        public string Position { get; set; }
    }

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
            //Quantity = part.Quantity;
            //Coupon = part.Coupon;
            //Position = part.Position;
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
        public decimal Price { get; set; }
        [JsonProperty("quantity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Quantity { get; set; }
        [DefaultValue("")]
        [JsonProperty("coupon", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Coupon { get; set; }
        [JsonProperty("position", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Position { get; set; }
    }
}