using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections;
using Orchard.Projections.Models;
using Laser.Orchard.Reporting.Models;
using Laser.Orchard.Reporting.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using Orchard.ContentManagement;
using System.Web.Mvc;
using Orchard.Core.Title.Models;
using Laser.Orchard.Reporting.Providers;
using Laser.Orchard.Reporting.Services;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.Settings;
using Orchard.DisplayManagement;
using Orchard;
using Orchard.Themes;
using Orchard.ContentPicker.Fields;
using Orchard.Security;
using System.Text;
using Laser.Orchard.Commons.Services;
using System.Collections.Generic;
using occ = Orchard.Core.Contents;

namespace Laser.Orchard.Reporting.Controllers {
    [ValidateInput(false), Admin]
    public class ReportController : Controller
    {
        private readonly IRepository<QueryPartRecord> queryRepository;
        private readonly IRepository<ReportRecord> reportRepository;
        private readonly IReportManager reportManager;
        private readonly IAuthorizer _authorizer;
        private IOrchardServices services { get; set; }
        private readonly ISiteService siteService;

        public ReportController(
            ISiteService siteService,
            IReportManager reportManager,
            IShapeFactory shapeFactory,
            IOrchardServices services,
            IAuthorizer authorizer,
            IRepository<QueryPartRecord> queryRepository,
            IRepository<ReportRecord> reportRepository)
        {
            this.siteService = siteService;
            this.reportManager = reportManager;
            this.reportRepository = reportRepository;
            this.queryRepository = queryRepository;
            this.services = services;
            this.Shape = shapeFactory;
            _authorizer = authorizer;
        }

        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        public ActionResult Index(PagerParameters pagerParameters)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            var siteSettings = this.siteService.GetSiteSettings();
            pagerParameters.PageSize = pagerParameters.PageSize ?? siteSettings.PageSize;
            pagerParameters.Page = pagerParameters.Page ?? 1;
            var pager = new Pager(siteSettings, pagerParameters);
            List<ReportRecord> reports = null;
            if (pager.PageSize == 0) { // visualizza tutti gli elementi
                reports = this.reportRepository.Table.OrderByDescending(c => c.Id).ToList();
            }
            else {
                reports = this.reportRepository.Table.OrderByDescending(c => c.Id).Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();
            }
            var model = new ReportListViewModel();
            model.Pager = Shape.Pager(pager).TotalItemCount(this.reportRepository.Table.Count());
            model.Reports.AddRange(reports.Select(c => new ReportViewModel
            {
                ReportId = c.Id,
                Name = c.Title,
                CategoryAndType = c.GroupByCategory
            }));
            return this.View(model);
        }

        public ActionResult Create()
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            var model = new ReportViewModel();

            this.FillRelatedData(model);

