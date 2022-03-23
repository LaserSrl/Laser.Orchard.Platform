using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Mvc.ViewEngines;
using Orchard.Mvc.ViewEngines.Razor;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.TenantViewEngine {
    [OrchardSuppressDependency("Orchard.Mvc.ViewEngines.Razor.RazorViewEngineProvider")]
    public class TenantRazorViewEngineProvider : 
        RazorViewEngineProvider, IViewEngineProvider, IShapeTemplateViewEngine {

        // This class should make it so we can make tenant level alternates for 
        // the shapes used for controller actions, same as we are doing for the
        // alternates for parts. It's the "tenant aware" engine corresponding to
        //      Orchard.Mvc.ViewEngines.Razor.RazorViewEngineProvider

        public TenantRazorViewEngineProvider(
            ShellSettings shellSettings) {
            Logger = NullLogger.Instance;
            RazorCompilationEventsShim.EnsureInitialized();

            _alternateFolderBasePath = 
                Path.Combine(@"~/App_Data/Sites", shellSettings.Name, "Alternates")
                    .Replace(Path.DirectorySeparatorChar, '/');
        }
        static readonly string[] DisabledFormats = new[] { "~/Disabled" };
        private readonly string _alternateFolderBasePath;

        public new IViewEngine CreateThemeViewEngine(CreateThemeViewEngineParams parameters) {
            // This replicates the implementation from the base class, while adding the paths for
            // the alternates for the tenant. Ideally, we would get the viewEngine from
            // base.CreateThemeViewEngine(parameters) and change it dynamically.

            // We have to get this as dynamic because IViewEngine doesn't expose the properties
            // we need to alter.
            var baseEngine = (dynamic)base.CreateThemeViewEngine(parameters);

            // parameters.VirtualPath is
            // ~/Themes/Theme.Id
            // We need only the Theme.Id for our tenant alternates
            var themeId = parameters.VirtualPath.Substring(9);
            var alternatesPath = Path.Combine(_alternateFolderBasePath, themeId)
                    .Replace(Path.DirectorySeparatorChar, '/');

            // compute paths that we need to add BEFORE the paths computed by the base implementation
            var partialViewLocationFormats = new List<string>() {
                alternatesPath + "/Views/{0}.cshtml",
                alternatesPath + "/Views/{1}/{0}.cshtml",
            };
            // add "original" paths
            partialViewLocationFormats.AddRange((IEnumerable<string>)baseEngine.PartialViewLocationFormats);
            // replace the paths in the base engine
            baseEngine.PartialViewLocationFormats = partialViewLocationFormats.ToArray();

            // compute paths that we need to add BEFORE the paths computed by the base implementation
            var areaPartialViewLocationFormats = new List<string>() {
                alternatesPath + "/Views/{2}/{1}/{0}.cshtml",
            };
            // add "original" paths
            areaPartialViewLocationFormats.AddRange((IEnumerable<string>)baseEngine.AreaPartialViewLocationFormats);
            // replace the paths in the base engine
            baseEngine.AreaPartialViewLocationFormats = areaPartialViewLocationFormats.ToArray();

            // change the cache location for the views, otherwise the view engine will still mess things up
            // down the line
            baseEngine.ViewLocationCache = new ThemeViewLocationCache(alternatesPath);

            return (RazorViewEngine)baseEngine;
        }

    }

    
}