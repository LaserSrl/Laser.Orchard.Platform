using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenApiResponseObject {
        public BearerTokenApiResponseObject() { }
        [JsonProperty("result")]
        public string Result { get; set; }
        [JsonProperty("result_message")]
        public string ResultMessage { get; set; }
    }

    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenCreatedRespose : BearerTokenApiResponseObject {
        public BearerTokenCreatedRespose() : base() {
            Result = "OK";
            ResultMessage = "operazione completata con successo";
        }
        [JsonProperty("token")]
        public string Token { get; set; }

    }
}