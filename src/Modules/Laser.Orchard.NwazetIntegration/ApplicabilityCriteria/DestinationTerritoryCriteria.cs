using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class DestinationTerritoryCriteria : IApplicabilityCriterionProvider {

        public Localizer T { get; set; }
        public void Describe(DescribeCriterionContext describe) {
            describe
                .For("Destination", 
                    T("Destination in territories"), 
                    T("Destination in territories"))
                .Element("Destination in territories",
                    T("Destination in territories"),
                    T("Destination in territories Criterion"),
                    ApplyCriteria,
                    DisplayCriteria,
                    DestinationTerritoryForm.FormName);
        }

        public void ApplyCriteria(CriterionContext context) {
            // TODO
            // in the postalcode we pass, after the postal code, 
            // a list of territory Ids separated by semicolon.
            var zipSplit = context
                .ApplicabilityContext
                .ZipCode
                .Split(new char[] { ';' }, StringSplitOptions.None);
            if (zipSplit.Any()) {
                var ids = zipSplit
                    .Skip(1)
                    .Select(s => {
                        int tmp = 0;
                        int.TryParse(s, out tmp);
                        return tmp;
                    })
                    .Where(i => i > 0);
                // these ids are for the InternalTerritoryRecords
                // As soon as one of them is among the configured territories for
                // the criterion, or is a child of a configured territory, the
                // criterion is known to apply.
            }
        }

        public LocalizedString DisplayCriteria(CriterionContext context) {
            return T("Destination is in a group of territories (perhaps as a child).");
        }
    }
}