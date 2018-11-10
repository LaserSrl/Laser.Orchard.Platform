using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway {
    public class PaymentGatewayRoutes : IHttpRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] { 
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/Laser.Orchard.PaymentGateway/PaymentGatewayAPI/GetPosNames",
                    Defaults = new {
                        area = "Laser.Orchard.PaymentGateway",
                        controller = "PaymentGatewayAPI",
                        action = "GetPosNames"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/Laser.Orchard.PaymentGateway/PaymentGatewayAPI/GetValidCurrencies",
                    Defaults = new {
                        area = "Laser.Orchard.PaymentGateway",
                        controller = "PaymentGatewayAPI",
                        action = "GetValidCurrencies"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/Laser.Orchard.PaymentGateway/PaymentGatewayAPI/GetAPIFilterTerms",
                    Defaults = new {
                        area = "Laser.Orchard.PaymentGateway",
                        controller = "PaymentGatewayAPI",
                        action = "GetAPIFilterTerms"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/Laser.Orchard.PaymentGateway/PaymentGatewayAPI/GetVirtualPosUrl",
                    Defaults = new {
                        area = "Laser.Orchard.PaymentGateway",
                        controller = "PaymentGatewayAPI",
                        action = "GetVirtualPosUrl"
                    }
                }
            };
        }
    }
}