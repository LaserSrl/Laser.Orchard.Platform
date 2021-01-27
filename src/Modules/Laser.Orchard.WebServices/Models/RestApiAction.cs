using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

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
        // TODO: consider extenting this with further properties.
        // For example, we may want to do something other than invoking
        // a workflow to handle a call.
    }
}