using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace Laser.Orchard.StartupConfig {
    [OrchardFeature("Laser.Orchard.StartupConfig.MediaExtensions")]
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                    AddRoute("MediaExtensions/ImageUrl", "MediaTransform", "Image"),
            };
        }

        private RouteDescriptor AddRoute(string routePattern, string controllerName, string action) {
            return new RouteDescriptor {
                Priority = 15,
                Route = new Route(
                    routePattern,
                    new RouteValueDictionary {
                            {"area", "Laser.Orchard.StartupConfig"},
                            {"controller", controllerName},
                            {"action", action}
                        },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                            {"area", "Laser.Orchard.StartupConfig"}
                        },
                    new MvcRouteHandler())
            };

        }
    }
}