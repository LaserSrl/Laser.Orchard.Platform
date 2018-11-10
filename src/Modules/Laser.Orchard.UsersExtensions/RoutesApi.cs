using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UsersExtensions {
    public class RoutesApi : IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }


        public IEnumerable<RouteDescriptor> GetRoutes() {
            string[] Methods = new string[]{
            "GetCleanRegistrationPolicies",
            "GetRegistrationPolicies",
            "GetUserRegistrationModel",
            "GetCleanRegistrationPoliciesSsl",
            "GetRegistrationPoliciesSsl",
            "GetUserRegistrationModelSsl",
            "RegisterSsl",
            "SignInSsl",
            "SignOutSsl",
            "RequestLostPasswordSmsSsl",
            "RequestLostPasswordAccountOrEmailSsl",
            "Register",
            "SignIn",
            "SignOut",
            "RequestLostPasswordSms",
            "RequestLostPasswordAccountOrEmail"
        };

            foreach (string method in Methods) {
                yield return (
                     new HttpRouteDescriptor {
                         Priority = 5,
                         RouteTemplate = "API/UserActions/" + method,
                         Defaults = new {
                             area = "Laser.Orchard.UsersExtensions",
                             controller = "UserActionsAPI",
                             action = method
                         }
                     }
                );
            }
        }
    }
}