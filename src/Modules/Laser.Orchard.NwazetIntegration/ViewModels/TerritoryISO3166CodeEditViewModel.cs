using Laser.Orchard.NwazetIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class TerritoryISO3166CodeEditViewModel {
        public TerritoryISO3166CodeEditViewModel() { }
        public TerritoryISO3166CodeEditViewModel(
            TerritoryISO3166CodePart part) {

            ISO3166Code = part.ISO3166Code;
        }

        public string ISO3166Code { get; set; }
    }
}