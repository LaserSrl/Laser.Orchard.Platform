using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class GTMProductSettingVM {
        /// <summary>
        /// In GTM product part is identifier with ProductId
        /// </summary>
        public Enums.TypeId Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string Variant { get; set; }
    }

    public class GTMProductVM {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string Variant { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Coupon { get; set; }
        public int Position { get; set; }
    }
}