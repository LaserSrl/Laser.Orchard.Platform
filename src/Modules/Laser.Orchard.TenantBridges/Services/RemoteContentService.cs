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

            // TODO: Limit the type of content this call can retrieve:
            // right now every id can be requested, consider the idea of returning a valid 
            // snippet only if a widget is requested and return an exception/generic message/whatever in every other case


            // compute the full path we'll need to call
            var baseUrl = part.RemoteTenantBaseUrl.Trim().Trim('/').Trim();
            if (string.IsNullOrWhiteSpace(baseUrl)) {
                return string.Empty;
            }

            var fullUrl = $"{baseUrl}/{Constants.GetSnippetUrl}/{part.RemoteContentId}" +
                $"?wrappers={(!part.RemoveRemoteWrappers).ToString().ToLower()}";

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

        string IRemoteContentService.GetJson(RemoteTenantContentSnippetWidgetPart part) {

            // TODO: In some cases the current tenant must be authenticated in the remote tenant
            // and the following call should ideally be identical to the ones made by the mobile library.
            // That means this call must know the ApiKey, ApiChannel and encryption key of the remote tenant.
            // A possibile solution could be to create a new content called RemoteTenant where we can specify these values 
            // and change the remote content snippet widget so that one of its fields is the RemoteTenant to get the data from, but this has to be discussed together.
        

            // compute the full path we'll need to call
            var baseUrl = part.RemoteTenantBaseUrl.Trim().Trim('/').Trim();
            if (string.IsNullOrWhiteSpace(baseUrl)) {
                return string.Empty;
            }

           
            var fullUrl = $"{baseUrl}/{Constants.GetJsonUrl}" +
                $"?alias={(part.Alias).ToString().ToLower()}";

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
                string error = string.Format("TenantBridge: WebException calling GET on {0}. Response: {1}. Exception: {2}.",
                    fullUrl, resp, ex.Message);
              
                if (resp.StatusCode == HttpStatusCode.NotFound) {
                    error += Environment.NewLine + "The Laser.WebService feature on the remote tab may not be enabled";
                }

                Logger.Error(error);

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
