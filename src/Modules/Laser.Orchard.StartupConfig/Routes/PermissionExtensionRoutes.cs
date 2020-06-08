using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig {
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class PermissionExtensionRoutes : IHttpRouteProvider {


        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/UsersGroupsSettings",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.StartupConfig"},
                            {"controller", "UsersGroupsSettings"},
                            {"action", "Settings"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.StartupConfig"}
                        },
                        new MvcRouteHandler())
                }
        };

        }
    }
}