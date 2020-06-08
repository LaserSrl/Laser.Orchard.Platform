using Laser.Orchard.NwazetIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class TerritoryAddressTypeEditViewModel {
        public TerritoryAddressTypeEditViewModel() { }
        public TerritoryAddressTypeEditViewModel(
            TerritoryAddressTypePart part) {

            Shipping = part.Shipping;
            Billing = part.Billing;
        }

        public bool Shipping { get; set; }
        public bool Billing { get; set; }
    }
}