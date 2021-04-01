using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Routes {
    public class CouponRESTApiRoutes : IHttpRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            yield return (
                new HttpRouteDescriptor {
                    // Aliases formed by Autoroute have Priority 80
                    Name = "DefaultApi",
                    Priority = 85,
                    RouteTemplate = "API/Coupon",
                    Defaults = new {
                        area = "Laser.Orchard.NwazetIntegration",
                        controller = "CouponRESTApi"
                    }
                }
            );
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }
    }
}