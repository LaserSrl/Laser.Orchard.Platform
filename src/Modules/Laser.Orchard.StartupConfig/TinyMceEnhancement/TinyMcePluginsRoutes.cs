//using Orchard.Mvc.Routes;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Routing;

//namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
//    public class TinyMcePluginsRoutes : IRouteProvider {
//        public IEnumerable<RouteDescriptor> GetRoutes() {
//            return new RouteDescriptor[] {
//                    new RouteDescriptor() {
//                        Route = new Route(
//                            "tinymceplugins/{plugin}",
//                            new RouteValueDictionary{
//                                {"area", "Laser.Orchard.StartupConfig"},
//                            },
//                            new RouteValueDictionary(),
//                            new RouteValueDictionary {
//                                {"area", "Laser.Orchard.StartupConfig"}
//                            },
//                            new MvcRouteHandler()
//                        )
//                    }
//            };
//        }

//        public void GetRoutes(ICollection<RouteDescriptor> routes) {
//            foreach (var routeDescriptor in GetRoutes()) {
//                routes.Add(routeDescriptor);
//            }
//        }
//    }
//}