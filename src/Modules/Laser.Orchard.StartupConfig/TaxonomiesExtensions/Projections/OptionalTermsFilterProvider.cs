using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using OPServices = Orchard.Projections.Services;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Projections {
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class OptionalTermsFilterProvider : OPServices.IFilterProvider {
        private readonly ITaxonomyService _taxonomyService;

        public OptionalTermsFilterProvider(
            ITaxonomyService taxonomyService) {

            _taxonomyService = taxonomyService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe
                .For("Taxonomy", T("Taxonomy"), T("Taxonomy"))
                .Element("HasTermsIfAny",
                    T("Has the selected terms (if any was selected)"),
                    T("ContentItems with the given terms, or any ContentItem if no term si provided to the query."),
                    ApplyFilter,
                    DisplayFilter,
                    "SelectTermsForm"); ;
        }

        private Func<ITaxonomyService, int, IEnumerable<TermPart>> termsWithoutChildren = (service, tid) => {
            var term = service.GetTerm(tid);
            if (term == null) {
                return Enumerable.Empty<TermPart>();
            }
            var ts = new List<TermPart>() { term };
            return ts;
        };
        private Func<ITaxonomyService, int, IEnumerable<TermPart>> termsWithChildren = (service, tid) => {
            var term = service.GetTerm(tid);
            if (term == null) {
                return Enumerable.Empty<TermPart>();
            }
            var ts = new List<TermPart>() { term };
            ts.AddRange(service.GetChildren(term));
            return ts;
        };

        public void ApplyFilter(FilterContext context) {
            var termsString = (string)context.State.Terms;
            // Default action does nothing
            List<Action<FilterContext>> ActualFilters = new List<Action<FilterContext>>() { fc => { } };
            if (!string.IsNullOrWhiteSpace(termsString)) {
                var termIds = termsString
                    .Split(new char[] { ',' })
                    .Select(int.Parse);
                if (termIds.Any()) {
                    // if there are no term ids, it makes no sense to be here
                    Func<int, IEnumerable<TermPart>> selectManyExpression;
                    bool.TryParse(context.State.IncludeChildren?.Value, out bool includeChildren);
                    if (includeChildren) {
                        selectManyExpression = tid => termsWithChildren(_taxonomyService, tid);
                    }
                    else {
                        selectManyExpression = tid => termsWithoutChildren(_taxonomyService, tid);
                    }
                    var terms = termIds
                        .SelectMany(selectManyExpression)
                        .Distinct();
                    if (terms.Any()) {
                        // if there are no terms, it makes no sense to be here
                        var allIds = terms.Select(t => t.Id);
                        int op = Convert.ToInt32(context.State.Operator);
                        switch (op) {
                            case 0: // is one of
                                // Unique alias so we always get a unique join everytime so can have > 1 HasTerms filter on a query.
                                // define the Action that will actually do a query
                                ActualFilters.Add(fc => {
                                    fc.Query.Where(
                                        x => x.ContentPartRecord<TermsPartRecord>()
                                            .Property("Terms", "terms" + Guid.NewGuid().ToString()),
                                        c => c.InG("TermRecord.Id", allIds.ToList()));
                                });
                                break;
                            case 1: // is all of
                                ActualFilters.AddRange(allIds.Select(tid => {
                                    Action<FilterContext> af = fc => fc.Query.Where(
                                        x => x.ContentPartRecord<TermsPartRecord>()
                                            .Property("Terms", "terms" + tid),
                                        c => c.Eq("TermRecord.Id", tid));
                                    return af;
                                }));
                                break;
                        }
                    }
                }
            }
            // execute the filter
            foreach (var filter in ActualFilters) {
                filter(context);
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("For the selected terms (or any)");
        }
    }
}