﻿using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Cookies.Models {
    [OrchardFeature("Laser.Orchard.HubSpot")]
    public class HubSpotSettingsPart : ContentPart {
        /// <summary>
        /// Get or Set HubSpot Key
        /// </summary>
        public string HubSpotKey {
            get {
                return this.Retrieve(x => x.HubSpotKey);
            }
            set { this.Store(x => x.HubSpotKey, value); }
        }
    }
}