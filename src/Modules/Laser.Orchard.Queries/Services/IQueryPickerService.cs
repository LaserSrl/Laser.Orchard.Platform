using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Tokens;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Forms.Services;
using Orchard.Data;

namespace Laser.Orchard.Queries.Services {
    public interface IQueryPickerService : IDependency {
        List<QueryPart> GetUserDefinedQueries();
        List<QueryPart> GetOneShotQueries();
        IHqlQuery GetCombinedContentQuery(int[] queryIds, Dictionary<string, object> tokens = null, string[] contentTypesToFilter = null);
    }


    public class QueryPickerDefault : IQueryPickerService {
        private readonly IOrchardServices _services;
        private readonly IEnumerable<IFilterProvider> _filterProviders;
        private readonly IContentManager _contentManager;
        private readonly ITokenizer _tokenizer;
        private readonly IProjectionManager _projectionManager;
        private readonly IRepository<QueryPartRecord> _queryRepository;

        public QueryPickerDefault(IOrchardServices services, IContentManager contentManager,
                        ITokenizer tokenizer,
                        IProjectionManager projectionManager,
                        IEnumerable<IFilterProvider> filterProviders,
            IRepository<QueryPartRecord> queryRepository) {
            _services = services;
            _tokenizer = tokenizer;
            _contentManager = contentManager;
            _projectionManager = projectionManager;
            _queryRepository = queryRepository;
            _filterProviders = filterProviders;

        }

        public List<QueryPart> GetUserDefinedQueries() {
            var availableQueries = _services.ContentManager.Query().ForType("Query").Join<TitlePartRecord>().OrderBy(x => x.Title).List()
                .Where(x => ((dynamic)x).QueryUserFilterExtensionPart.UserQuery.Value == true && (((dynamic)x).QueryUserFilterExtensionPart.OneShotQuery.Value ?? false) == false)
                .Select(x => x.As<QueryPart>());
            return availableQueries.ToList();
        }

        public List<QueryPart> GetOneShotQueries() {
            var availableQueries = _services.ContentManager.Query().ForType("Query").Join<TitlePartRecord>().OrderBy(x => x.Title).List()
                .Where(x => ((dynamic)x).QueryUserFilterExtensionPart.UserQuery.Value == true && ((dynamic)x).QueryUserFilterExtensionPart.OneShotQuery.Value == true)
                .Select(x => x.As<QueryPart>());
            return availableQueries.ToList();
        }
        public IHqlQuery GetCombinedContentQuery(int[] queryIds, Dictionary<string, object> tokens = null, string[] contentTypesToFilter = null) {
            var availableFilters = DescribeFilters().ToList();

            IEnumerable<ContentItem> contentItems = new List<ContentItem>();
            IHqlQuery contentQuery = null;
            contentQuery = _contentManager.HqlQuery();
            // Siccome sono in una Query Definita dallo User, devo anche filtrare per ContentType "CommunicationContact"
            if (contentTypesToFilter != null)
                contentQuery = contentQuery.ForType(contentTypesToFilter);
            foreach (var queryId in queryIds) {
                var queryRecord = _queryRepository.Get(queryId);
                // Per un bug di Orchard non si devono usare i gruppi, quindi prendo solo il primo gruppo
                var group = queryRecord.FilterGroups[0];
                // fatto da HERMES
                if (tokens == null) {
                    tokens = new Dictionary<string, object>();
                }
                //FINE
                // iterate over each filter to apply the alterations to the query object
                foreach (var filter in group.Filters) {
                    var tokenizedState = _tokenizer.Replace(filter.State, tokens /*new Dictionary<string, object>()*/);
                    var filterContext = new FilterContext {
                        Query = contentQuery,
                        State = FormParametersHelper.ToDynamic(tokenizedState),
                        QueryPartRecord = queryRecord
                    };

                    string category = filter.Category;
                    string type = filter.Type;

                    // look for the specific filter component
                    var descriptor = availableFilters
                        .SelectMany(x => x.Descriptors)
                        .FirstOrDefault(x => x.Category == category && x.Type == type);

                    // ignore unfound descriptors
                    if (descriptor == null) {
                        continue;
                    }

                    // apply alteration
                    descriptor.Filter(filterContext);

                    contentQuery = filterContext.Query;
                }
            }
            // Filtro sullo stato di pubblicato
            contentQuery = contentQuery.ForVersion(VersionOptions.Published);
            return contentQuery;
        }
        private IEnumerable<TypeDescriptor<FilterDescriptor>> DescribeFilters() {
            var context = new DescribeFilterContext();

            foreach (var provider in _filterProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

    }
}