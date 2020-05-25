using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Forms.Services;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Laser.Orchard.Reporting.Models;
using Laser.Orchard.Reporting.Providers;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Core.Common.Fields;
using System.Collections;
using NHibernate.Transform;
using NHibernate;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Logging;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using Orchard.Core.Contents;
using Orchard.Fields.Fields;

namespace Laser.Orchard.Reporting.Services {
    public class ReportManager : IReportManager
    {
        private readonly IEnumerable<IGroupByParameterProvider> groupByProviders;
        private readonly IContentManager contentManager;
        private readonly IProjectionManager projectionManager;
        private readonly ITokenizer _tokenizer;
        private readonly IRepository<QueryPartRecord> queryRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuthorizer _authorizer;
        private Dictionary<int, Permission> _reportPermissions;
        private Dictionary<int, Permission> _dashboardPermissions;
        private readonly IRepository<ReportRecord> _reportRepository;
        public Localizer T { get; set; }
        public ILogger Log { get; set; }

        public ReportManager(
            IRepository<QueryPartRecord> queryRepository,
            IRepository<ReportRecord> reportRepository,
            IProjectionManager projectionManager,
            IEnumerable<IGroupByParameterProvider> groupByProviders,
            IContentManager contentManager,
            ITokenizer tokenizer,
            ITransactionManager transactionManager,
            IAuthorizer authorizer)
        {
            this.queryRepository = queryRepository;
            _reportRepository = reportRepository;
            this.projectionManager = projectionManager;
            _tokenizer = tokenizer;
            this.contentManager = contentManager;
            this.groupByProviders = groupByProviders;
            _transactionManager = transactionManager;
            _authorizer = authorizer;
            T = NullLocalizer.Instance;
            Log = NullLogger.Instance;
        }

        public IEnumerable<TypeDescriptor<GroupByDescriptor>> DescribeGroupByFields()
        {
            DescribeGroupByContext context = new DescribeGroupByContext();
            foreach (var provider in this.groupByProviders)
            {
                provider.Describe(context);
            }

            return context.Describe();
        }

        public int GetCount(ReportRecord report, IContent container)
        {
            if (report == null) { throw new ArgumentNullException("report"); }
            if (report.Query == null) { throw new ArgumentException("There is no QueryRecord associated with the Report"); }

            var descriptors = this.DescribeGroupByFields();
            var descriptor = descriptors.SelectMany(c => c.Descriptors).FirstOrDefault(c => c.Category == report.GroupByCategory && c.Type == report.GroupByType);

            if (descriptor == null)
            {
                throw new ArgumentOutOfRangeException("There is no GroupByDescriptor for the given category and type");
            }

            var queryRecord = this.queryRepository.Get(report.Query.Id);

            var contentQueries = this.GetContentQueries(queryRecord, queryRecord.SortCriteria, container);

            return contentQueries.Sum(c => c.Count());
        }

        public IEnumerable<AggregationResult> RunReport(ReportRecord report, IContent container)
        {
            if (report == null) { throw new ArgumentNullException("report"); }
            if (report.Query == null) { throw new ArgumentException("There is no QueryRecord associated with the Report"); }

            var descriptors = this.DescribeGroupByFields();
            var descriptor = descriptors.SelectMany(c => c.Descriptors).FirstOrDefault(c => c.Category == report.GroupByCategory && c.Type == report.GroupByType);

            if (descriptor == null)
            {
                throw new ArgumentOutOfRangeException("There is no GroupByDescriptor for the given category and type");
            }

            var queryRecord = this.queryRepository.Get(report.Query.Id);

            var contentQueries = this.GetContentQueries(queryRecord, queryRecord.SortCriteria, container);

            Dictionary<string, AggregationResult> returnValue = new Dictionary<string, AggregationResult>();

            foreach (var contentQuery in contentQueries)
            {
                var dictionary = descriptor.Run(contentQuery, (AggregateMethods)report.AggregateMethod);

                foreach (var item in dictionary)
                {
                    if (returnValue.ContainsKey(item.Label))
                    {
                        var previousItem = returnValue[item.Label];
                        previousItem.AggregationValue += item.AggregationValue;
                        returnValue[item.Label] = previousItem;
                    }
                    else
                    {
                        returnValue[item.Label] = item;
                    }
                }
            }

            return returnValue.Values;
        }

