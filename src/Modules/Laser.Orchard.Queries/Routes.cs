using System;
using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace Laser.Orchard.Queries {
    public class Routes : IHttpRouteProvider {

        const string renamedRouteName = "Admin/Queries/";

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                AddRenamedRoute( "MyQueryAdmin","Index"),
                AddRenamedRoute( "MyQueryAdmin","Edit", "/Edit/{id}"),
                AddRenamedRoute( "UserQueryAdmin","Create", "/Create"),
                AddRenamedRoute( "UserQueryAdmin","CreateOneShot", "/CreateOneShot"),
                AddRenamedRoute( "UserQueryAdmin","Edit","/Edit/{id}"),
                AddRenamedRoute( "UserQueryAdmin","Preview","/Preview/{id}"),
                AddRenamedRoute( "Filter","Edit","/Edit/{id}/{category}/{type}"),
                AddRenamedRoute( "Filter","Add","/Add/{id}")
            };
        }

        private RouteDescriptor AddRenamedRoute(string controllerName, string action, string routeQueue = "") {
            RouteValueDictionary routeDictionary = new RouteValueDictionary();
            routeDictionary.Add("area", "Laser.Orchard.Queries");
            routeDictionary.Add("controller", controllerName);
            routeDictionary.Add("action", action);

            return new RouteDescriptor {
                Route = new Route(
                        renamedRouteName + controllerName + routeQueue,
                        routeDictionary,
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Queries"}
                        },
                        new MvcRouteHandler())
            };
        }
    }
}
