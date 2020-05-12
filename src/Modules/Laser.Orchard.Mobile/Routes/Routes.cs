using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.Routes {
    public class Routes : IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "apple-app-site-association",
                        new RouteValueDictionary {
                                                    {"area", "Laser.Orchard.Mobile"},
                                                    {"controller", "ManifestAppFile"},
                                                    {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", "Laser.Orchard.Mobile"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        ".well-known/apple-app-site-association",
                        new RouteValueDictionary {
                                                    {"area", "Laser.Orchard.Mobile"},
                                                    {"controller", "ManifestAppFile"},
                                                    {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", "Laser.Orchard.Mobile"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        ".well-known/apple-developer-domain-association.txt",
                        new RouteValueDictionary {
                                                    {"area", "Laser.Orchard.Mobile"},
                                                    {"controller", "ManifestAppFile"},
                                                    {"action", "DeveloperDomain"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", "Laser.Orchard.Mobile"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Mobile/ManifestAppFile",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Mobile"},
                            {"controller", "ManifestAppFileAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Mobile"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/DevTools/PushNotification",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Mobile"},
                            {"controller", "PushNotification"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Mobile"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}
