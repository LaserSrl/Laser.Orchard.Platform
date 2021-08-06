using Orchard.Environment.Extensions;

namespace Laser.Orchard.WebServices.Helpers {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    public static class CustomRestApiHelper {

        public static string SettingsGroupId = "CustomRestApiSiteSettings";
        public static string SettingsCacheKey = "CustomRestApiSiteSettings";
    }
}