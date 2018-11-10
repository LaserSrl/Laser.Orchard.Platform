using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Laser.Orchard.Policy {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                    AddRoute("{lang}/policies", "Policies", "Index"),
                    AddRoute("{lang}/policies/Save", "Policies", "SavePolicies"),
                    AddRoute("{lang}/policies/Edit", "Policies", "Index", new Dictionary<string,object>(){ { "editMode", true } }),
                    AddRoute("policies", "Policies", "Index"),
                    AddRoute("policies/Save", "Policies", "SavePolicies"),
                    AddRoute("policies/Edit", "Policies", "Index", new Dictionary<string,object>(){ { "editMode", true } })
            };
        }

        private RouteDescriptor AddRoute(string routePattern, string controllerName, string action, Dictionary<string,object> parameters = null) {
            Dictionary<string, object> routeDictionary = new Dictionary<string, object>();

            routeDictionary.Add("area", "Laser.Orchard.Policy");
            routeDictionary.Add("controller", controllerName);
            routeDictionary.Add("action", action);

            if (parameters != null)
                routeDictionary = routeDictionary.Union(parameters).ToDictionary(k => k.Key, v => v.Value);

            return new RouteDescriptor {
                Priority = 15,
                Route = new Route(
                    routePattern,
                    new RouteValueDictionary(routeDictionary),
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                            {"area", "Laser.Orchard.Policy"}
                        },
                    new MvcRouteHandler())
            };

        }
    }
}