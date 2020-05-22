using Newtonsoft.Json;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Projections {
    public class ConfigurableSortCriterionProvider : ISortCriterionProvider {

        public ConfigurableSortCriterionProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeSortCriterionContext describe) {
            describe.For("General", T("General"), T("General sort criteria"))
                .Element("ConfigurableSorting",
                    T("Configurable Sort Criteria"),
                    T("Sorts the results by choosing the criterion from a list"),
                    context => ApplyFilter(context),
                    context => T("Order based off a variable configuration"),
                    ConfigurableSortCriterionProviderForm.FormName);
        }

        public void ApplyFilter(SortCriterionContext context) {
            var json = (string)context.State.CriteriaArray;
            var indexStr = (string)context.State.CriterionIndex;
            int criterionIndex = 0;
            int.TryParse(indexStr, out criterionIndex);
            try {
                var criteriaArray = JsonConvert
                    .DeserializeObject<SortCriterionConfiguration[]>(json);
                if (criteriaArray != null && criteriaArray.Any()) {
                    if (criterionIndex >= criteriaArray.Count()) {
                        // given index is out of range
                        criterionIndex = 0;
                    }
                    // get the ith criterion
                    var criterionToUse = criteriaArray[criterionIndex];
                }
            } catch (Exception) {
                // impossible to parse the array
            }
        }
    }
}