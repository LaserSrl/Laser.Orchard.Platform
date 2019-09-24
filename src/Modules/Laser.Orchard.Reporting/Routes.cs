using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.Reporting {
    public class Routes : IHttpRouteProvider {

        const string renamedRouteName = "Admin/Reporting/";

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                AddRenamedRoute("Report","Index"),
                AddRenamedRoute("Report","ShowReports", "/ShowReports"),
                AddRenamedRoute("Report","DashboardList", "/DashboardList"),
                AddRenamedRoute("Report","Create", "/Create"),
                AddRenamedRoute("Report","CreateHql", "/CreateHql"),
                AddRenamedRoute("Report","EditHql", "/EditHql/{id}"),
                AddRenamedRoute("Report","Edit", "/Edit/{id}"),
                AddRenamedRoute("Report","ShowDashboard", "/ShowDashboard/{id}"),
                AddRenamedRoute("Report","Display", "/Display/{id}")

            };
        }

        private RouteDescriptor AddRenamedRoute(string controllerName, string action, string routeQueue = "") {
            RouteValueDictionary routeDictionary = new RouteValueDictionary();
            routeDictionary.Add("area", "Laser.Orchard.Reporting");
            routeDictionary.Add("controller", controllerName);
            routeDictionary.Add("action", action);

            return new RouteDescriptor {
                Route = new Route(
                        renamedRouteName + controllerName + routeQueue,
                        routeDictionary,
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Reporting"}
                        },
                        new MvcRouteHandler())
            };
        }
    }
}