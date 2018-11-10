using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;

namespace Laser.Orchard.Fidelity.Routes {
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
                                                         "FidelityAPI/LoyalzooRegistration",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"},
                                                                                      {"controller", "FidelityAPI"},
                                                                                      {"action", "LoyalzooRegistration"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                            new RouteDescriptor {
                                                     Route = new Route(
                                                         "FidelityAPI/CustomerDetails",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"},
                                                                                      {"controller", "FidelityAPI"},
                                                                                      {"action", "CustomerDetails"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                            new RouteDescriptor {
                                                     Route = new Route(
                                                         "FidelityAPI/PlaceData",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"},
                                                                                      {"controller", "FidelityAPI"},
                                                                                      {"action", "PlaceData"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                            new RouteDescriptor {
                                                     Route = new Route(
                                                         "FidelityAPI/AddPoints",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"},
                                                                                      {"controller", "FidelityAPI"},
                                                                                      {"action", "AddPoints"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                            new RouteDescriptor {
                                                     Route = new Route(
                                                         "FidelityAPI/AddPointsFromAction",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"},
                                                                                      {"controller", "FidelityAPI"},
                                                                                      {"action", "AddPointsFromAction"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                            new RouteDescriptor {
                                                     Route = new Route(
                                                         "FidelityAPI/GiveReward",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"},
                                                                                      {"controller", "FidelityAPI"},
                                                                                      {"action", "GiveReward"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                            new RouteDescriptor {
                                                     Route = new Route(
                                                         "FidelityAPI/UpdateSocial",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"},
                                                                                      {"controller", "FidelityAPI"},
                                                                                      {"action", "UpdateSocial"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Fidelity"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
             };
        }
    }
}