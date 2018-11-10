using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Mvc.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace itWORKS.ExtendedRegistration
{
    /// <summary>
    /// Here we define the routing scheme for our module.
    /// If you're coming from ASP.NET MVC development, 
    /// you do basically the same thing as in Global.asax.cs in ordinary ASP.NET MVC app.
    /// </summary>
    public class Routes : IRouteProvider
    {

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                
                    new RouteDescriptor {
                    Priority = 19,
                    Route = new Route(
                        "Users/Account/Register",
                        new RouteValueDictionary {
                            {"area", "itWORKS.ExtendedRegistration"},
                            {"controller", "Account"},
                            {"action", "Register"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "itWORKS.ExtendedRegistration"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}