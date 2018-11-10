using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace KrakeAdmin {
    public class AdminThemeOverride : IThemeSelector {
        public ThemeSelectorResult GetTheme(RequestContext context) {
            if (AdminFilter.IsApplied(context)) {
                return new ThemeSelectorResult { Priority = 300, ThemeName = "KrakeAdmin" };
            }
            return null;
        }
    }
}