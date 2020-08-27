using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class ProductPriceEditorViewModel {

        public ProductPriceEditorViewModel() {
            VatRates = new Dictionary<int, decimal>();
        }

        public decimal BasePrice { get; set; }
        public decimal DiscountPrice { get; set; }

        public int SelectedVatConfigurationId { get; set; }
        public int DefaultVatConfigurationId { get; set; }

        public string DefaultTerritoryName { get; set; }

        public Dictionary<int, decimal> VatRates { get; set; }
    }
}