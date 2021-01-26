using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.WebServices.Models {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    public class RestApiAction {
        public RestApiAction() {
            Verbs = new List<string>();
        }
        [JsonProperty(
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Name { get; set; }
        [JsonProperty(
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Verbs { get; set; }
    }
}