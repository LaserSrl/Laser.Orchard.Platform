using Laser.Orchard.ContentExtension.Services;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

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