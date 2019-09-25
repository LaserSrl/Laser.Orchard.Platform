using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.Questionnaires {
    public class Routes : IHttpRouteProvider {

        const string renamedRouteName = "Admin/Questionnaires/";

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "api/laser.questionnaireresponse/{qContext}",
                    Defaults = new {
                        area = "Laser.Orchard.Questionnaires",
                        controller = "QuestionnaireResponse",
                        action = "PostContext"
                    }
                },
                AddRenamedRoute("QuestionnaireStats","Index"),
                AddRenamedRoute("QuestionnaireStats","Detail", "/{idQuestionario}"),
                AddRenamedRoute("QuestionnaireStats","QuestionDetail", "/{idQuestionario}/{idDomanda}"),
                AddRenamedRoute("AdminRanking","Index"),
                AddRenamedRoute("AdminRanking","GetListSingleGame", "/{ID}/{deviceType}"),
                AddRenamedRoute("AdminRanking","GetListSingleGame", "/{ID}")
            };
        }

        private RouteDescriptor AddRenamedRoute(string controllerName, string action, string routeQueue = "") {
            RouteValueDictionary routeDictionary = new RouteValueDictionary();
            routeDictionary.Add("area", "Laser.Orchard.Questionnaires");
            routeDictionary.Add("controller", controllerName);
            routeDictionary.Add("action", action);

            return new RouteDescriptor {
                Route = new Route(
                        renamedRouteName + controllerName + routeQueue,
                        routeDictionary,
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Questionnaires"}
                        },
                        new MvcRouteHandler())
            };
        }
    }
}