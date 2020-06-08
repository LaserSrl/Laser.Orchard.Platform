using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;

namespace Laser.Orchard.Vimeo {
    public class VimeoRoutes : IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[]{
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/Laser.Orchard.Vimeo/VimeoUploadAPI/TryStartUpload",
                    Defaults = new {
                        area = "Laser.Orchard.Vimeo",
                        controller = "VimeoUploadAPI",
                        action = "TryStartUpload"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/Laser.Orchard.Vimeo/VimeoUploadAPI/FinishUpload",
                    Defaults = new {
                        area = "Laser.Orchard.Vimeo",
                        controller = "VimeoUploadAPI",
                        action = "FinishUpload"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/Laser.Orchard.Vimeo/VimeoUploadAPI/ErrorHandler",
                    Defaults = new {
                        area = "Laser.Orchard.Vimeo",
                        controller = "VimeoUploadAPI",
                        action = "ErrorHandler"
                    }
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Vimeo",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Vimeo"},
                            {"controller", "Admin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.Vimeo"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}