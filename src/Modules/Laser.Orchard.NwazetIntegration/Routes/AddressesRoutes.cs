using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.NwazetIntegration.Routes {
    public class AddressesRoutes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "addresses",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"},
                            {"controller", "Addresses"},
                            {"action", "MyAddresses"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "addresses/create",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"},
                            {"controller", "Addresses"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "addresses/{id}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"},
                            {"controller", "Addresses"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "addresses/delete/{id}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"},
                            {"controller", "Addresses"},
                            {"action", "Delete"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"}
                        },
                        new MvcRouteHandler())
                },
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }
    }
}