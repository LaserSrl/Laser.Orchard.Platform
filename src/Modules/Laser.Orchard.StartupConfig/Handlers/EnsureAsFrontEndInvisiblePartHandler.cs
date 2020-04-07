using System.Net;
using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.UI.Admin;
using Orchard.Autoroute.Models;
using System.Web.Routing;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class EnsureAsFrontEndInvisiblePartHandler : ContentHandler {
        private readonly IWorkContextAccessor _workContext;
        private string _currentRoute = "";
        private HttpRequestBase _request;
        const string CONTENT_DISPLAY_ROUTE = "Contents/Item/Display/{0}";


        public EnsureAsFrontEndInvisiblePartHandler(IWorkContextAccessor workContext) {
            _workContext = workContext;
        }
        protected override void BuildDisplayShape(BuildDisplayContext context) {
            if (_currentRoute == "") {
                _request = _workContext.GetContext().HttpContext.Request;
                var area = _request.RequestContext.RouteData.Values["area"];
                var controller = _request.RequestContext.RouteData.Values["controller"];
                var action = _request.RequestContext.RouteData.Values["action"];
                var id = 0;
                int.TryParse(_request.RequestContext.RouteData.Values["id"]?.ToString(), out id);
                _currentRoute = string.Format("{0}/{1}/{2}/{3}", area, controller, action, id);
            }
            if (_currentRoute.Equals(string.Format(CONTENT_DISPLAY_ROUTE, context.ContentItem.Id), StringComparison.InvariantCultureIgnoreCase)) {
                var isAutoroute = context.ContentItem.Is<AutoroutePart>();
                var isInvisible = context.ContentItem.Is<EnsureAsFrontEndInvisiblePart>();
                if (!isAutoroute || isInvisible) {
                    // The browser is trying to display a content without an AutoroutePart 
                    // or is trying to display in the front-end a content having the specific part for hiding content in front-end
                    _workContext.GetContext().HttpContext.Response.RedirectToRoute(new { Area = "Common", Controller = "Error", Action = "NotFound" });
                }
            }
        }
    }
}
