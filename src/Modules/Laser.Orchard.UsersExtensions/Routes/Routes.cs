using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.UsersExtensions.Routes {

    public class Routes : IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Policies/List",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.UsersExtensions"},
                            {"controller", "Policies"},
                            {"action", "GetPolicyList"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.UsersExtensions"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

    }
}