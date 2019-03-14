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
                foreach (var kvp in splitParameters.Select((val, i) => new { i, val })) {
                    queryParams.Add($"param{kvp.i.ToString()}", kvp.val);
                }

                // This will be a filter on ContentItems
                Action<IAliasFactory> alias = af => af.ContentItem();
                Action<IHqlExpressionFactory> predicate = ef => ef.InSubquery("Id", query, queryParams);
                context.Query.Where(alias, predicate);
            }
        }

        //private void SafeSubQuery(
        //    IHqlExpressionFactory ef,
        //    string propertyName,
        //    string subquery, 
        //    Dictionary<string, object> parameters) {

        //    // NHibernate fails to assign values to all instances of a same parameter.
        //    // As a workaround, we create a new "copy" of the query, to handle multiplicity.
        //    var qString = subquery;
        //    // We use the base query to compute the substitutions in the actual query.
        //    var baseQuery = _session.CreateQuery(subquery);
        //    // Dictionary of the substitutions. Keys are the parameter names in the "new" query.
        //    // Values are the "old" parameter names. 
        //    var substitutionMap = new Dictionary<string, string>();
        //    foreach (var para in baseQuery.NamedParameters)
        //    {
        //        var pCode = $":{para}"; // Parameters in the query are prepended by :
        //        var rgx = new Regex(pCode);
        //        // while there is a match with the original parameter, replace it
        //        for (Match match = rgx.Match(qString); match.Success; match = rgx.Match(qString))
        //        {
        //            var newParam = $"param_{substitutionMap.Count}";
        //            qString = rgx.Replace(
        //                qString,
        //                $":{newParam}",
        //                1,
        //                (match.Index - 1) < 0 ? 0 : (match.Index));
        //            // add the information on the substitution to the map
        //            substitutionMap.Add(newParam, para);
        //        }
        //    }
        //    // Now create the actual query
        //    var hqlQuery = _session.CreateQuery(qString);
        //    foreach (var para in hqlQuery.NamedParameters)
        //    {
        //        // get the original parameter name
        //        var oldPara = substitutionMap[para];
        //        // OWASP says nhibernate's SetParameter is safe
        //        if (parameters.ContainsKey(oldPara))
        //        {
        //            hqlQuery.SetParameter(para, parameters[oldPara]);
        //        }
        //        else
        //        {
        //            hqlQuery.SetParameter(para, "");
        //        }
        //    }
        //    // A lot of assumptions here that are not enforced in code.The main thing is
        //    //that we are basically assuming that the query has been written to return Ids.
        //    ef.InG(propertyName, hqlQuery.List<int>());
        //}

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Filter ContentItems using a parametrized query.");
        }
    }
}