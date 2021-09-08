using Orchard;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.Services {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsService : IThemeSkinsService {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public ThemeSkinsService(
            IWorkContextAccessor workContextAccessor,
            IVirtualPathProvider virtualPathProvider) {

            _workContextAccessor = workContextAccessor;
            _virtualPathProvider = virtualPathProvider;
        }


        // based on the ThemeSkinsPart, find the css to load.

        // Read the /Styles/Skins folders for the current theme. Find the
        // css files to be used as skins. In the future we'll be able to get
        // also files with the same name and different extensions to implement
        // additiona sub features, like displaying a preview or a description.
        protected IEnumerable<string> GetSkinNames() {
            // get current theme
            var theme = _workContextAccessor.GetContext().CurrentTheme;
            // find the Styles/Skins folder for the theme
            var basePath = Path.Combine(theme.Location, theme.Id).Replace(Path.DirectorySeparatorChar, '/');
            var stylesPath = Path.Combine(basePath, "Styles").Replace(Path.DirectorySeparatorChar, '/');
            var skinsPath = Path.Combine(basePath, "Skins").Replace(Path.DirectorySeparatorChar, '/');
            // find the css files
            var styleSheets = _virtualPathProvider.ListFiles(skinsPath)
                .Select(Path.GetFileName)
                .Where(fileName => string.Equals(
                    Path.GetExtension(fileName),
                    ".css",
                    StringComparison.OrdinalIgnoreCase))
                .Select(fileName => Path.GetFileNameWithoutExtension(fileName));
            return styleSheets;
        }
    }
}