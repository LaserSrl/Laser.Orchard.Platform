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

        const string renamedRouteName = "Admin/ComunicationGateway/";

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                AddRoute("Api/CommunicationGateway/Get/{Id}", "DeliveryReport", "Get"),
                AddRoute("Api/CommunicationGateway/GetByExternalId/{ExternalId}", "DeliveryReport", "GetByExternalId"),
                AddRenamedRoute("CampaignAdmin","Index"),
                AddRenamedRoute("CampaignAdmin","Edit", "/Edit/{id}"),
                AddRenamedRoute("AdvertisingAdmin","Index","/{id}"),         
                AddRenamedRoute("AdvertisingAdmin","Edit","/Edit/{id}/{idCampaign}"),
                AddRenamedRoute("ContactsAdmin","Index"),
                AddRenamedRoute("ContactsAdmin","IndexSearch", "/IndexSearch"),
                AddRenamedRoute("ContactsAdmin","View","/{id}"),
                AddRenamedRoute("ContactsAdmin","Edit","/Edit/{id}")
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

        private RouteDescriptor AddRenamedRoute(string controllerName, string action, string routeQueue = "") {
            RouteValueDictionary routeDictionary = new RouteValueDictionary();
            routeDictionary.Add("area", "Laser.Orchard.CommunicationGateway");
            routeDictionary.Add("controller", controllerName);
            routeDictionary.Add("action", action);

            return new RouteDescriptor {
                Route = new Route(
                        renamedRouteName + controllerName + routeQueue,
                        routeDictionary,
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.CommunicationGateway"}
                        },
                        new MvcRouteHandler())
            };
        }


    }
}