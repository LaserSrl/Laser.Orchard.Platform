using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class GTMPurchaseVM {
        public GTMActionField ActionField { get; set; }
        public List<GTMProductVM> ProductList { get; set; }
    }


    public class GTMActionField {
        public string Id { get; set; }
        public string Affiliation { get; set; }
        public decimal Revenue { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public string Coupon { get; set; }
    }
}