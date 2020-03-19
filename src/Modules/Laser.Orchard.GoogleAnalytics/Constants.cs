using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.GoogleAnalytics {
    public static class Constants {
        public const string SiteSettingsCacheKey =
            "Laser.Orchard.GoogleAnalytics.GetSiteSettings";
        public const string SiteSettingsEvictSignal =
            "GoogleAnalyticsSettingsPart_EvictCache";
    }
}