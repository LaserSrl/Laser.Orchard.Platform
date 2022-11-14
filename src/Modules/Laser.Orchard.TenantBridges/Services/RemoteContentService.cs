using Laser.Orchard.TenantBridges.Models;
using Orchard.Logging;
using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Laser.Orchard.TenantBridges.Services {
    public class RemoteContentService : IRemoteContentService {

        public RemoteContentService() {

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public string GetSnippet(RemoteTenantContentSnippetWidgetPart part) {
            // compute the full path we'll need to call
            var baseUrl = part.RemoteTenantBaseUrl.Trim().Trim('/').Trim();
            if (string.IsNullOrWhiteSpace(baseUrl)) {
                return string.Empty;
            }

            var fullUrl = $"{baseUrl}/{Constants.GetSnippetUrl}/{part.RemoteContentId}" +
                $"?wrappers={part.RemoveRemoteWrappers.ToString().ToLower()}";

            var wr = HttpWebRequest.CreateHttp(fullUrl);
            wr.Method = WebRequestMethods.Http.Get;
#if DEBUG
            // when debugging, also accept selfsigned certificates
            wr.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
#endif

            try {
                using (var resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        using (var reader = new StreamReader(resp.GetResponseStream())) {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex) {
                var resp = (HttpWebResponse)ex.Response;
                Logger.Error("TenantBridge: WebException calling GET on {0}. Response: {1}. Exception: {2}.",
                    fullUrl, resp, ex.Message);
                using (var reader = new StreamReader(resp.GetResponseStream())) {
                    // TODO: should we have a setting in the part to choose what to do here?
                    // we still return the html: the controller of the other tenant will have
                    // responded with a 404 or something like that
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex) {
                Logger.Error("TenantBridge: Exception calling GET on {0}. Response: {1}. Exception: {2}.",
                    fullUrl, ex.Message);
                // can't recover from this.
            }

            return string.Empty;
        }
    }
}