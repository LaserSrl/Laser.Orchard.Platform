using Newtonsoft.Json;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenApiResponseObject {
        public BearerTokenApiResponseObject() { }
        [JsonProperty("result")]
        public string Result { get; set; }
        [JsonProperty("result_message")]
        public string ResultMessage { get; set; }
        /*
         Expected response for authentication looks like this:
         {
            "result": "OK", // NOT_OK
            "result_message": "done"
         }
         Should we put in place a way to have more forms of it? Meaning, something where we could
         have different property names all mapping to our Result and ResultMessage? How would we even 
         choose which one to send?
         */
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