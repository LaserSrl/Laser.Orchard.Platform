namespace Proligence.QrCodes {
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Orchard.Mvc.Routes;

    public class Routes : IRouteProvider {
        #region IRouteProvider Members

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Priority = 12,
                    Route = new Route(
                        "QrCode/{id}",
                        new RouteValueDictionary {
                            {"area", "Proligence.QrCodes"},
                            {"controller", "Image"},
                            {"action", "Render"},
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Proligence.QrCodes"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

        #endregion
    }
}