        public IEnumerable<AggregationResult> RunHqlReport(ReportRecord report, IContent container, bool multiColumnTable = false) {
            if (report == null) { throw new ArgumentNullException("report"); }
            if (report.Query == null) { throw new ArgumentException("There is no QueryRecord associated with the Report"); }

            var queryRecord = contentManager.Get(report.Query.Id);
            var contentQuery = queryRecord.Parts.FirstOrDefault(x => x.PartDefinition.Name == "MyCustomQueryPart");
            if(contentQuery == null) {
                throw new ArgumentOutOfRangeException("HQL query not valid.");
            }
            var queryField = contentQuery.Get(typeof(TextField), "QueryString") as TextField;
            string parameters = (contentQuery.Get(typeof(TextField), "QueryParameterValues") as TextField).Value;
            bool hasParameters = !string.IsNullOrWhiteSpace(parameters); // before tokens substitution because tokens can result in an empty value
            var sqlField = contentQuery.Get(typeof(BooleanField), "IsSQL") as BooleanField;
            var isSql = sqlField == null
                ? false
                : sqlField.Value.HasValue ? sqlField.Value.Value : false; 
            var query = queryField.Value.Trim();
            // tokens replacement
            Dictionary<string, object> contextDictionary = new Dictionary<string, object>();
            if (container != null) {
                contextDictionary.Add("Content", container);
            }
            query = _tokenizer.Replace(query, contextDictionary);
            parameters = _tokenizer.Replace(parameters, contextDictionary);
            IQuery hql = null;
            Dictionary<string, AggregationResult> returnValue = new Dictionary<string, AggregationResult>();
            IEnumerable result = null;
            // check on query: must start with the word "select"
            var startsWithSelect = query
                // TrimStart() removes all whitespace from the beginning of the query,
                // including newline characters
                .TrimStart()
                .StartsWith("select", StringComparison.InvariantCultureIgnoreCase)
                // select is followed by whitespace
                && char.IsWhiteSpace(query.TrimStart(), 6);
            if (!startsWithSelect) {
                throw new ArgumentOutOfRangeException("HQL query not valid: please specify select clause with at least 2 columns (the first for labels, the second for values).");
            } 
            try {
                var session = _transactionManager.GetSession();
                if (isSql) {
                    // SQL Query
                    hql = session.CreateSQLQuery(query);
                } else {
                    // HQL Query
                    hql = session.CreateQuery(query);
                    if (hql.ReturnAliases.Count() < 2) {
                        throw new ArgumentOutOfRangeException("HQL query not valid: please specify select clause with at least 2 columns (the first for labels, the second for values).");
                    }
                }
                hql.SetCacheable(true);
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
                result = hql.SetResultTransformer(Transformers.AliasToEntityMap).List();
            } catch (Exception ex) {
                Log.Error(ex, "RunHqlReport error - query: " + query);
                throw new Exception("RunHqlReport error - query: " + query, ex);
            }

            string[] columnTitles = null;
            if (!string.IsNullOrWhiteSpace(report.ColumnAliases)) {
                // try to use the given aliases as column names
                var aliases = report.ColumnAliases
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToArray();
                if (aliases.Any()) {
                    columnTitles = aliases;
                }
            }
            if (columnTitles == null) {
                if (isSql) {
                    var list = (IEnumerable<object>)result;
                    if (list.Any()) {
                        var tmp = list.First() as Hashtable;
                        columnTitles = new string[tmp.Keys.Count];
                        int i = 0;
                        foreach (var key in tmp.Keys) {
                            columnTitles[i++] = key.ToString();
                        }
                    } else {
                        columnTitles = new string[] { T("No results found for your search.").Text };
                    }
                } else {
                    columnTitles = hql.ReturnAliases;
                }
            }

            if (multiColumnTable) {
                returnValue.Add("0", new AggregationResult {
                    AggregationValue = 0,
                    Label = "",
                    GroupingField = "",
                    Other = columnTitles
                });
                int rownum = 0;
                foreach (var record in result) {
                    var row = new List<object>();
                    var ht = record as Hashtable;
                    foreach(var alias in columnTitles) {
                        if (ht.ContainsKey(alias)) {
                            row.Add(ht[alias]);
                        } else {
                            row.Add(null);
                        }
                    }
                    rownum++;
                    returnValue.Add(rownum.ToString(), new AggregationResult {
                        AggregationValue = 0,
                        Label = "",
                        GroupingField = "",
                        Other = row.ToArray()
                    });
                }
            } else {
                foreach (var record in result) {
                    var ht = record as Hashtable;
                    string key = Convert.ToString(ht[columnTitles[0]]);
                    double value = 0;
                    double.TryParse(Convert.ToString(ht[columnTitles[1]]), out value);
                    if (returnValue.ContainsKey(key)) {
                        var previousItem = returnValue[key];
                        previousItem.AggregationValue += value;
                        returnValue[key] = previousItem;
                    } else {
                        returnValue[key] = new AggregationResult {
                            AggregationValue = value,
                            Label = key,
                            GroupingField = columnTitles[0],
                            Other = null
                        };
                    }
                }
            }
            return returnValue.Values;
        }

