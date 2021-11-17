using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Resources;
using Orchard.Users.Events;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.GoogleAnalytics.Handlers {
    [OrchardFeature("Laser.Orchard.GoogleAnalytics")]
    public class UserEventHandler : FilterProvider, IUserEventHandler, IActionFilter, IResultFilter {
        private readonly IWorkContextAccessor _workContext;
        private readonly IResourceManager _resourceManager;

        private bool isNewRegistration { get; set; }
        private const string TempDataKey = "userRegistrationSuccessfull";

        public UserEventHandler(
            IWorkContextAccessor workContext,
            IResourceManager resourceManager) {
            _workContext = workContext;
            _resourceManager = resourceManager;

            isNewRegistration = false;
        }

        public void Approved(IUser user) {
            if (!isNewRegistration) {
                AddEventScript(user);
            }
        }

        public void ConfirmedEmail(IUser user) {
            if (!isNewRegistration) {
                AddEventScript(user);
            }
        }

        private void AddEventScript(IUser user) {
            if (user.As<UserPart>().EmailStatus != UserStatus.Approved) {
                isNewRegistration = false;
            } else {
                isNewRegistration = true;
            }
            _workContext.GetContext().HttpContext.Items[TempDataKey] = isNewRegistration;
        }

        #region not used IUserEventHandler methods 
        public void AccessDenied(IUser user) { }
        public void ChangedPassword(IUser user) { }
        public void Created(UserContext context) { }
        public void Creating(UserContext context) { }
        public void LoggedIn(IUser user) { }
        public void LoggedOut(IUser user) { }
        public void LoggingIn(string userNameOrEmail, string password) { }
        public void LogInFailed(string userNameOrEmail, string password) { }
        public void Moderate(IUser user) { }
        public void SentChallengeEmail(IUser user) { }
        #endregion

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
                    script.Append("'event': 'userRegistrationSuccessfull'");
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
                isNewRegistration
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