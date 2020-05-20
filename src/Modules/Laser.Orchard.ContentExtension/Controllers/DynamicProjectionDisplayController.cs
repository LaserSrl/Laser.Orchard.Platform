using Laser.Orchard.ContentExtension.Models;
using Laser.Orchard.ContentExtension.Services;
using Laser.Orchard.Queries.Models;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Projections.Services;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace Laser.Orchard.ContentExtension.Controllers {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionDisplayController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManagerExtension _projectionManager;
        private readonly IContentManager _contentManager;
        private readonly IDynamicProjectionService _dynamicProjectoinService;
        private readonly IQueryPickerService _queryPickerService;
        private readonly IFrontEndEditService _frontEndEditService;
        private readonly ShellSettings _shellSettings;
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic _shapeFactory { get; set; }

        public DynamicProjectionDisplayController(
            IOrchardServices orchardServices,
            IProjectionManagerExtension projectionManager,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IDynamicProjectionService dynamicProjectoinService,
            IQueryPickerService queryPickerService,
            IFrontEndEditService frontEndEditService,
            ShellSettings shellSettings
            ) {
            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _shapeFactory = shapeFactory;
            _dynamicProjectoinService = dynamicProjectoinService;
            _queryPickerService = queryPickerService;
            _frontEndEditService = frontEndEditService;
            _shellSettings = shellSettings;
        }

        [Admin]
        [Themed(false)]
        public ActionResult AjaxList(int contentid, PagerParameters pagerParameters) {
            //TODO: protection level is too basic, should work based on permissions for the DynamicProjection
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var ci = _orchardServices.ContentManager.Get(contentid);
            DynamicProjectionPart part = ci.As<DynamicProjectionPart>();
            if (ci == null || part == null) {
                return null;
            }
            else {
                var record = part.Record;
                var queryString = _orchardServices.WorkContext.HttpContext.Request.QueryString;
                pager.PageSize = part.Record.Items;
                var pageSizeKey = "pageSize" + record.PagerSuffix;
                if (queryString.AllKeys.Contains(pageSizeKey)) {
                    int qsPageSize;

                    if (int.TryParse(queryString[pageSizeKey], out qsPageSize)) {
                        if (record.MaxItems == 0 || qsPageSize <= record.MaxItems) {
                            pager.PageSize = qsPageSize;
                        }
                    }
                }
                //TODO: Should works also for queries that are pure Hql and not only for ContentManager queries
                //Options:
                // 1.   Use QueryPickerPart that has the ability to configure HqlQueries 
                //      However QueryPickerPart and HqlQueries need some improvements:
                //          a. Use of parameters instead of string substitution
                //          b. QueryPickerPart is not attachable
                //          c. QueryPickerPart does not support pagination by now
                //          d. QueryPickerPart does not support pure Hql queries because under the hood it uses a ContentQuery
                //          e. We should have a way to call a razor view in order to show results of the query
                // 2.   Use DynamicProjectionPart 
                //          a. Firing only the Hql filter if exists otherwise it return null
                //          b. this approach is the most easy but also the worst because we have queries that will throw exceptions if used by projectionpart or the ProjectionManager
                //          c. using this approach we could use the laout of the query to customize the result view 
                // 3.   Use a text tokenized as query 
                //          a. Use of parameters instead of string substitution
                //          b. We should have a way to call a razor view in order to show results of the query

                if (part.ReturnsHqlResults) {
                    var queryPicker = ci.As<QueryPickerPart>();
                    if (queryPicker == null || !queryPicker.Ids.Any()) {
                        return null;
                    }

                    //Results and Count
                    var countQuery = _queryPickerService.GetCustomQuery(queryPicker.Ids.First(), new Dictionary<string, object> { { "Content", ci } }, true);
                    var query = _queryPickerService.GetCustomQuery(queryPicker.Ids.First(), new Dictionary<string, object> { { "Content", ci } });
                    var queryresults = _dynamicProjectoinService.GetResults(query,
                                                                        pager.GetStartIndex() + record.Skip,
                                                                        pager.PageSize);
                    var counttot = _dynamicProjectoinService.GetCount(countQuery);

                    //Pager
                    var pagerShape = _shapeFactory
                        .Pager(pager).TotalItemCount(counttot);

                    //Shape for results
                    var resultsfile = string.Empty;
                    if (!string.IsNullOrEmpty(part.ShapeForResults)) {
                        resultsfile = string.Format(
                            "~/App_Data/Sites/{0}/Code/{1}",
                            _shellSettings.Name,
                            ci.As<DynamicProjectionPart>().ShapeForResults);
                    }


                    var viewModel = _shapeFactory.ViewModel()
                        .ResultShape(resultsfile)
                        .Items(queryresults)
                        .Pager(pagerShape)
                        .Part(ci.As<DynamicProjectionPart>());
                    return View(viewModel);

                }
                else {
                    var contentItems = _projectionManager
                        .GetContentItems(
                            record.QueryPartRecord.Id,
                            ci.As<DynamicProjectionPart>(),
                            pager.GetStartIndex() + record.Skip,
                            pager.PageSize)
                        .ToList();
                    var counttot = _projectionManager
                        .GetCount(record.QueryPartRecord.Id, ci.As<DynamicProjectionPart>());
                    IEnumerable<ContentItem> pageOfContentItems = null;
                    var pagerShape = _shapeFactory
                        .Pager(pager).TotalItemCount(counttot);
                    var list = _shapeFactory.List();
                    pageOfContentItems = contentItems;
                    if (pageOfContentItems != null) {
                        list.AddRange(pageOfContentItems
                            .Select(contentitem => _contentManager.BuildDisplay(contentitem, "SummaryAdmin")));
                    }
                    var viewModel = _shapeFactory.ViewModel()
                        .ContentItems(list)
                        .Pager(pagerShape)
                        .Part(ci.As<DynamicProjectionPart>());
                    return View(viewModel);
                }
            }
        }

        [Admin]
        public ActionResult List(int contentid) {
            var ci = _orchardServices.ContentManager.Get(contentid);
            var part = ci.As<DynamicProjectionPart>();
            if (ci == null || part == null) {
                return null;
            }
            else {
                Func<ContentItem, IShape> shape;
                ContentItem contentFormTemplate;
                if (string.IsNullOrWhiteSpace(part.TypeForFilterForm)) {
                    contentFormTemplate= _contentManager.New("FakeContentForDynamicProjection");
                    shape = (content) => {
                        var contpart = ci.Parts
                            .Where(x => x.PartDefinition.Name == ci.ContentType)
                            .FirstOrDefault();
                        IEnumerable<ContentField> lcf = null;
                        if (contpart != null) {
                            lcf = contpart.Fields;
                            foreach (var field in lcf)
                                content.Parts.FirstOrDefault().Weld(field);
                        }
                        return _contentManager.BuildEditor(content);
                    };
                }
                else {
                    contentFormTemplate = _contentManager.New(part.TypeForFilterForm);
                    shape = (content) => {
                        IShape contenFormEditShape = _contentManager.BuildEditor(content);
                        return _frontEndEditService.BuildFrontEndShape(contenFormEditShape, (ctpd, typeName) => {
                            var uselessParts = new string[] { "CommonPart" };
                            return !uselessParts.Contains(ctpd.PartDefinition.Name); //Excludes uselessParts
                        },
                            //All fields
                            (ctpd) => true);
                    };
                }
                var formfile = string.Empty;
                if (!string.IsNullOrEmpty(part.Shape)) {
                    formfile = string.Format(
                        "~/App_Data/Sites/{0}/Code/{1}",
                        _shellSettings.Name,
                        ci.As<DynamicProjectionPart>().Shape);
                }

                //TODO: the way to create a filter form content should be more flexible and should support more complex scenarios.
                var viewModel = _shapeFactory.ViewModel()
                    .Part(ci.As<DynamicProjectionPart>())
                    .Form(formfile)
                    .FilterContent(contentFormTemplate)
                    .CalculateFilterContentShape(shape);
                return View(viewModel);
            }

        }
    }
}
