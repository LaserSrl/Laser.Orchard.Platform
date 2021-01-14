using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Models {
    public class BearerTokenAuthRequest {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }
        [JsonProperty("api_secret")]
        public string ApiSecret { get; set; }
    }
}