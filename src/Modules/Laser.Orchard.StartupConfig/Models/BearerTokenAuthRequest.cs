using Newtonsoft.Json;

namespace Laser.Orchard.StartupConfig.Models {
    public class BearerTokenAuthRequest {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }
        [JsonProperty("api_secret")]
        public string ApiSecret { get; set; }
        /*
         Expected body for authentication looks like this:
         {
            "api_key": "mykeyvalue",
            "api_secret": "mysecretvalue"
         }
         Should we put in place a way to have more forms of it? Meaning, something where we could
         have different property names all mapping to our ApiKey and ApiSecret.
         */
    }
}