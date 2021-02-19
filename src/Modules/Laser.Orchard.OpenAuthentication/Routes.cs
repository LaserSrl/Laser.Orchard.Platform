using Laser.Orchard.OpenAuthentication.Extensions;
using Orchard.Mvc.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.OpenAuthentication {
    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/OpenAuthentication",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "Index"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/OpenAuthentication/Remove",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "Remove"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/OpenAuthentication/CreateProvider",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "CreateProvider"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "External/LogOn",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "Account"},
                                                                                      {"action", "ExternalLogOn"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                            new RouteDescriptor {
                                                     Route = new Route(
                                                         "External/TokenLogOn",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "Account"},
                                                                                      {"action", "ExternalTokenLogOn"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                  
                                                 },
                            new RouteDescriptor {
                                                     Route = new Route(
                                                         "External/WebTokenLogOn",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "Account"},
                                                                                      {"action", "ExternalTokenLogonWeb"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())

                                                 },
                            new RouteDescriptor {
                                                     Route = new Route(
                                                         "AKExternal/TokenLogOn",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "AKAccount"},
                                                                                      {"action", "ExternalTokenLogOn"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())

                                                 },
                         };
        }
    }
}