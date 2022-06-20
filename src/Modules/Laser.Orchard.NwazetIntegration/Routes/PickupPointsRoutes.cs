using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.NwazetIntegration.Routes {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsRoutes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/ecommerce/PickupPoints",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"},
                            {"controller", "PickupPointsAdmin"},
                            {"action", "List"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/ecommerce/PickupPoints/Create",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.NwazetIntegration"},
                            {"controller", "PickupPointsAdmin"},
                            {"action", "Create"}
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
            foreach (var descriptor in GetRoutes()) {
                routes.Add(descriptor);
            }
        }
    }
}