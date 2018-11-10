using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;

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
            };
        }
    }
}