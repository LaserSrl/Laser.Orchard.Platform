using Contrib.Widgets.Models;
using Orchard;
using Orchard.Mvc.Filters;
using Orchard.Taxonomies.Controllers;
using System.Net;
using System.Web.Mvc;
using Orchard.Mvc;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.ViewModels;
using Orchard.ContentManagement.Handlers;
using System;
using System.Web;

namespace Contrib.Widgets.Filters {
    public class WidgetFilter : FilterProvider, IActionFilter, IContentHandler {

        private readonly IWorkContextAccessor _workContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private string zone = string.Empty;
        private int hostId = 0;

        public WidgetFilter(
            IWorkContextAccessor workContext,
            IHttpContextAccessor httpContextAccessor) {

            _workContext = workContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            if (string.IsNullOrWhiteSpace(filterContext.HttpContext.Request.Form["ContribWidget.ClickedZone"]) ||
                string.IsNullOrWhiteSpace(filterContext.HttpContext.Request.Form["ContribWidget.HostIdSaved"])) {
                return;
            }

            zone = filterContext.HttpContext.Request.Form["ContribWidget.ClickedZone"];
            hostId = Convert.ToInt32(filterContext.HttpContext.Request.Form["ContribWidget.HostIdSaved"]);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            if (string.IsNullOrWhiteSpace(zone) || hostId == 0) {
                return;
            }

            if (!((dynamic)filterContext.Controller).ModelState.IsValid) {
                return;
            }

            
            UrlHelper urlHelper = new UrlHelper(_httpContextAccessor.Current().Request.RequestContext);
            filterContext.Result = new RedirectResult(
                urlHelper.Action("ListWidgets",
                    "Admin",
                    new { area = "Contrib.Widgets", hostId = hostId, zone = zone }));
            
        }

        public void Activating(ActivatingContentContext context) { }

        public void Activated(ActivatedContentContext context) { }

        public void Initializing(InitializingContentContext context) { }

        public void Initialized(InitializingContentContext context) { }

        public void Creating(CreateContentContext context) { }

        public void Created(CreateContentContext context) {
            if (!string.IsNullOrWhiteSpace(zone) && hostId != context.ContentItem.Id) {
                hostId = context.ContentItem.Id;
            }
        }

        public void Loading(LoadContentContext context) { }

        public void Loaded(LoadContentContext context) { }

        public void Updating(UpdateContentContext context) { }

        public void Updated(UpdateContentContext context) { }

        public void Versioning(VersionContentContext context) { }

        public void Versioned(VersionContentContext context) { }

        public void Publishing(PublishContentContext context) { }

        public void Published(PublishContentContext context) { }

        public void Unpublishing(PublishContentContext context) { }

        public void Unpublished(PublishContentContext context) { }

        public void Removing(RemoveContentContext context) { }

        public void Removed(RemoveContentContext context) { }

        public void Indexing(IndexContentContext context) { }

        public void Indexed(IndexContentContext context) { }

        public void Importing(ImportContentContext context) { }

        public void Imported(ImportContentContext context) { }

        public void Cloning(CloneContentContext context) { }

        public void Cloned(CloneContentContext context) { }

        public void ImportCompleted(ImportContentContext importContentContext) { }

        public void Exporting(ExportContentContext context) { }

        public void Exported(ExportContentContext context) { }

        public void GetContentItemMetadata(GetContentItemMetadataContext context) { }

        public void BuildDisplay(BuildDisplayContext context) { }

        public void BuildEditor(BuildEditorContext context) { }

        public void UpdateEditor(UpdateEditorContext context) { }

        public void Restoring(RestoreContentContext context) { }

        public void Restored(RestoreContentContext context) { }

        public void Destroying(DestroyContentContext context) { }

        public void Destroyed(DestroyContentContext context) { }
    }
}