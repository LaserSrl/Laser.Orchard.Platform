using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace Laser.Orchard.ContentExtension {
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
                        "Admin/ContentExtension/Settings",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.ContentExtension"},
                            {"controller", "Admin"},
                            {"action", "Settings"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.ContentExtension"}
                        },
                        new MvcRouteHandler())
                },
            new RouteDescriptor {
                    Route = new Route(
                        "Admin/ContentExtension/DynamicProjectionDisplay/List/{contentid}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.ContentExtension"},
                            {"controller", "DynamicProjectionDisplay"},
                            {"action", "List"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.ContentExtension"}
                        },
                        new MvcRouteHandler())
                }
            };

        }
    }
}