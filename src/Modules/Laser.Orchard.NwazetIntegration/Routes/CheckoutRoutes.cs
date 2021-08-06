using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.NwazetIntegration.Routes {
    public class CheckoutRoutes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "checkout",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"},
                            {"controller", "Checkout"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "shipping",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"},
                            {"controller", "Checkout"},
                            {"action", "Shipping"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "review",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"},
                            {"controller", "Checkout"},
                            {"action", "Review"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }
    }
}