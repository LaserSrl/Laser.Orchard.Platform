using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.OutputCache;
using Orchard.Roles.Models;
using Orchard.Security;

namespace Laser.Orchard.StartupConfig.CacheByRole.Filters {
    [OrchardFeature("Laser.Orchard.StartupConfig.CacheByRole")]
    public class CacheByRoleFilter : FilterProvider, IActionFilter, IResultFilter, ICachingEventHandler {
        private readonly IAuthenticationService _authenticationService;
        public CacheByRoleFilter(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public void KeyGenerated(StringBuilder key) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (currentUser != null) {
                var roles = currentUser.As<UserRolesPart>().Roles;
                if (roles.Any()) {
                    // append roles in alphabetical order
                    key.Append("UserRoles=" + 
                        String.Join(";",roles.OrderBy(r => r)));
                }
                else {
                    key.Append("UserRoles=empty-role");
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
        }
    }
}