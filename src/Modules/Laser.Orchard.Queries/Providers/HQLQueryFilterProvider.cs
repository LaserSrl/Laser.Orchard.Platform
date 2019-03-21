using NHibernate;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Laser.Orchard.StartupConfig.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OPServices = Orchard.Projections.Services;

namespace Laser.Orchard.Queries.Providers {
    [OrchardFeature("Laser.Orchard.HQLProjectionFilter")]
    public class HQLQueryFilterProvider : OPServices.IFilterProvider {
        private readonly ISession _session;
        private readonly ITransactionManager _transactionManager;

        public HQLQueryFilterProvider(
            ITransactionManager transactionManager) {

            _transactionManager = transactionManager;
            _session = _transactionManager.GetSession();

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
                // Parse parameters:
                // The correct way to input parameters in the text area is to have one parameter
                // per line and end each line with a comma.
                var splitParameters = parameters.Split(
                    new string[] { ",\n", "," + Environment.NewLine },
                    StringSplitOptions.None); // keep empty entries
                // we don't trim values in case whitespace is desired.
                // Parameters are always assumed to be strings.
                foreach (var kvp in splitParameters.Select((val, i) => new { i, val })) {
                    queryParams.Add($"param{kvp.i.ToString()}", kvp.val);
                }

                // This will be a filter on ContentItems
                Action<IAliasFactory> alias = af => af.ContentItem();
                Action<IHqlExpressionFactory> predicate = ef => ef.InSubquery("Id", query, queryParams);
                context.Query.Where(alias, predicate);
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Filter ContentItems using a parametrized query.");
        }
    }
}