using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Controllers;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.Widgets.Models;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using OCore = Orchard.Core.Contents;

namespace Laser.Orchard.TenantBridges.Controllers {
    [Themed]
    public class ItemController : ContentControllerBase {
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _hca;
        private readonly IAuthorizer _authorizer;
        private readonly IOrchardServices _orchardServices;

        public ItemController(
            IContentManager contentManager,
            IHttpContextAccessor hca,
            IAuthorizer authorizer,
            IOrchardServices orchardServices)
            : base(contentManager) {

            _contentManager = contentManager;
            _hca = hca;
            _authorizer = authorizer;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Display(
            int? id,
            string zone,
            bool? wrappers) {
            // Normally, we should be checking 
            // _hca.Current().Request.IsAjaxRequest()
            // to test whether this is a request specifically for only the html of the content.
            // However, that kind of output is the whole point for this Action, so we don't
            // check that, and assume that is the intent of the caller.

            // Start processing
            if (id == null) {
                // this is just a 404 "partial"
                return PartialNotFoundResult();
            }

            var contentItem = _contentManager.Get(id.Value, VersionOptions.Published);

            // TODO: The default Display action for content items here would check whether the
            // specific item should be displayed by its own route.
            // var customRouteRedirection = GetCustomContentItemRouteRedirection(contentItem, ContentItemRoute.Display);
            // Here we don't have a clear way to do the same thing.

            if (contentItem == null) {
                // this is just a 404 "partial"
                return PartialNotFoundResult();
            }
            
            // This logic on containers is the same as the one from the defail ItemController for 
            // content items from Orchard.Core.
            var container = contentItem.As<CommonPart>()?.Container;
            if (container != null && !container.HasPublished()) {
                // if the content has a container that has not a published version we check preview permissions
                // in order to check if user can view the content or not.
                // Open point: should we handle hierarchies? 
                if (!_authorizer.Authorize(OCore.Permissions.PreviewContent, contentItem)) {
                    // this is just a 401 "partial"
                    return PartialUnauthorizedResult();
                }
            }

            if (!_authorizer.Authorize(OCore.Permissions.ViewContent, contentItem, T("Cannot view content"))) {
                // this is just a 401 "partial"
                return PartialUnauthorizedResult();
            }

            var clearWrappers = !(wrappers.HasValue && wrappers.Value);
            var isWidget = contentItem.Is<WidgetPart>();

            // TODO: figure out a way to actually use the zone parameter. As thigs are now
            // widgets are able to get the correct zone alternates, while ContentItems are
            // all rendered as if from the "Content" Zone.
            if (isWidget) {
                zone = contentItem.As<WidgetPart>().Zone;
            }
            if (string.IsNullOrEmpty(zone)) {
                // default value for zone
                zone = "Content";
            }
            dynamic shape = _contentManager.BuildDisplay(contentItem); ;
            if (isWidget && clearWrappers) {
                // remove the widget wrappers?
                try {
                    // this won't remove any wrappers that are added later by the execution of
                    // shapes
                    shape.Metadata.Wrappers = new List<string>();
                }
                catch { }
            }

            return new ShapePartialResult(this, shape);
        }

        private ActionResult PartialNotFoundResult() {
            // ref: Orchard.Exceptions.Filters.UnhandledExceptionFilter
            var model = _orchardServices.New.NotFound();
            var request = _hca.Current().Request;

            var url = request.RawUrl;
            // If the url is relative then replace with Requested path
            model.RequestedUrl = request.Url.OriginalString.Contains(url) & request.Url.OriginalString != url ?
                request.Url.OriginalString : url;

            // Dont get the user stuck in a 'retry loop' by
            // allowing the Referrer to be the same as the Request
            model.ReferrerUrl = request.UrlReferrer != null &&
                                request.UrlReferrer.OriginalString != model.RequestedUrl ?
                request.UrlReferrer.OriginalString : null;

            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return new ShapePartialResult(this, model);
        }

        private ActionResult PartialUnauthorizedResult() {
            // Redirection for HttpUnauthorizedResult would normally be handled by 
            // Orchard.Users.Services.AuthenticationRedirectionFilter by redirecting
            // to an AccessDenied page. In this case we cannot be doig the same thing.
            // TODO: temporarily we choose to replicate a NotFound partial, just to show
            // some html, but with the correct status code. This should possibly be
            // replaced by something else.
            // ref: Orchard.Exceptions.Filters.UnhandledExceptionFilter
            var model = _orchardServices.New.NotFound();
            var request = _hca.Current().Request;

            var url = request.RawUrl;
            // If the url is relative then replace with Requested path
            model.RequestedUrl = request.Url.OriginalString.Contains(url) & request.Url.OriginalString != url ?
                request.Url.OriginalString : url;

            // Dont get the user stuck in a 'retry loop' by
            // allowing the Referrer to be the same as the Request
            model.ReferrerUrl = request.UrlReferrer != null &&
                                request.UrlReferrer.OriginalString != model.RequestedUrl ?
                request.UrlReferrer.OriginalString : null;

            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            // prevent IIS 7.0 classic mode from handling the 401 itself
            Response.SuppressFormsAuthenticationRedirect = true;
            return new ShapePartialResult(this, model);
        }
    }
}