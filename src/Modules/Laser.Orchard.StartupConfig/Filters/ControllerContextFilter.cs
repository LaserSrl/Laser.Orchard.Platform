using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Mvc.Filters;

namespace Laser.Orchard.StartupConfig.Filters {
    public class ControllerContextFilter : FilterProvider, IActionFilter {
        private readonly IControllerContextAccessor _controllercontextAccesor;
        public ControllerContextFilter(IControllerContextAccessor controllercontextAccesor) {
            _controllercontextAccesor = controllercontextAccesor;
        }
        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            _controllercontextAccesor.Context = filterContext.Controller.ControllerContext;
        }
    }
}