using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.MultiStepAuthentication.Routes {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceLoginRoutes : IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "api/noncelogin",
                    Defaults = new {
                        area = "Laser.Orchard.MultiStepAuthentication",
                        controller = "NonceLoginApi"
                    }
                },
                new RouteDescriptor {
                        Route = new Route(
                        "Admin/NonceLoginSettings",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.MultiStepAuthentication"},
                            {"controller", "NonceLoginAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.MultiStepAuthentication"}
                        },
                        new MvcRouteHandler())
                },
                    new RouteDescriptor {
                    Route = new Route(
                        "NonceAppCamouflage",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.MultiStepAuthentication"},
                            {"controller", "NonceAppCamouflage"},
                            {"action", "GetByURL"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.MultiStepAuthentication"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}