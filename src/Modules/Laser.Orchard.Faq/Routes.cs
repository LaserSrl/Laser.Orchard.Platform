using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Laser.Orchard.Faq
{
    public class Routes : IRouteProvider
    {
        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            string area = "Laser.Orchard.Faq";
            return new[]{
                new RouteDescriptor{
                    Route=new Route(
                        "FaqWidgetAjax/GetTypedFaq",
                        new RouteValueDictionary {
                            {"area",area},
                            {"controller", "FaqWidgetAjax"},
                            {"action", "GetTypedFaq"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary{
                            {"area",area}
                        },
                        new MvcRouteHandler())
                }
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var route in GetRoutes())
            {
                routes.Add(route);
            }
        }
    }
}