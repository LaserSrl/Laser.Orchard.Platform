using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;

namespace Laser.Orchard.WebServices.Routes {
    public class SignalRESTApiRoutes : IHttpRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            // TODO: we could add a way to have further routes dynamically
            // (through providers/settings).
            yield return (
                new HttpRouteDescriptor {
                    // Aliases formed by Autoroute have Priority 80
                    // Route name MUST be unique -> Ensure that by checking other GetRoutes() functions.
                    Name = "SignalRESTApiRoutes",
                    Priority = 85,
                    RouteTemplate = "api/signal/{signalName}",
                    Defaults = new {
                        area = "Laser.Orchard.WebServices",
                        controller = "SignalApi",
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