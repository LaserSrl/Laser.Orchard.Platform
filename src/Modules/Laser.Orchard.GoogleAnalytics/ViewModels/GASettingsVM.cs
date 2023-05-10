using Laser.Orchard.Cookies;
using Laser.Orchard.GoogleAnalytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.GoogleAnalytics.ViewModels {
    public class GASettingsVM {
        public GASettingsVM(GoogleAnalyticsSettingsPart part) {
            GoogleAnalyticsKey = part?.GoogleAnalyticsKey ?? string.Empty;
            GTMContainerId = part?.GTMContainerId ?? string.Empty;
            UseTagManager = part?.UseTagManager ?? false;
            TrackOnAdmin = part?.TrackOnAdmin ?? false;
            TrackOnFrontEnd = part?.TrackOnFrontEnd ?? false;
            TrackGTMOnAdmin = part?.TrackGTMOnAdmin ?? false;
            TrackGTMOnFrontEnd = part?.TrackGTMOnFrontEnd ?? false;
            DomainName = part?.DomainName ?? string.Empty;
            AnonymizeIp = part?.AnonymizeIp ?? false;
            CookieLevel = part?.CookieLevel ?? CookieType.Statistical;
        }

        public string GoogleAnalyticsKey { get; set; }
        public string GTMContainerId { get; set; }
        public bool UseTagManager { get; set; }
        public bool TrackOnAdmin { get; set; }
        public bool TrackOnFrontEnd { get; set; }
        public bool TrackGTMOnAdmin { get; set; }   
        public bool TrackGTMOnFrontEnd { get; set; }  
        public string DomainName { get; set; }
        public bool AnonymizeIp { get; set; }
        public CookieType CookieLevel { get; set; }
    }
}