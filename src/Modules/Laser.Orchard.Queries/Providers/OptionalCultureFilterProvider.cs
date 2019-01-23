using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OPServices = Orchard.Projections.Services;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Localization.Models;

namespace Laser.Orchard.Queries.Providers {
    [OrchardFeature("Laser.Orchard.OptionalFilters")]
    public class OptionalCultureFilterProvider : OPServices.IFilterProvider {
        private readonly ICultureManager _cultureManager;

        public OptionalCultureFilterProvider(
            ICultureManager cultureManager) {

            _cultureManager = cultureManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe
                .For("Localization", T("Localization"), T("Localization"))
                .Element("ForSelectedCultureOrAny",
                    T("Selected culture or any"),
                    T("Localized content items for a given culture (or any culture if none provided)."),
                    ApplyFilter,
                    DisplayFilter,
                    "SelectCultureForm"); ;
        }

        public void ApplyFilter(FilterContext context) {
            var cultureName = (string)context.State.Culture;
            // Default action does nothing
            Action<FilterContext> ActualFilter = fc => { };
            if (!string.IsNullOrWhiteSpace(cultureName)) {
                var cultures = _cultureManager.ListCultures();
                // get the culture with the given name
                var culture = cultures
                    .FirstOrDefault(c => c.Equals(cultureName, StringComparison.InvariantCultureIgnoreCase));
                if (culture != null) {
                    var cultureRecord = _cultureManager.GetCultureByName(culture);
                    if (cultureRecord != null) { //sanity check
                        // define the Action that will actually do a query
                        ActualFilter = fc => {
                            fc.Query.Where(
                                x => x.ContentPartRecord<LocalizationPartRecord>(),
                                c => c.Eq("CultureId", cultureRecord.Id));
                        };
                    }
                }
            }
            // execute the filter
            ActualFilter(context);
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("For the selected culture (or any)");
        }
    }
}