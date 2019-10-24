using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace Laser.Orchard.AdvancedSearch {
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
                        "Admin/AdvancedSearch/List",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.AdvancedSearch"},
                            {"controller", "Admin"},
                            {"action", "List"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.AdvancedSearch"}
                        },
                        new MvcRouteHandler())
                }
        };

        }
    }
}
