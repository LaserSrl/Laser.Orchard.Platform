using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.PaymentGateway {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                    AddRoute("{lang}/payment/pay", "Payment", "Pay"),
                    AddRoute("{lang}/payment/info", "Payment", "Info"),
                    AddRoute("payment/pay", "Payment", "Pay"),
                    AddRoute("payment/info", "Payment", "Info"),

                    new RouteDescriptor {
                    Route = new Route(
                        "Admin/PaymentGateway/",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.PaymentGateway"},
                            {"controller", "Admin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.PaymentGateway"}
                        },
                        new MvcRouteHandler())
                }
                    ,
                    new RouteDescriptor {
                    Route = new Route(
                        "Admin/PaymentGateway/PaymentInfo/ListAll",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.PaymentGateway"},
                            {"controller", "PaymentInfo"},
                            {"action", "ListAll"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.PaymentGateway"}
                        },
                        new MvcRouteHandler())
                }


            };
        }

        private RouteDescriptor AddRoute(string routePattern, string controllerName, string action, Dictionary<string, object> parameters = null) {
            Dictionary<string, object> routeDictionary = new Dictionary<string, object>();

            routeDictionary.Add("area", "Laser.Orchard.PaymentGateway");
            routeDictionary.Add("controller", controllerName);
            routeDictionary.Add("action", action);

            if (parameters != null)
                routeDictionary = routeDictionary.Union(parameters).ToDictionary(k => k.Key, v => v.Value);

            return new RouteDescriptor {
                Priority = 15,
                Route = new Route(
                    routePattern,
                    new RouteValueDictionary(routeDictionary),
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                            {"area", "Laser.Orchard.PaymentGateway"}
                        },
                    new MvcRouteHandler())
            };
        }
    }
}