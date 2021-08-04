using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    /// <summary>
    /// These classes parse the json template of the body and set its parameters before sending it to the create / update product api.
    /// </summary>
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookServiceJsonContext {
        string method { get; set; }
        string retailer_id { get; set; }
        FacebookServiceJsonContextData data { get; set; }
    }

    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookServiceJsonContextData {
        string name { get; set; }
        string description { get; set; }
        string availability { get; set; }
        string condition { get; set; }
        string price { get; set; }
        string currency { get; set; }
        string url { get; set; }
        string image_url { get; set; }
        string brand { get; set; }
    }
}