using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class ProductVatConfigurationPartInputPriceSettings {

        public ProductVatConfigurationPartInputPriceSettings() {
            InputFinalPrice = false;
        }

        /// <summary>
        /// Controls whether users will be allowed to input the final price
        /// (i.e. the price including VAT), rather than the taxable amount.
        /// </summary>
        public bool InputFinalPrice { get; set; }
    }
}