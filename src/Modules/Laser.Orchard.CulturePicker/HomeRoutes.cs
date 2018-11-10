using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace Laser.Orchard.CulturePicker {
    [OrchardFeature("Laser.Orchard.CulturePicker.HomePageRedirect")]
    public class HomeRoutes : IRouteProvider {
        #region IRouteProvider Members

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                //it's igly, but works
                //TODO: find more elegant way without controller
                new RouteDescriptor {
                    Name = "Homepage",
                    Priority = 85,
                    Route = new Route(
                        "",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.CulturePicker"},
                            {"controller", "LocalizableHome"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.CulturePicker"},
                            {"controller", "LocalizableHome"},
                        },
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.CulturePicker"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

        #endregion
    }
}