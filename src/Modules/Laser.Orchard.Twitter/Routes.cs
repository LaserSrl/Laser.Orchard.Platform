using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;


namespace Laser.Orchard.Twitter {
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
                        "Admin/Twitter",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Twitter"},
                            {"controller", "TwitterAccount"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Twitter"}
                        },
                        new MvcRouteHandler())
                }
        };

        }
    }
}