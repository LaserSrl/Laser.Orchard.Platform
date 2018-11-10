using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.UserProfiler {
    public class Routes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            string area = "Laser.Orchard.UserProfiler";
            return new[]{
                new RouteDescriptor{
                    Route=new Route(
                        "Tracking/PostId",
                        new RouteValueDictionary {
                            {"area",area},
                            {"controller", "Profile"},
                            {"action", "PostId"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary{
                            {"area",area}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor{
                    Route=new Route(
                        "Tracking/PostIds",
                        new RouteValueDictionary {
                            {"area",area},
                            {"controller", "Profile"},
                            {"action", "PostIds"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary{
                            {"area",area}
                        },
                        new MvcRouteHandler())
                }
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var route in GetRoutes()) {
                routes.Add(route);
            }
        }
    }
}