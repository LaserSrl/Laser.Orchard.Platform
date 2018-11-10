using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Mvc.Routes;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement.Routes {
    [OrchardFeature("Laser.Orchard.WebTracking")]
    public class WebTrackingRoutes : IHttpRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "api/webtrack",
                    Defaults = new {
                        area = "Laser.Orchard.TemplateManagement",
                        controller = "WebTrackingApi"
                    }
                }
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }
    }
}