        public IHqlQuery ApplyFilter(QueryPartRecord queryRecord,IHqlQuery contentQuery, string category, string type, dynamic state)
        {
            var availableFilters = projectionManager.DescribeFilters().ToList();

            // look for the specific filter component
            var descriptor = availableFilters
                .SelectMany(x => x.Descriptors)
                .FirstOrDefault(x => x.Category == category && x.Type == type);

            // ignore unfound descriptors
            if (descriptor == null)
            {
                return contentQuery;
            }

            var filterContext = new FilterContext
            {
                Query = contentQuery,
                State = state,
                QueryPartRecord = queryRecord
            };

            // apply alteration
            descriptor.Filter(filterContext);

            return filterContext.Query;
        }

        public IEnumerable<IHqlQuery> GetContentQueries(QueryPartRecord queryRecord, IEnumerable<SortCriterionRecord> sortCriteria, IContent container)
        {
            Dictionary<string, object> filtersDictionary = new Dictionary<string, object>();

            if (container != null)
            {
                filtersDictionary.Add("Content", container);
            }
            
            // pre-executing all groups 
            foreach (var group in queryRecord.FilterGroups)
            {
                var contentQuery = this.contentManager.HqlQuery().ForVersion(VersionOptions.Published);

                // iterate over each filter to apply the alterations to the query object
                foreach (var filter in group.Filters)
                {
                    var tokenizedState = _tokenizer.Replace(filter.State, filtersDictionary);
                    dynamic state = FormParametersHelper.ToDynamic(tokenizedState);
                    contentQuery = this.ApplyFilter(queryRecord,contentQuery, filter.Category, filter.Type, state);
                }

                yield return contentQuery;
            }
        }
        public IEnumerable<GenericItem> GetReportListForCurrentUser(string titleFilter = "") {
            string filter = (titleFilter ?? "").ToLowerInvariant();
            var reportLst = new List<GenericItem>();
            var unfilteredList = GetReports().Select(x => new GenericItem {
                Id = x.Id,
                Title =  (x.Has<TitlePart>() ? x.As<TitlePart>().Title : T("[No Title]").ToString()),
                ContentItem = x.ContentItem
            });
            foreach(var report in unfilteredList) {
                if (report.Title.ToLowerInvariant().Contains(filter)) {
                    if (_authorizer.Authorize(GetReportPermissions()[report.Id])) {
                        if (_authorizer.Authorize(Permissions.EditOwnContent, report.ContentItem) == false) {
                            report.ContentItem = null;
                        }
                        reportLst.Add(report);
                    }
                }
            }
            return reportLst.OrderBy(x => x.Title);
        }
        public IEnumerable<GenericItem> GetDashboardListForCurrentUser(string titleFilter = "") {
            string filter = (titleFilter ?? "").ToLowerInvariant();
            var dashboardLst = new List<GenericItem>();
            var unfilteredList = contentManager.Query("DataReportDashboard").List().Select(x => new GenericItem {
                Id = x.Id,
                Title = (x.Has<TitlePart>() ? x.As<TitlePart>().Title : T("[No Title]").ToString()),
                ContentItem = x
            });
            foreach (var dashboard in unfilteredList) {
                if (dashboard.Title.ToLowerInvariant().Contains(filter)) {
                    if (_authorizer.Authorize(GetDashboardPermissions()[dashboard.Id])) {
                        if(_authorizer.Authorize(Permissions.EditOwnContent, dashboard.ContentItem) == false) {
                            dashboard.ContentItem = null;
                        }
                        dashboardLst.Add(dashboard);
                    }
                }
            }
            return dashboardLst.OrderBy(x => x.Title);
        }
        public IEnumerable<DataReportViewerPart> GetReports() {
            // la seguente condizione where è necessaria per ragioni di performance
            return contentManager.Query<DataReportViewerPart>().Where<DataReportViewerPartRecord>(x => true).List();
        }
        public Dictionary<int, Permission> GetReportPermissions() {
            if (_reportPermissions == null) {
                _reportPermissions = new Security.Permissions(contentManager).GetReportPermissions();
            }
            return _reportPermissions;
        }
        public Dictionary<int, Permission> GetDashboardPermissions() {
            if (_dashboardPermissions == null) {
                _dashboardPermissions = new Security.Permissions(contentManager).GetDashboardPermissions();
            }
            return _dashboardPermissions;
        }
        public string GetCsv(DataReportViewerPart part) {
            var report = _reportRepository.Table.FirstOrDefault(c => c.Id == part.Record.Report.Id);
            if (report == null) {
                return null;
            }
            IEnumerable<AggregationResult> reportData = null;
            if (string.IsNullOrWhiteSpace(report.GroupByCategory)) {
                var multiColumn = part.Record.ChartType == ((int)ChartTypes.SimpleTable);
                reportData = RunHqlReport(report, part.ContentItem, multiColumn);
            } else {
                reportData = RunReport(report, part.ContentItem);
            }
            var rows = reportData.ToList();
            var sb = new StringBuilder();
            var text = "";
            if (rows.Count > 0) {
                if (string.IsNullOrWhiteSpace(report.GroupByCategory) && rows[0].Other != null) {
                    // multi column hql report
                    foreach (var row in rows) {
                        foreach (var col in (object[])(row.Other)) {
                            if(col != null) {
                                switch (col.GetType().Name) {
                                    case "DateTime":
                                        sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss}", col);
                                        break;
                                    case "Decimal":
                                    case "Double":
                                    case "Float":
                                        sb.Append(Convert.ToString(col, CultureInfo.InvariantCulture).Replace('.', ','));
                                        break;
                                    default:
                                        text = Convert.ToString(col);
                                        if (text.Contains(';')) {
                                            text = string.Format("\"{0}\"", text);
                                        }
                                        sb.Append(text);
                                        break;
                                }
                            }
                            sb.Append(";"); // terminatore di campo
                        }
                        sb.Append("\r\n"); // terminatore di riga
                    }
                } else {
                    // standard report
                    foreach (var row in rows) {
                        text = Convert.ToString(row.Label);
                        if (text.Contains(';')) {
                            text = string.Format("\"{0}\"", text);
                        }
                        sb.AppendFormat("{0};{1}\r\n", text, row.AggregationValue);
                    }
                }
            }
            return sb.ToString();
        }
    }
}