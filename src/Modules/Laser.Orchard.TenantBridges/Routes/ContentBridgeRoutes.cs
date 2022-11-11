using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.TenantBridges.Routes {
    public class ContentBridgeRoutes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] { 
                new RouteDescriptor{ 
                    Route = new Route(
                        "ContentBridge/Item/Display/{Id}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.TenantBridges"},
                            {"controller", "Item"},
                            {"action", "Display"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.TenantBridges"}
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