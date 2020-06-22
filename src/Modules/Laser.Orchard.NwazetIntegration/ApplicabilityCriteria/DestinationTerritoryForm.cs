using Laser.Orchard.NwazetIntegration.Services;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class DestinationTerritoryForm : IFormProvider {
        private readonly IAddressConfigurationSettingsService _addressConfigurationSettingsService;

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public static string FormName = "DestinationTerritoryForm";

        public DestinationTerritoryForm(
            IShapeFactory shapeFactory,
            IAddressConfigurationSettingsService addressConfigurationSettingsService) {

            _addressConfigurationSettingsService = addressConfigurationSettingsService;

            Shape = shapeFactory;

            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form = shape => {
                var f = Shape.Form(
                    Id: "DestinationTerritoryCriterion",
                    _DestinationTerritories: Shape.SelectList(
                            Id: "territories", Name: "Territories",
                            Title: T("Territories"),
                            Description: T("Select the valid territories for this shipping method."),
                            Size: 10,
                            Multiple: true
                            ));

                // "Any territory" option
                f._DestinationTerritories.Add(
                    new SelectListItem { Value = "-1", Text = T("Any territory").Text });

                // get the list of valid territories for shipping
                var hierarchy = _addressConfigurationSettingsService
                    .ShippingCountriesHierarchy;
                // Even if the territory itself is not configured as one where we ship, we
                // should still show it here, because it may contain valid destinations.

                return false;
            };

            context.Form(FormName, form);
        }
    }
}