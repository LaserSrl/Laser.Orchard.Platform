using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.CommunicationGateway {
    public class Routes : IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                AddRoute("Api/CommunicationGateway/Get/{Id}", "DeliveryReport", "Get"),
                AddRoute("Api/CommunicationGateway/GetByExternalId/{ExternalId}", "DeliveryReport", "GetByExternalId")
            };
        }

        private RouteDescriptor AddRoute(string routePattern, string controllerName, string action) {
            return new HttpRouteDescriptor {

                Priority = 15,
                RouteTemplate = routePattern,
                Defaults = new {
                    area = "Laser.Orchard.CommunicationGateway",
                    controller = controllerName,
                    action = action
                }
            };
        }

    }
}