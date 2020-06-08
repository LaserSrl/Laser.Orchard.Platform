using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.PaymentGestPay {
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
                        "Admin/PaymentGateway/PaymentGestPay/",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.PaymentGestPay"},
                            {"controller", "Admin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.PaymentGestPay"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}