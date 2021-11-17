using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;
using Orchard.UI.Resources;
using System;
using System.Text;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Filters {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpFilter : FilterProvider, IActionFilter, IResultFilter {
        private readonly IResourceManager _resourceManager;
        private readonly IWorkContextAccessor _workContext;
        private readonly IMailchimpApiService _apiService;

        public const string TempDataKey = "newsletterSubscription";

        public MailchimpFilter(
            IResourceManager resourceManager,
            IWorkContextAccessor workContext,
            IMailchimpApiService apiService) {
            _resourceManager = resourceManager;
            _workContext = workContext;
            _apiService = apiService;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            // display our stuff if we are going to display a view
            if (!(filterContext.Result is ViewResultBase))
                return;

            if (!AdminFilter.IsApplied(_workContext.GetContext().HttpContext.Request.RequestContext)) {
                object fromTmp = filterContext.HttpContext.Items[TempDataKey];
                bool? isNewSubscription = fromTmp == null ? (bool?)null : (bool?)fromTmp;
                if (isNewSubscription.HasValue && isNewSubscription.Value) {
                    // add our scripts to the page's footer
                    StringBuilder script = new StringBuilder();
                    script.Append("<script type=\"text/javascript\">");
                    script.Append("window.dataLayer = window.dataLayer || [];");
                    script.Append("window.dataLayer.push({");
                    script.Append("'event': 'newsletterSubscription'");
                    script.Append("});");
                    script.Append("</script>");
                    _resourceManager.RegisterFootScript(script.ToString());
                }
            }
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            // if we have our stuff in tempdata, this is a good time to read it
            object fromTmp = filterContext.Controller.TempData[TempDataKey];
            bool? isNewSubscription = fromTmp == null ? (bool?)null : (bool?)fromTmp;
            if (!isNewSubscription.HasValue) {
                return;
            }

            // and add our stuff back into tempdata so it's not lost
            filterContext.HttpContext.Items[TempDataKey] = isNewSubscription.Value;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            object fromTmp = filterContext.HttpContext.Items[TempDataKey];
            bool? isNewSubscription = fromTmp == null ? (bool?)null : (bool?)fromTmp;
            if (
                // we have done a new subscription from the service
                _apiService.IsNewSubscription
                // or we had registered that value from a previous call
                || (isNewSubscription.HasValue && isNewSubscription.Value)) {

                if (filterContext.Result is ViewResultBase) {
                    // make sure we are carrying forwards the information about the 
                    // fact we've had a new subscription
                    filterContext.HttpContext.Items[TempDataKey] = true;
                    return;
                }
                // not a view, so carry the information in the tempdata for the controller
                filterContext.Controller.TempData[TempDataKey] = true;
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
            // nothing to do here
        }
    }
}