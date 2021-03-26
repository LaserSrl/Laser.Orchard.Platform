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
            HasCities = part.HasCities;
            HasProvinces = part.HasProvinces;
        }

        public TerritoryAdministrativeType AdministrativeType { get; set; }
        public bool HasCities { get; set; }
        public bool HasProvinces { get; set; }

        public IEnumerable<SelectListItem> AdministrativeTypes { get; set; }
    }
}