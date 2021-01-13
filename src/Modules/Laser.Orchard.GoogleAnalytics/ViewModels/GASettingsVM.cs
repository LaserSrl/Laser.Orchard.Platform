using Laser.Orchard.GoogleAnalytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.GoogleAnalytics.ViewModels {
    public class GASettingsVM {
        public GASettingsVM(GoogleAnalyticsSettingsPart part) {
            GoogleAnalyticsKey = part?.GoogleAnalyticsKey ?? string.Empty;
            UseTagManager = part?.UseTagManager ?? false;
            TrackOnAdmin = part?.TrackOnAdmin ?? false;
            TrackOnFrontEnd = part?.TrackOnFrontEnd ?? false;
            DomainName = part?.DomainName ?? string.Empty;
            AnonymizeIp = part?.AnonymizeIp ?? false;
        }

        public string GoogleAnalyticsKey { get; set; }
        public bool UseTagManager { get; set; }
        public bool TrackOnAdmin { get; set; }
        public bool TrackOnFrontEnd { get; set; }
        public string DomainName { get; set; }
        public bool AnonymizeIp { get; set; }
    }
}