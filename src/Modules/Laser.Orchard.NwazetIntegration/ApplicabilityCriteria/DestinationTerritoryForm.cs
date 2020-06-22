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

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public static string FormName = "DestinationTerritoryForm";

        public DestinationTerritoryForm(
            IShapeFactory shapeFactory) {
            
            Shape = shapeFactory;

            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form = shape => {
                var f = Shape.Form(
                    Id: "DestinationTerritoryCriterion",
                    _DestinationTerritories: Shape.TerritorySelector(
                        Id: "territories", Name: "Territories"
                        ));
                // The TerritorySelector shape will handle setting things up to have an autocomplete
                // based on ajax.
                return f;
            };

            context.Form(FormName, form);
        }
    }
}