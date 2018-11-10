using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Laser.Orchard.NewsLetters {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                    AddRoute("Admin/Newsletters", "NewsletterAdmin", "Index"),
                    AddRoute("Admin/Newsletters/Create", "NewsletterAdmin", "Create"),
                    AddRoute("Admin/Newsletters/{newsletterId}/Edit", "NewsletterAdmin", "Edit"),
                    AddRoute("Admin/Newsletters/{newsletterId}/Remove", "NewsletterAdmin", "Remove"),

                    AddRoute("Admin/Newsletters/{newsletterId}/NewsletterEditions", "NewsletterEditionAdmin", "Index"),
                    AddRoute("Admin/Newsletters/{newsletterId}/NewsletterEditions/Create", "NewsletterEditionAdmin", "Create"),
                    AddRoute("Admin/Newsletters/{newsletterId}/NewsletterEditions/{Id}/Edit", "NewsletterEditionAdmin", "Edit"),
                    AddRoute("Admin/Newsletters/{newsletterId}/NewsletterEditions/{Id}/Remove", "NewsletterEditionAdmin", "Remove"),
                    
                    AddRoute("Admin/Newsletters/{newsletterId}/Subscribers", "SubscribersAdmin", "Index"),
                    AddRoute("Admin/Newsletters/{newsletterId}/Subscribers/Create", "SubscribersAdmin", "Create"),
                    AddRoute("Admin/Newsletters/{newsletterId}/Subscribers/{Id}/Edit", "SubscribersAdmin", "Edit"),
                    AddRoute("Admin/Newsletters/{newsletterId}/Subscribers/{Id}/Remove", "SubscribersAdmin", "Remove"),

                    AddRoute("Newsletters/Subscribe", "Subscription", "Subscribe"),
                    AddRoute("Newsletters/ConfirmSubscribe", "Subscription", "ConfirmSubscribe"),
                    AddRoute("Newsletters/Unsubscribe", "Subscription", "Unsubscribe"),
                    AddRoute("Newsletters/ConfirmUnsubscribe", "Subscription", "ConfirmUnsubscribe"),
            
            };
        }

        private RouteDescriptor AddRoute(string routePattern, string controllerName, string action) {
            return new RouteDescriptor {
                Priority = 15,
                Route = new Route(
                    routePattern,
                    new RouteValueDictionary {
                            {"area", "Laser.Orchard.NewsLetters"},
                            {"controller", controllerName},
                            {"action", action}
                        },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                            {"area", "Laser.Orchard.NewsLetters"}
                        },
                    new MvcRouteHandler())
            };

        }
    }
}