using Laser.Orchard.NwazetIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class ProductVatConfigurationPartInputPriceSettingsVM {

        public ProductVatConfigurationPartInputPriceSettingsVM() {
            InputFinalPrice = false;
        }

        public ProductVatConfigurationPartInputPriceSettingsVM(
            ProductVatConfigurationPartInputPriceSettings settings) 
            : this() {
            if (settings != null) {
                InputFinalPrice = settings.InputFinalPrice;
            }
        }

        public bool InputFinalPrice { get; set; }
    }
}