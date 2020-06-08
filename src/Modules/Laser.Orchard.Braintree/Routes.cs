using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.Braintree
{
    public class Routes : IHttpRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new HttpRouteDescriptor {
                    Priority = 5,
                    //RouteTemplate = "charta/{controller}/{action}",
                    RouteTemplate = "PaypalInit",
                    Defaults = new {
                        area = "Laser.Orchard.Braintree",
                        controller = "Paypal",
                        action = "Get"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "Pay",
                    Defaults = new {
                        area = "Laser.Orchard.Braintree",
                        controller = "Paypal",
                        action = "Post"
                    }
                }
                ,
                 new RouteDescriptor {
                    Route = new Route(
                        "Admin/PaymentGateway/Braintree/",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Braintree"},
                            {"controller", "Admin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Braintree"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}