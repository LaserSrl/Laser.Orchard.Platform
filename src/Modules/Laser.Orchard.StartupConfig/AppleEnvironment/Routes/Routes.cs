using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.AppleEnvironment.Routes {
    [OrchardFeature("Laser.Orchard.ApplePay.DomainAssociation")]
    public class Routes : IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        ".well-known/apple-developer-merchantid-domain-association",
                        new RouteValueDictionary {
                                                    {"area", "Laser.Orchard.StartupConfig"},
                                                    {"controller", "MerchantDomainAssociation"},
                                                    {"action", "Index"}
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
