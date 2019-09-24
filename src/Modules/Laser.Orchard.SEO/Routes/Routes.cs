using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Laser.Orchard.SEO {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                                new RouteDescriptor {   Priority = 5,
                                                        Route = new Route(
                                                            "robots.txt",
                                                            new RouteValueDictionary {
                                                                                        {"area", "Laser.Orchard.SEO"},
                                                                                        {"controller", "Robots"},
                                                                                        {"action", "Index"}
                                                            },
                                                            new RouteValueDictionary(),
                                                            new RouteValueDictionary {
                                                                                        {"area", "Laser.Orchard.SEO"}
                                                            },
                                                            new MvcRouteHandler())
                                },
                                new RouteDescriptor {
                                    Route = new Route(
                                        "Admin/SEO/RobotsAdmin",
                                        new RouteValueDictionary {
                                            {"area", "Laser.Orchard.SEO"},
                                            {"controller", "RobotsAdmin"},
                                            {"action", "Index"}
                                        },
                                        new RouteValueDictionary(),
                                        new RouteValueDictionary {
                                            {"area", "Laser.Orchard.SEO"}
                                        },
                                        new MvcRouteHandler())
                                }
                            };
        }
    }
}