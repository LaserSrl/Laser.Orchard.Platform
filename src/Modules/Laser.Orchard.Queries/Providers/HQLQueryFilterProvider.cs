using Laser.Orchard.StartupConfig.Projections;
using Orchard.Environment.Extensions;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OPServices = Orchard.Projections.Services;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Localization;
using Orchard.ContentManagement;

namespace Laser.Orchard.Queries.Providers {
    [OrchardFeature("Laser.Orchard.HQLProjectionFilter")]
    public class HQLQueryFilterProvider : OPServices.IFilterProvider {

        public HQLQueryFilterProvider() {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe
                .For("Search", T("HQL Query"), T("Allows using a parametrized HQL query to select content items."))
                .Element("HQLQuery",
                    T("HQL Query"),
                    T("Allows using a parametrized HQL query to select content items."),
                    ApplyFilter,
                    DisplayFilter,
                    "ParametrizedHQLForm");
        }

        public void ApplyFilter(FilterContext context) {
            var query = (string)context.State.Query;
            if (!string.IsNullOrWhiteSpace(query)) {
                // parameters here have already had tokens replaced by values
                var parameters = (string)context.State.Parameters;
                Dictionary<string, object> queryParams = new Dictionary<string, object>();
                if (!string.IsNullOrWhiteSpace(parameters)) {
                    var ps = parameters.Split(
                        new string[] { Environment.NewLine, "\n" },
                        StringSplitOptions.None)
                        .Select(p => p.Trim())
                        .Select((v, i) => new Tuple<int, string>(i, v));
                    foreach (var tuple in ps) {
                        queryParams.Add($"param{tuple.Item1.ToString()}", tuple.Item2);
                    }
                }


                // This will be a filter on ContentItems
                Action<IAliasFactory> alias = af => af.ContentItem();
                Action<IHqlExpressionFactory> predicate = ef =>
                    ef.InSubquery("Id", query, queryParams);
                context.Query.Where(alias, predicate);
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Filter ContentItems using a parametrized query.");
        }
    }
}