using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.PaymentCartaSi {
    public class Routes : IHttpRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                 new RouteDescriptor {
                    Route = new Route(
                        "Admin/PaymentGateway/PaymentCartaSi/",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.PaymentCartaSi"},
                            {"controller", "Admin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.PaymentCartaSi"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}