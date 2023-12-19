using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;
using System.IO;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using System.Web;
using System.Web.Hosting;
using Laser.Orchard.ContentExtension.Services;
using System.Security.Policy;
using System.Xml.Linq;

namespace Laser.Orchard.ContentExtension
{
    [OrchardFeature("Laser.Orchard.ContentExtension.StaticContents")]
    public class StaticContentsRoutes : IHttpRouteProvider
    {
        private readonly IStaticContentsConstraint _staticContentConstraint;

        public StaticContentsRoutes(IStaticContentsConstraint staticContentConstraint)
        {
            _staticContentConstraint = staticContentConstraint;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (RouteDescriptor routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "{*path}",
                        new RouteValueDictionary {
                                                    {"area", "Laser.Orchard.ContentExtension"},
                                                    {"controller", "StaticContents"},
                                                    {"action", "Display"}
                                                },
                        new RouteValueDictionary {
                                                    {"path", _staticContentConstraint},
                                                },
                        new RouteValueDictionary {
                                                    {"area", "Laser.Orchard.ContentExtension"}
                                                },
                        new MvcRouteHandler())
                }
            };

        }
    }
}