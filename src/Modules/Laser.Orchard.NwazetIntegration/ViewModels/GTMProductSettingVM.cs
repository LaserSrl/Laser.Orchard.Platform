using System;
using System.Collections.Generic;
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
}