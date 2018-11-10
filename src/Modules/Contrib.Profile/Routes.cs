using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Contrib.Profile {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                            // higher priority than Profile/{username} as Edit could be interpreted as a username
                             new RouteDescriptor {   Priority = 6,
                                                     Route = new Route(
                                                         "Profile/Edit",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Contrib.Profile"},
                                                                                      {"controller", "Home"},
                                                                                      {"action", "Edit"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Contrib.Profile"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },
                             new RouteDescriptor {   Priority = 5,
                                                     Route = new Route(
                                                         "Profile/{username}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Contrib.Profile"},
                                                                                      {"controller", "Home"},
                                                                                      {"action", "Index"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Contrib.Profile"}
                                                                                  },
                                                         new MvcRouteHandler())

                             }
                         };
        }
    }
}