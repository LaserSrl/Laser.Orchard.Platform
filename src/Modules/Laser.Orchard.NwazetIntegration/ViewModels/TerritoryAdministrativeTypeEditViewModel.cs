using Laser.Orchard.NwazetIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class TerritoryAdministrativeTypeEditViewModel {
        public TerritoryAdministrativeTypeEditViewModel() { }
        public TerritoryAdministrativeTypeEditViewModel(
            TerritoryAdministrativeTypePart part) {
            AdministrativeType = part.AdministrativeType;
        }

        public TerritoryAdministrativeType AdministrativeType { get; set; }


        public IEnumerable<SelectListItem> AdministrativeTypes { get; set; }
    }
}