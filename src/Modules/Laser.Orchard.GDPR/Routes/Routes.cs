using Orchard.Mvc.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.GDPR.Routes {
    public class Routes : IRouteProvider {
        

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/GDPR/Index",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" },
                            {"controller", "GDPRAdmin" },
                            {"action", "Index" }
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" }
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/GDPR",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" },
                            {"controller", "GDPRAdmin" },
                            {"action", "Index" }
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" }
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/GDPR/Anonymize/{id}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" },
                            {"controller", "GDPRAdmin" },
                            {"action", "Anonymize" }
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" }
                        },
                        new MvcRouteHandler())
                 },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/GDPR/Erase/{id}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" },
                            {"controller", "GDPRAdmin" },
                            {"action", "Erase" }
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" }
                        },
                        new MvcRouteHandler())
                  },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/GDPR/SetProtection/{id}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" },
                            {"controller", "GDPRAdmin" },
                            {"action", "SetProtection" }
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" }
                        },
                        new MvcRouteHandler())
                 },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/GDPR/ResetProtection/{id}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" },
                            {"controller", "GDPRAdmin" },
                            {"action", "ResetProtection" }
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.GDPR" }
                        },
                        new MvcRouteHandler())
                 },
            };
        }

    }
}