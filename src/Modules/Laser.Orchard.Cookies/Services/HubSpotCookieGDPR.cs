﻿using Laser.Orchard.Cookies.Services;
using System.Collections.Generic;
using Laser.Orchard.Cookies;
using Orchard.Environment.Extensions;
using Orchard;
using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement;

namespace Laser.Orchard.HubSpot.Services {
    [OrchardFeature("Laser.Orchard.HubSpot")]
    public class HubSpotCookieGDPR : ICookieGDPR {

        private readonly IOrchardServices _orchardServices;
        private readonly ICookieManagerProviderService _cookieManagerProviderService;
        public HubSpotCookieGDPR(
            IOrchardServices orchardServices,
            ICookieManagerProviderService cookieManagerProviderService)
        {
            _orchardServices = orchardServices;
            _cookieManagerProviderService = cookieManagerProviderService;
        }

        public string GetCookieName()
        {
            return "HubSpot";
        }

        public IList<CookieType> GetCookieTypes()
        {
            return new List<CookieType>() { CookieType.Marketing };
        }

        public string GetFootScript(IList<CookieType> allowedTypes)
        {
            // Check if required cookies have been accepted by user.
            var acceptedCookies = _cookieManagerProviderService.GetAcceptedCookieTypes();
            foreach (var c in GetCookieTypes()) {
                if (!acceptedCookies.Contains(c)) {
                    return string.Empty;
                }
            }

            //string result = "<!-- Start of HubSpot Embed Code -->\r\n<script type=\"text/javascript\" id=\"hs - script - loader\" async defer src=\"//js.hs-scripts.com/2170079.js\"></script>\r\n<!-- End of HubSpot Embed Code -->";
            string result = string.Empty;

            var appIdHubSpot = _orchardServices.WorkContext.CurrentSite.As<HubSpotSettingsPart>().HubSpotKey;
            if (!string.IsNullOrEmpty(appIdHubSpot)) {
                result = "<!-- Start of HubSpot Embed Code -->\r\n<script type=\"text/javascript\" id=\"hs - script - loader\" async defer src=\"//js.hs-scripts.com/"+ appIdHubSpot + ".js\"></script>\r\n<!-- End of HubSpot Embed Code -->";
            }

            return result;
        }

        public string GetHeadScript(IList<CookieType> allowedTypes) {
            return string.Empty;
        }
    }
}