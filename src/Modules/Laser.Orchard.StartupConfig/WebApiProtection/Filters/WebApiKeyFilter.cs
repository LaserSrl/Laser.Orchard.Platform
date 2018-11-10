using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using Orchard.ContentManagement;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Mvc;
using Laser.Orchard.StartupConfig.WebApiProtection.Services;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Filters {
    /// <summary>
    /// Filter for MVC Controllers
    /// </summary>
    public class WebApiKeyFilterForControllers : System.Web.Mvc.ActionFilterAttribute {

        private IWebApiFilterService _webApiFilterService;
        private IUtilsServices _utilsServices;

        private readonly bool _protectAlways;
        public WebApiKeyFilterForControllers(bool protectAlways) {
            _protectAlways = protectAlways;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            filterContext.RequestContext.GetWorkContext().TryResolve<IWebApiFilterService>(out _webApiFilterService);
            filterContext.RequestContext.GetWorkContext().TryResolve<IUtilsServices>(out _utilsServices);
            _webApiFilterService.ApplyFilter(() => base.OnActionExecuting(filterContext),
                (result) => {
                    if (filterContext == null) return;
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    var response = _utilsServices.GetResponse(ViewModels.ResponseType.UnAuthorized);
                    response.Data = result;
                    filterContext.Result = new JsonResult {
                        Data = response,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };

                    return;
                },
                () => {
                    return filterContext.RequestContext.GetWorkContext();
                }, _protectAlways);



        }
    }
    /// <summary>
    /// Filter for Api Controllers
    /// </summary>
    public class WebApiKeyFilter : System.Web.Http.Filters.ActionFilterAttribute {
        private IWebApiFilterService _webApiFilterService;
        private IUtilsServices _utilsServices;

        private readonly bool _protectAlways;
        public WebApiKeyFilter(bool protectAlways) {
            _protectAlways = protectAlways;
        }

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
             actionContext.ControllerContext.GetWorkContext().TryResolve<IWebApiFilterService>(out _webApiFilterService);
             actionContext.ControllerContext.GetWorkContext().TryResolve<IUtilsServices>(out _utilsServices);

            _webApiFilterService.ApplyFilter(() => base.OnActionExecuting(actionContext),
                                             (result) => {
                                                 if (actionContext == null) return;
                                                 var response = _utilsServices.GetResponse(ViewModels.ResponseType.UnAuthorized);
                                                 response.Data = result;
                                                 actionContext.Response = actionContext.ControllerContext.Request.CreateResponse(HttpStatusCode.Unauthorized, response, "application/json");
                                             },
                                             () => {
                                                 return actionContext.ControllerContext.GetWorkContext();
                                             }, _protectAlways);
        } 


    }


}