using Orchard.Mvc.Filters;
using Orchard.Themes;
using Orchard.UI.Resources;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.Filters {
    public class CPDisableThemeFilter : FilterProvider, IActionFilter {
        private readonly IResourceManager _resourceManager;

        public CPDisableThemeFilter(IResourceManager resourceManager) {
            _resourceManager = resourceManager;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            if (filterContext.ActionDescriptor.ActionName == "Create" && filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "Admin") {
                //ThemeFilter.Disable(filterContext.RequestContext);

                var cssPath = "/Themes/TheAdmin/Styles/test.css";
                _resourceManager.Include("stylesheet", cssPath, cssPath).AtFoot();
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }
    }
}