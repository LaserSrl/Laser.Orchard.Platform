using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Laser.Orchard.ContentExtension.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Projections.Services;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

[OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
public class DynamicProjectionDisplayController : Controller {
    private readonly IOrchardServices _orchardServices;
    private readonly IProjectionManagerExtension _projectionManager;
    private readonly IContentManager _contentManager;
    private readonly ShellSettings _shellSettings;
    public Localizer T { get; set; }
    public ILogger Logger { get; set; }
    dynamic _shapeFactory { get; set; }

    public DynamicProjectionDisplayController(
        IOrchardServices orchardServices,
        IProjectionManagerExtension projectionManager,
        IContentManager contentManager,
        IShapeFactory shapeFactory,
        ShellSettings shellSettings
        ) {
        _orchardServices = orchardServices;
        _projectionManager = projectionManager;
        _contentManager = contentManager;
        T = NullLocalizer.Instance;
        Logger = NullLogger.Instance;
        _shapeFactory = shapeFactory;
        _shellSettings = shellSettings;
    }


    public ActionResult AjaxList(int contentid, PagerParameters pagerParameters) {
        Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
        var ci = _orchardServices.ContentManager.Get(contentid);
        if (ci == null || ci.As<DynamicProjectionPart>() == null) {
            return null;
        } else {
            var record = ci.As<DynamicProjectionPart>().Record;
            var queryString = _orchardServices.WorkContext.HttpContext.Request.QueryString;
            pager.PageSize = ci.As<DynamicProjectionPart>().Record.Items;
            var pageSizeKey = "pageSize" + record.PagerSuffix;
            if (queryString.AllKeys.Contains(pageSizeKey)) {
                int qsPageSize;

                if (int.TryParse(queryString[pageSizeKey], out qsPageSize)) {
                    if (record.MaxItems == 0 || qsPageSize <= record.MaxItems) {
                        pager.PageSize = qsPageSize;
                    }
                }
            }

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

    [Admin]
    public ActionResult List(int contentid) {
        var ci = _orchardServices.ContentManager.Get(contentid);
        if (ci == null || ci.As<DynamicProjectionPart>() == null) {
            return null;
        } else {
            var newci = _contentManager.New("FakeContentForDynamicProjection");
            var formfile = string.Empty;
            if (!string.IsNullOrEmpty(ci.As<DynamicProjectionPart>().Shape)) {
                formfile = string.Format(
                    "~/App_Data/Sites/{0}/Code/{1}",
                    _shellSettings.Name,
                    ci.As<DynamicProjectionPart>().Shape);
            }
            var contpart = ci.Parts
                .Where(x => ((dynamic)x).PartDefinition.Name == ci.ContentType)
                .FirstOrDefault();
            IEnumerable<ContentField> lcf = null;
            if (contpart != null) {
                lcf = contpart.Fields;
                foreach (var field in lcf)
                    newci.Parts.FirstOrDefault().Weld(field);
            } else {
                newci = null;
            }
            var viewModel = _shapeFactory.ViewModel()
                .Part(ci.As<DynamicProjectionPart>())
                .Form(formfile)
                .FilterContent(newci);
            return View(viewModel);
        }
    }
}