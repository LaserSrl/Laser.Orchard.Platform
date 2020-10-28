using Orchard;
using Orchard.Mvc.Filters;
using Orchard.Taxonomies.Controllers;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;

namespace Contrib.Widgets.Filters {
    public class WidgetFilter : FilterProvider, IActionFilter {

        private readonly IWorkContextAccessor _workContext;

        private string executedActionUrl = string.Empty;
        private string returnTermUrl = string.Empty;

        public WidgetFilter(
            IWorkContextAccessor workContext) {
            _workContext = workContext;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            if (_workContext.GetContext().CurrentUser != null &&
                filterContext.Controller is TermAdminController &&
                filterContext.ActionDescriptor.ActionName.Equals("Edit")) {

                if (filterContext.HttpContext.Request.Form["executed-action"] != null) {
                    executedActionUrl = filterContext.HttpContext.Request.Form["executed-action"].ToString();
                    returnTermUrl = filterContext.HttpContext.Request.RawUrl;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            if (_workContext.GetContext().CurrentUser != null &&
                filterContext.Controller is TermAdminController &&
                filterContext.ActionDescriptor.ActionName.Equals("Edit")) {

                if (!string.IsNullOrWhiteSpace(executedActionUrl) &&
                    filterContext.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK) {
                    filterContext.Result = new RedirectResult(executedActionUrl);
                }
            }
        }
    }
}