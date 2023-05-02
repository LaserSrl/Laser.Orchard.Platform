using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.WebServices.Routes {

    public class Routes : IRouteProvider {//, IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                #region [ WebApiController ]
                new RouteDescriptor {
                    Route = new Route(
                        //Laser.Orchard.Webservices/WebApi/Display?alias={alias}
                        "api/v2/content/display/{*alias}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "WebApi"},
                            {"action", "Display"},
                            {"alias", UrlParameter.Optional}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        //Laser.Orchard.Webservices/WebApi/Display?alias={alias}
                        "api/v2/content/{contenttype}/display/{*alias}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "WebApi"},
                            {"action", "Display"},
                            {"alias", UrlParameter.Optional}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        //Laser.Orchard.Webservices/WebApi/Display?alias={alias}
                        "api/v2/terms/display/{*alias}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "WebApi"},
                            {"action", "Terms"},
                            {"alias", UrlParameter.Optional}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Terms/GetIconsIds",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Terms"},
                            {"action", "GetIconsIds"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                #endregion
                #region [ LMNV - JsonController ]
                new RouteDescriptor {
                    Route = new Route(
                        //Laser.Orchard.Webservices/Json/GetByAlias?DisplayAlias={displayalias}
                        "api/v1/content/display/{*displayalias}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Json"},
                            {"action", "GetByAlias"},
                            {"displayalias", UrlParameter.Optional}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        //Laser.Orchard.Webservices/Json/GetByAlias?DisplayAlias={displayalias}
                        "api/v1/content/{contenttype}/display/{*displayalias}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Json"},
                            {"action", "GetByAlias"},
                            {"displayalias", UrlParameter.Optional}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "WebServices/Alias/",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Json"},
                            {"action", "GetByAlias"},
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Laser.Orchard.WebServices/Json/GetByAlias",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Json"},
                            {"action", "GetByAlias"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                     Priority=1,
                    Route = new Route(
                        "WebServices/ObjectAlias",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Json"},
                            {"action", "GetObjectByAlias"},
                            {"id",  UrlParameter.Optional },
                            { "version", UrlParameter.Optional},
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                #endregion
                #region [E015]
                new RouteDescriptor {
                    Route = new Route(
                        "WebServices/E015",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Json"},
                            {"action", "GetObjectByAlias"},
                            {"id",  UrlParameter.Optional },
                            { "version",1},

                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "WebServices/E015/{version}",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Json"},
                            {"id",  UrlParameter.Optional },
                            {"action", "GetObjectByAlias"},
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "WebServices/E015/ID",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Json"},
                            {"action", "GetObjectById"},
                            {"version",1}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "WebServices/E015/{version}/ID",
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"},
                            {"controller", "Json"},
                            {"action", "GetObjectById"},
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Laser.Orchard.WebServices"}
                        },
                        new MvcRouteHandler())
                }
                #endregion
            };
        }

    }
}