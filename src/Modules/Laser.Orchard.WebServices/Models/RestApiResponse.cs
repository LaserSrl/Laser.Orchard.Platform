using Newtonsoft.Json;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Laser.Orchard.WebServices.Models {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    public class RestApiResponse {

        public RestApiResponse() {
            // Default to not found.
            StatusCode = HttpStatusCode.NotFound;
            Headers = new Dictionary<string, string>();
        }
        public RestApiResponse(HttpStatusCode code) : this() {
            // Default to not found.
            StatusCode = code;
        }

        public HttpStatusCode StatusCode { get; set; }
        // This will be serialized
        public object Content { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpResponseMessage ToMessage() {
            var msg = new HttpResponseMessage(StatusCode);
            if (Content != null) {
                msg.Content = new StringContent(
                    JsonConvert
                        .SerializeObject(Content)
                        .ToString());
                msg.Content.Headers.ContentType = 
                    new MediaTypeHeaderValue("application/json");
                if (Headers != null && Headers.Any()) {
                    foreach (var kvp in Headers) {
                        msg.Content.Headers.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            return msg;
        }
    }
}