            return this.View(model);
        }

        public ActionResult CreateHql() {
            if (!services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            var model = new HqlReportViewModel();

            FillRelatedData(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult CreatePost(ReportViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            if (!this.ModelState.IsValid)
            {
                model = model ?? new ReportViewModel();
                this.FillRelatedData(model);
                return this.View("Create", model);
            }

            var groupByDescriptor = this.DecodeGroupByCategoryAndType(model.CategoryAndType);

            if (groupByDescriptor == null)
            {
                this.ModelState.AddModelError("CategoryAndType", T("There is no GroupBy field matched with the given parameters").Text);
                this.FillRelatedData(model);
                return this.View("Create", model);
            }

            AggregateMethods selectedAggregate = (AggregateMethods)model.AggregateMethod;
            if (!groupByDescriptor.AggregateMethods.Any(c => c == selectedAggregate))
            {
                this.ModelState.AddModelError("AggregateMethod", T("The selected field does't support the selected Aggregate method").Text);
                this.FillRelatedData(model);
                return this.View("Create", model);
            }

            ReportRecord newReport = new ReportRecord
            {
                Title = model.Title,
                Name = model.Name,
                Query = new QueryPartRecord { Id = model.QueryId.Value },
                GroupByCategory = groupByDescriptor.Category,
                GroupByType = groupByDescriptor.Type,
                AggregateMethod = model.AggregateMethod,
                GUID = Guid.NewGuid().ToString()
            };

            this.reportRepository.Create(newReport);
            this.reportRepository.Flush();

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CreateHqlPost(HqlReportViewModel model) {
            if (!services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) {
                model = model ?? new HqlReportViewModel();
                FillRelatedData(model);
                return View("CreateHql", model);
            }

            ReportRecord newReport = new ReportRecord {
                Title = model.Title,
                Name = model.Name,
                Query = new QueryPartRecord { Id = model.QueryId.Value },
                GroupByCategory = "",
                GroupByType = "",
                AggregateMethod = 0,
                ColumnAliases = model.ColumnAliases,
                GUID = Guid.NewGuid().ToString()
            };

            reportRepository.Create(newReport);
            reportRepository.Flush();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Remove(int reportId)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            ReportRecord report = this.reportRepository.Get(reportId);

            if (report == null)
            {
                if (!this.ModelState.IsValid)
                {
                    this.ModelState.AddModelError("ReportId", T("There is no report with the given Id").Text);
                    var reports = this.reportRepository.Table.ToList();
                    var model = reports.Select(c => new ReportViewModel
                    {
                        ReportId = c.Id,
                        Name = c.Title
                    }).ToList();

                    return this.View(model);
                }
            }

            this.reportRepository.Delete(report);
            this.reportRepository.Flush();
            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult EditPost(ReportViewModel model)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            if (!this.ModelState.IsValid)
            {
                model = model ?? new ReportViewModel();
                this.FillRelatedData(model);
                return this.View("Edit", model);
            }

            ReportRecord report = this.reportRepository.Get(model.ReportId);

            if (report == null)
            {
                if (!this.ModelState.IsValid)
                {
                    this.ModelState.AddModelError("ReportId", T("There is no report with the given Id").Text);
                    this.FillRelatedData(model);
                    return this.View("Edit", model);
                }
            }

            var groupByDescriptor = this.DecodeGroupByCategoryAndType(model.CategoryAndType);

            if (groupByDescriptor == null)
            {
                this.ModelState.AddModelError("CategoryAndType", T("There is no GroupBy field matched with the given parameters").Text);
                this.FillRelatedData(model);
                return this.View("Edit", model);
            }

            AggregateMethods selectedAggregate = (AggregateMethods)model.AggregateMethod;
            if (!groupByDescriptor.AggregateMethods.Any(c => c == selectedAggregate))
            {
                this.ModelState.AddModelError("AggregateMethod", T("The selected field does't support the selected Aggregate method").Text);
                this.FillRelatedData(model);
                return this.View("Edit", model);
            }

            report.Title = model.Title;
            report.Name = model.Name;
            report.Query = model.QueryId.HasValue ? new QueryPartRecord { Id = model.QueryId.Value } : null;
            report.GroupByCategory = groupByDescriptor.Category;
            report.GroupByType = groupByDescriptor.Type;
            report.AggregateMethod = model.AggregateMethod;
            if (string.IsNullOrWhiteSpace(report.GUID)) {
                report.GUID = Guid.NewGuid().ToString();
            }

            this.reportRepository.Update(report);
            this.reportRepository.Flush();

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult EditHqlPost(HqlReportViewModel model) {
            if (!services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) {
                model = model ?? new HqlReportViewModel();
                FillRelatedData(model);
                return View("EditHql", model);
            }

            ReportRecord report = reportRepository.Get(model.ReportId);

            if (report == null) {
                if (!ModelState.IsValid) {
                    ModelState.AddModelError("ReportId", T("There is no report with the given Id").Text);
                    FillRelatedData(model);
                    return View("EditHql", model);
                }
            }

            report.Title = model.Title;
            report.Name = model.Name;
            report.Query = model.QueryId.HasValue ? new QueryPartRecord { Id = model.QueryId.Value } : null;
            report.GroupByCategory = "";
            report.GroupByType = "";
            report.AggregateMethod = 0;
            report.ColumnAliases = model.ColumnAliases;
            if (string.IsNullOrWhiteSpace(report.GUID)) {
                report.GUID = Guid.NewGuid().ToString();
            }

            reportRepository.Update(report);
            reportRepository.Flush();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (!this.services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            var report = this.reportRepository.Table.FirstOrDefault(c => c.Id == id);

            if (report == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "{0}={1}", T("There is no report with the Id"), id.ToString(CultureInfo.InvariantCulture)));
            }

            var model = new ReportViewModel
            {
                ReportId = report.Id,
                CategoryAndType = this.EncodeGroupByCategoryAndGroupByType(report.GroupByCategory, report.GroupByType),
                Title = report.Title,
                Name = report.Name,
                AggregateMethod = report.AggregateMethod,
                QueryId = report.Query != null ? (int?)report.Query.Id : null
            };

            this.FillRelatedData(model);

            return this.View(model);
        }

        public ActionResult EditHql(int id) {
            if (!services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list Reports")))
                return new HttpUnauthorizedResult();

            var report = reportRepository.Table.FirstOrDefault(c => c.Id == id);

            if (report == null) {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "{0}={1}", T("There is no report with the Id"), id.ToString(CultureInfo.InvariantCulture)));
            }

            var model = new HqlReportViewModel {
                ReportId = report.Id,
                Title = report.Title,
                Name = report.Name,
                ColumnAliases = report.ColumnAliases,
                QueryId = report.Query != null ? (int?)report.Query.Id : null
            };

            FillRelatedData(model);

            return View(model);
        }

        public ActionResult Display(ReportDisplayViewModel model) {
            if (_authorizer.Authorize(reportManager.GetReportPermissions()[model.Id]) == false) { 
                return new HttpUnauthorizedResult(T("Not authorized to list Reports").ToString());
            }
            var ci = services.ContentManager.Get(model.Id);
            if(ci.Has<DataReportViewerPart>() == false) {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "{0}={1}", T("There is no report with the Id"), model.Id.ToString(CultureInfo.InvariantCulture)));
            }
            var filterPart = ci.Parts.FirstOrDefault(x => x.PartDefinition.Name == ci.ContentType);
            if(filterPart != null) {
                model.FilterContent = services.ContentManager.New("DataReportEmptyType");
                model.FilterContent.Weld(filterPart);
            }
            else {
                model.ViewerContent = services.ContentManager.New("DataReportEmptyType");
                model.ViewerContent.Weld(ci.As<DataReportViewerPart>());
            }
            return View(model);
        }
        [Themed(Enabled = false)]
        public ActionResult DisplayChart(int id) {
            if (_authorizer.Authorize(reportManager.GetReportPermissions()[id]) == false) {
                return new HttpUnauthorizedResult(T("Not authorized to list Reports").ToString());
            }
            var ci = services.ContentManager.Get(id);
            if (ci.Has<DataReportViewerPart>() == false) {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "{0}={1}", T("There is no report with the Id"), id));
            }
            var viewerPart = ci.As<DataReportViewerPart>();
            var model = services.ContentManager.New("DataReportEmptyType");
            model.Weld(viewerPart);
            return View(model);
        }
        [Themed(Enabled = false)]
        public ActionResult DownloadChart(int id) {
            if (_authorizer.Authorize(reportManager.GetReportPermissions()[id]) == false || _authorizer.Authorize(Security.Permissions.DownloadReportData) == false) {
                return new HttpUnauthorizedResult(T("Not authorized").ToString());
            }
            var ci = services.ContentManager.Get(id);
            if (ci.Has<DataReportViewerPart>() == false) {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "{0}={1}", T("There is no report with the Id"), id));
            }
            var viewerPart = ci.As<DataReportViewerPart>();
            var q = reportManager.GetCsv(viewerPart);
            return new FileContentResult(Encoding.UTF8.GetBytes(q), "application/csv") {
                FileDownloadName = new CommonUtils().NormalizeFileName(viewerPart.Record.Report.Title, "chart") + ".csv"
            };
        }
        [Themed(Enabled = false)]
        public ActionResult DownloadDashboard(int id) {
            if (_authorizer.Authorize(reportManager.GetDashboardPermissions()[id]) == false || _authorizer.Authorize(Security.Permissions.DownloadDashboardData) == false) {
                return new HttpUnauthorizedResult(T("Not authorized").ToString());
            }
            // recupera l'elenco dei report e lo aggiunge al model
            var ciDashboard = services.ContentManager.Get(id);
            var dashboard = ciDashboard.Parts.FirstOrDefault(x => x.PartDefinition.Name == "DataReportDashboardPart");
            if (dashboard == null) {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "{0}={1}", T("There is no dashboard with the Id"), id));
            }
            var cpf = dashboard.Fields.FirstOrDefault(x => x.Name == "ReportIds") as ContentPickerField;
            var reports = services.ContentManager.GetMany<ContentItem>(cpf.Ids, VersionOptions.Published, QueryHints.Empty);
            var csvContents = new List<string>();
            var titles = new List<string>();
            var commonUtils = new CommonUtils();
            foreach (var rep in reports) {
                var viewerPart = rep.As<DataReportViewerPart>();
                var csv = reportManager.GetCsv(viewerPart);
                csvContents.Add(csv);
                titles.Add(viewerPart.Record.Report.Title);
            }
            var zip = commonUtils.CreateZipArchive(csvContents, titles, "csv");
            var dashboardTitle = "dashboard";
            var titlePart = ciDashboard.As<TitlePart>();
            if(titlePart != null) {
                dashboardTitle = commonUtils.NormalizeFileName(titlePart.Title, dashboardTitle);
            }
            return new FileContentResult(zip, "application/zip") {
                FileDownloadName = dashboardTitle + ".zip"
            };
        }
        public ActionResult ShowReports(ShowReportsViewModel model) {
            var siteSettings = this.siteService.GetSiteSettings();
            var pagerParameters = new PagerParameters();
            pagerParameters.PageSize = model.pageSize ?? siteSettings.PageSize;
            pagerParameters.Page = model.page ?? 1;
            var list = reportManager.GetReportListForCurrentUser(model.TitleFilter);
            var pager = new Pager(siteSettings, pagerParameters);
            model.Pager = Shape.Pager(pager).TotalItemCount(list.Count());
            if (pager.PageSize == 0) { // visualizza tutti gli elementi
                model.Reports = list;
            }
            else {
                model.Reports = list.Skip(pager.GetStartIndex()).Take(pager.PageSize);
            }
            var ctList = services.ContentManager.GetContentTypeDefinitions().Where(t => t.Parts.Any(p => p.PartDefinition.Name == "DataReportViewerPart"));
            foreach(var ct in ctList) {
                model.ContentTypes.Add(ct);
            }
            model.BaseUrlForCreate = GetBaseUrlForCreate();
            return View(model);
        }
        public ActionResult ShowDashboard(ShowDashboardViewModel model) {
            if (_authorizer.Authorize(reportManager.GetDashboardPermissions()[model.Id]) == false) {
                return new HttpUnauthorizedResult(T("Not authorized to execute this dashboard").ToString());
            }
            // recupera l'elenco dei report e lo aggiunge al model
            var ciDashboard = services.ContentManager.Get(model.Id);
            var dashboard = ciDashboard.Parts.FirstOrDefault(x => x.PartDefinition.Name == "DataReportDashboardPart");
            if(dashboard == null) {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "{0}={1}", T("There is no dashboard with the Id"), model.Id));
            }
            model.Title = ciDashboard.As<TitlePart>().Title;
            var cpf = dashboard.Fields.FirstOrDefault(x => x.Name == "ReportIds") as ContentPickerField;
            var reports = services.ContentManager.GetMany<ContentItem>(cpf.Ids, VersionOptions.Published, QueryHints.Empty);
            foreach (var rep in reports) {
                // grafico
                var ci = services.ContentManager.New("DataReportEmptyType");
                ci.Weld(rep.As<DataReportViewerPart>());
                model.Reports.Add(ci);
                // filtri
                var filterPart = rep.Parts.FirstOrDefault(x => x.PartDefinition.Name == rep.ContentType);
                if(filterPart != null) {
                    if(model.Filters == null) {
                        model.Filters = services.ContentManager.New("DataReportEmptyType");
                    }
                    // controlla che questa specifica part di filtro non sia già stata aggiunta
                    var checkPart = model.Filters.Parts.FirstOrDefault(x => x.PartDefinition.Name == filterPart.PartDefinition.Name);
                    if(checkPart == null) {
                        model.Filters.Weld(filterPart);
                    }
                }
            }
            return View(model);
        }
        public ActionResult DashboardList(DashboardListViewModel model) {
            var siteSettings = this.siteService.GetSiteSettings();
            var pagerParameters = new PagerParameters();
            pagerParameters.PageSize = model.pageSize ?? siteSettings.PageSize;
            pagerParameters.Page = model.page ?? 1;
            var list = reportManager.GetDashboardListForCurrentUser(model.TitleFilter);
            var pager = new Pager(siteSettings, pagerParameters);
            model.Pager = Shape.Pager(pager).TotalItemCount(list.Count());
            if (pager.PageSize == 0) { // visualizza tutti gli elementi
                model.Dashboards = list;
            }
            else {
                model.Dashboards = list.Skip(pager.GetStartIndex()).Take(pager.PageSize);
            }
            var dummyContent = services.ContentManager.New("DataReportDashboard");
            if (_authorizer.Authorize(occ.Permissions.CreateContent, dummyContent)) {
                model.UrlForCreateDashboard = GetBaseUrlForCreate() + "DataReportDashboard";
            }
            return View(model);
        }
        private string EncodeGroupByCategoryAndGroupByType(string category, string type)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}__{1}", category, type);
        }

        private GroupByDescriptor DecodeGroupByCategoryAndType(string categoryAndType)
        {
            var descriptors = this.reportManager.DescribeGroupByFields();
            foreach (var item in descriptors.SelectMany(c => c.Descriptors))
            {
                if (this.EncodeGroupByCategoryAndGroupByType(item.Category, item.Type) == categoryAndType)
                {
                    return item;
                }
            }

            return null;
        }

        private void FillRelatedData(ReportViewModel model)
        {
            var queries = this.services.ContentManager.Query().ForType("Query").List();

            // Fill queries
            foreach (var query in queries)
            {
                var title = query.As<TitlePart>();
                model.Queries.Add(new SelectListItem
                {
                    Text = title != null ? title.Title : T("[No Name]").Text,
                    Value = query.Id.ToString()
                });
            }

            // Fill Aggregations
            model.Aggregations.Add(new SelectListItem { Text = T("Count").Text, Value = ((byte)AggregateMethods.Count).ToString(CultureInfo.InvariantCulture) });
            model.Aggregations.Add(new SelectListItem { Text = T("Sum").Text, Value = ((byte)AggregateMethods.Sum).ToString(CultureInfo.InvariantCulture) });
            model.Aggregations.Add(new SelectListItem { Text = T("Average").Text, Value = ((byte)AggregateMethods.Average).ToString(CultureInfo.InvariantCulture) });
            model.Aggregations.Add(new SelectListItem { Text = T("Minimum").Text, Value = ((byte)AggregateMethods.Minimum).ToString(CultureInfo.InvariantCulture) });
            model.Aggregations.Add(new SelectListItem { Text = T("Maximum").Text, Value = ((byte)AggregateMethods.Maximum).ToString(CultureInfo.InvariantCulture) });

            // descriptors
            var typeDescriptors = this.reportManager.DescribeGroupByFields();
            foreach (var typeDescriptor in typeDescriptors)
            {
                ReportGroupByFieldCollectionViewModel groupByCollection = new ReportGroupByFieldCollectionViewModel
                {
                    Name = typeDescriptor.Name,
                    Description = typeDescriptor.Description
                };

                model.GroupByFieldsCollection.Add(groupByCollection);

                foreach (var descriptor in typeDescriptor.Descriptors)
                {
                    groupByCollection.GroupByFields.Add(new ReportGroupByFieldViewModel
                    {
                        CategoryAndType = this.EncodeGroupByCategoryAndGroupByType(descriptor.Category, descriptor.Type),
                        Description = descriptor.Description,
                        Name = descriptor.Name
                    });
                }
            }
        }
        private void FillRelatedData(HqlReportViewModel model) {
            var queries = services.ContentManager.Query().ForType("MyCustomQuery").List();

            // Fill queries
            foreach (var query in queries) {
                var title = query.As<TitlePart>();
                model.Queries.Add(new SelectListItem {
                    Text = title != null ? title.Title : T("[No Name]").Text,
                    Value = query.Id.ToString()
                });
            }
        }
        private string GetBaseUrlForCreate() {
            var dummyContent = services.ContentManager.New("DataReportDashboard");
            ContentItemMetadata metadata = services.ContentManager.GetItemMetadata(dummyContent);
            return Url.RouteUrl(metadata.CreateRouteValues).Replace("DataReportDashboard", "");
        }
    }
}