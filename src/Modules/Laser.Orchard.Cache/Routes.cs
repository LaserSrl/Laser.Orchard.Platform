using System;
using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace Laser.Orchard.Cache {
    public class Routes : IHttpRouteProvider {

        const string renamedRouteName = "Admin/Cache/";

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                AddRenamedRoute("CacheURLAdmin","Index"),
                AddRenamedRoute("CacheURLAdmin","Edit", "/Edit/{id}"),
            };
        }

        private RouteDescriptor AddRenamedRoute(string controllerName, string action, string routeQueue = "") {
            RouteValueDictionary routeDictionary = new RouteValueDictionary();

            routeDictionary.Add("area", "Laser.Orchard.Cache");
            routeDictionary.Add("controller", controllerName);
            routeDictionary.Add("action", action);

            return new RouteDescriptor {
                Route = new Route(
                        renamedRouteName + controllerName + routeQueue,
                        routeDictionary,
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Cache"}
                        },
                        new MvcRouteHandler())
            };
        }
    }
}