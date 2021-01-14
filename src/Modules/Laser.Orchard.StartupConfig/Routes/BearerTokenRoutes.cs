using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenRoutes : IHttpRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            yield return (
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/BearerToken/Auth",
                    Defaults = new {
                        area = "Laser.Orchard.StartupConfig",
                        controller = "BearerToken",
                        action = "Auth"
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