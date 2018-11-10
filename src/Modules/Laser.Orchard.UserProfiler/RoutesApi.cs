using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler {
    public class RoutesApi : IHttpRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            string[] Methods = new string[]{
            "PostId",
            "PostIds"
        };

            foreach (string method in Methods) {
                yield return (
                     new HttpRouteDescriptor {
                         Priority = 5,
                         RouteTemplate = "API/Tracking/" + method,
                         Defaults = new {
                             area = "Laser.Orchard.UserProfiler",
                             controller = "ProfileAPI",
                             action = method
                         }
                     }
                );
            }
        }
    }
}