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
using Laser.Orchard.Queries.Models;
using NHibernate;
using System.Text.RegularExpressions;
using NHibernate.Transform;
using Orchard.Fields.Fields;

namespace Laser.Orchard.Queries.Services {
    public interface IQueryPickerService : IDependency {
        List<QueryPart> GetUserDefinedQueries();
        List<QueryPart> GetOneShotQueries();
        List<TitlePart> GetHqlQueries();
        IHqlQuery GetCombinedContentQuery(int[] queryIds, Dictionary<string, object> tokens = null, string[] contentTypesToFilter = null);
        IQuery GetCustomQuery(int queryId, Dictionary<string, object> tokens, bool count = false);
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

        public List<TitlePart> GetHqlQueries() {
            var availableQueries = _services.ContentManager.Query<TitlePart>().ForType("MyCustomQuery").List();
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

        public IQuery GetCustomQuery(int queryId, Dictionary<string, object> tokens, bool count = false) {
            var customQueryContent = _services.ContentManager.Get(queryId);
            string parameters = ((dynamic)customQueryContent)
                .MyCustomQueryPart.QueryParameterValues.Value;
            bool hasParameters = !string.IsNullOrWhiteSpace(parameters); // before tokens substitution because tokens can result in an empty value
            string query = ((dynamic)customQueryContent)
                .MyCustomQueryPart.QueryString.Value;
            var sqlField = customQueryContent.Parts
                .FirstOrDefault(x => x.PartDefinition.Name == "MyCustomQueryPart")
                .Get(typeof(BooleanField), "IsSQL") as BooleanField;
            var isSql = sqlField == null
                ? false
                : sqlField.Value.HasValue ? sqlField.Value.Value : false;
            // Apply tokenization for parameters and query
            parameters = _tokenizer.Replace(parameters, tokens);
            query = _tokenizer.Replace(query, tokens);
            if (string.IsNullOrWhiteSpace(query)) {
                return null;
            }
            Dictionary<string, object> queryParams = new Dictionary<string, object>();
            bool cacheable = true;
            // check on query: must start with the word "select"
            var startsWithSelect = query
                // TrimStart() removes all whitespace from the beginning of the query,
                // including newline characters
                .TrimStart()
                .StartsWith("select", StringComparison.InvariantCultureIgnoreCase)
                // select is followed by whitespace
                && char.IsWhiteSpace(query.TrimStart(), 6);
            if (startsWithSelect) {
                if (count) { // 
                    var splittedQuery = query.Split(new string[] { "\n", " ", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    var indexOfFromWord = Array.FindIndex(splittedQuery, x => x.ToLower() == "from");
                    var indexOfOrderWord = Array.FindLastIndex(splittedQuery, x => x.ToLower() == "order");
                    query = "select count(*) " + string.Join(" ", splittedQuery, indexOfFromWord, (indexOfOrderWord > 0 ? indexOfOrderWord : splittedQuery.Length) - indexOfFromWord);
                }
                var session = _services.TransactionManager.GetSession();
                IQuery hql = isSql
                    ? session // SQL
                        .CreateSQLQuery(query)
                    : session // HQL
                        .CreateQuery(query);
                hql.SetCacheable(cacheable);

                if (hasParameters) {
                    // Parse parameters:
                    // The correct way to input parameters in the text area is to have one parameter
                    // per line and end each line with a comma.
                    var splitParameters = parameters.Split(
                        new string[] { ",\n", "," + Environment.NewLine },
                        StringSplitOptions.None); // keep empty entries
                                                  // we don't trim values in case whitespace is desired.
                                                  // Parameters are always assumed to be strings.

                    foreach (var kvp in splitParameters.Select((val, i) => new { i, val })) {
                        hql.SetParameter("param" + kvp.i, kvp.val);
                    }
                }
                return hql;
            }
            else {
                throw new Exception("Query should starts with \"select\" keyword.\r\nQuery is:\r\n" + query);
            }
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