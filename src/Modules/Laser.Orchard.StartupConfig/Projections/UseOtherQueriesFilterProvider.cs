using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections;
using Orchard.Projections.Descriptors.Filter;
using OPS = Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Projections.Models;
using Orchard.Core.Title.Models;
using System.Web.Mvc;
using Orchard.ContentManagement.Handlers;
using Orchard.Tokens;
using Orchard.Forms.Services;
using Orchard.Projections.Descriptors;
using Orchard.Logging;

namespace Laser.Orchard.StartupConfig.Projections {
    public class UseOtherQueriesFilterProvider :
        OPS.IFilterProvider {
        private readonly IContentManager _contentManager;
        private readonly ITokenizer _tokenizer;
        private readonly IWorkContextAccessor _workContextAccessor;
        public UseOtherQueriesFilterProvider(
            IContentManager contentManager,
            ITokenizer tokenizer,
            IWorkContextAccessor workContextAccessor) {

            _contentManager = contentManager;
            _tokenizer = tokenizer;
            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe
                .For("Content", T("Content"), T("Content"))
                .Element("OrchardQuery",
                    T("Orchard Query"),
                    T("Allows using a different Orchard Query to filter content results."),
                    ApplyFilter,
                    DisplayFilter,
                    UseOtherQueriesFilterProviderForm.FormName);
        }

        public void ApplyFilter(FilterContext context) {

            var queryId = (string)context.State.QueryId;
            int id = 0;
            if (!string.IsNullOrWhiteSpace(queryId)
                && int.TryParse(queryId, out id)) {
                var queryPart = _contentManager.Get<QueryPart>(id);
                if (queryPart != null && allFilterProviders.Any()) {
                    var recursionGuardString = (string)context.State.OtherQueriesFilterRecursionGuard ?? "";
                    var recursionGuard = recursionGuardString
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
                    // prevent processing an "inner" query if it's the same as the "outer" query
                    recursionGuard.Add(context.QueryPartRecord.Id);
                    if (!recursionGuard.Contains(queryPart.Id)) {
                        // we haven't processed this filter before in this query
                        // prevent processing the same filter recursively several times
                        recursionGuard.Add(queryPart.Id);
                        recursionGuardString = string.Join(",", recursionGuard.Select(i => i.ToString()));
                        var queryRecord = queryPart.Record;
                        // pretend we are a ProjectionManager
                        var tokens = context.Tokens;
                        foreach (var group in queryRecord.FilterGroups) {
                            var contentQuery = context.Query;
                            foreach (var filter in group.Filters) {
                                var tokenizedState = _tokenizer.Replace(filter.State, tokens);
                                var dynState = FormParametersHelper.ToDynamic(tokenizedState);
                                dynState.OtherQueriesFilterRecursionGuard = recursionGuardString;
                                var innerContext = new FilterContext {
                                    Query = contentQuery,
                                    State = dynState,
                                    QueryPartRecord = queryRecord,
                                    Tokens = tokens
                                };

                                string category = filter.Category;
                                string type = filter.Type;

                                var descriptor = AllDescriptors()
                                    .FirstOrDefault(d => d.Category == category && d.Type == type);
                                if (descriptor == null) {
                                    continue;
                                }
                                // Apply filters
                                descriptor.Filter(innerContext);
                                contentQuery = innerContext.Query;
                            }
                            context.Query = contentQuery;
                        }
                    } else {
                        Logger.Warning(T("Prevent recursive execution of filter with Id {0}.", queryPart.Id).Text);
                    }
                }
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {

            var queryId = (string)context.State.QueryId;
            int id = 0;
            if (!string.IsNullOrWhiteSpace(queryId)
                && int.TryParse(queryId, out id)) {
                var query = _contentManager.Get<QueryPart>(id);
                if (query != null) {
                    var dt = _contentManager.GetItemMetadata(query).DisplayText;
                    return T("Filter content items based off the query: {0}.", dt);
                }
            }

            return T("Filter content items based off another query.");
        }

        private IEnumerable<IFilterProvider> _allFilterProviders;
        private IEnumerable<IFilterProvider> allFilterProviders {
            get {
                if (_allFilterProviders == null) {
                    var workContext = _workContextAccessor.GetContext();
                    IEnumerable<IFilterProvider> provs;
                    if (workContext.TryResolve(out provs)) {
                        _allFilterProviders = provs;
                    } else {
                        _allFilterProviders = Enumerable.Empty<IFilterProvider>();
                    }
                }
                return _allFilterProviders;
            }
        }
        private IEnumerable<TypeDescriptor<FilterDescriptor>> DescribeFilters(
            IEnumerable<IFilterProvider> filterProviders) {
            var context = new DescribeFilterContext();
            foreach (var provider in filterProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }
        private IEnumerable<FilterDescriptor> _allDescriptors { get; set; }
        private IEnumerable<FilterDescriptor> AllDescriptors() {
            if (_allDescriptors == null) {
                _allDescriptors = DescribeFilters(allFilterProviders)
                    .SelectMany(x => x.Descriptors);
            }
            return _allDescriptors;
        }
    }

    public class UseOtherQueriesFilterProviderForm : IFormProvider {
        protected dynamic Shape { get; set; }
        private readonly IContentManager _contentManager;

        public static string FormName = "UseOtherQueriesFilterProviderForm";
        public Localizer T { get; set; }

        public UseOtherQueriesFilterProviderForm(
            IShapeFactory shapeFactory,
            IContentManager contentManager) {

            Shape = shapeFactory;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context) {
            Func<IShapeFactory, object> form =
                shape => {
                    var f = Shape.Form(
                        Id: "UseOtherQuery",
                        _QueryId: Shape.SelectList(
                            Id: "queryid", Name: "QueryId",
                            Title: T("Content Queries"),
                            Description: T("Select a content query.")),
                            Size: 10
                        );

                    var allQueries = _contentManager
                        .Query<QueryPart, QueryPartRecord>()
                        .Join<TitlePartRecord>().OrderBy(x => x.Title)
                        .List();

                    foreach (var query in allQueries) {
                        f._QueryId.Add(new SelectListItem {
                            Value = query.Id.ToString(),
                            Text = query.Name
                        });
                    }

                    return f;
                };

            context.Form(FormName, form,
                (Action<dynamic, ImportContentContext>)Import,
                (Action<dynamic, ExportContentContext>)Export);
        }
        public void Export(dynamic state, ExportContentContext context) {
            var queryId = Convert.ToString(state.QueryId);
            int id = 0;
            if (!string.IsNullOrWhiteSpace(queryId)
                && int.TryParse(queryId, out id)) {
                var query = context.ContentManager.Get<QueryPart>(id);
                if (query != null) {
                    var identity = context.ContentManager.GetItemMetadata(query).Identity.ToString();
                    state.QueryId = identity;
                } else {
                    state.QueryId = string.Empty;
                }
            }
        }

        public void Import(dynamic state, ImportContentContext context) {
            string queryIdentity = Convert.ToString(state.QueryId);
            if (!string.IsNullOrWhiteSpace(queryIdentity)) {
                var query = context.GetItemFromSession(queryIdentity);
                if (query != null) {
                    state.QueryId = query.Id.ToString();
                } 
            }
        }
    }
}