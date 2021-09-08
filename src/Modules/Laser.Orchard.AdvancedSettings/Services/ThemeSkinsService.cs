using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.Services {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsService : IThemeSkinsService {

        // based on the ThemeSkinsPart, find the css to load.

        // Read the /Styles/Skins folders for the current theme. Find the
        // css files to be used as skins. In the future we'll be able to get
        // also files with the same name and different extensions to implement
        // additiona sub features, like displaying a preview or a description.
    }
}