using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.WebServices.Helpers {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    public static class CustomRestApiHelper {

        public static string SettingsGroupId = "CustomRestApiSiteSettings";
        public static string SettingsCacheKey = "CustomRestApiSiteSettings";
    }
}