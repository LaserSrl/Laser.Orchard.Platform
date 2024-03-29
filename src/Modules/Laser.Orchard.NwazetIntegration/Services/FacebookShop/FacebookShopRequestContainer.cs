﻿using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopRequestContainer {
        /// <summary>
        /// In the standard scenario (standard e-commerce), ItemType isn't needed.
        /// It is needed when selling services (like hotel rooms).
        /// Possible values are: PRODUCT_ITEM, HOTEL, HOTEL_ROOM, FLIGHT, DESTINATION, HOME_LISTING, VEHICLE.
        /// </summary>
        [JsonProperty("item_type", NullValueHandling = NullValueHandling.Ignore)]
        public string ItemType { get; set; }
        [JsonProperty("requests")]
        public List<IFacebookShopRequest> Requests { get; set; }

        public FacebookShopRequestContainer() {
            Requests = new List<IFacebookShopRequest>();
        }

        public string ToJson() {
            HtmlDecode();
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Decodes every string to avoid html encoded characters (&quot;, &amp;, &egrave, ...).
        /// </summary>
        private void HtmlDecode() {
            foreach (var r in Requests) {
                r.HtmlDecode();
            }
        }
    }
}