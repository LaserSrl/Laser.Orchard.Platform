using Orchard;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Routing;

namespace Laser.Orchard.ContentExtension.Services
{

    [OrchardFeature("Laser.Orchard.ContentExtension.StaticContents")]
    public class StaticContentsConstraint : IStaticContentsConstraint
    {
        private readonly IStaticContentsService _staticContentsService;

        public StaticContentsConstraint(IStaticContentsService staticContentsService)
        {
            _staticContentsService = staticContentsService;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var path = values["path"] ?? "";
            if (string.IsNullOrWhiteSpace(path.ToString()))
            {
                return false;
            }

            string staticContentsFolder = _staticContentsService.GetBaseFolder();
            string filePath = Path.Combine(staticContentsFolder, path.ToString().Replace("/", "\\"));
            return System.IO.File.Exists(filePath);
        }
    }
}