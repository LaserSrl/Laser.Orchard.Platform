using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.Services;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.StartupConfig.MediaExtensions")]
    public class oEmbedProvidersService : IOEmbedService {
        public XDocument DownloadMediaData(string url) {
            var webClient = new WebClient { Encoding = Encoding.UTF8 };
            XDocument doc = null;
            try {
                var source = "";

                // Get the proper uri the provider redirects to
                var uri = GetRedirectUri(url);

                // Vimeo doesn't consent anymore the scraping of web pages, so the direct api call has to be enforced.
                // In this case, the downloaded string is already the expected xml, in the format that needs to be parsed.
                // Legacy process is done for non-Vimeo content.
                // First of all, url domain is checked.
                var vimeo = uri.Host.Equals("vimeo.com", StringComparison.OrdinalIgnoreCase);

                // Youtube changed the markup of the page of its videos, so the direct api call has to be enforced
                // Api url is built based on the requested video
                var youtube = uri.Host.Equals("www.youtube.com", StringComparison.OrdinalIgnoreCase);

                if (vimeo) {
                    // Add api url to original url provided as a parameter
                    url = "https://" + uri.Host + "/api/oembed.xml?url=" + url;
                    source = webClient.DownloadString(url);

                    doc = XDocument.Parse(source);
                } else if (youtube) {
                    // Add api url to original url provided as a parameter
                    url = "https://" + uri.Host + "/oembed?format=xml&url=" + url;
                    source = webClient.DownloadString(url);

                    doc = XDocument.Parse(source);
                } else {
                    // Standard (legacy) oembed import
                    source = webClient.DownloadString(url);

                    // seek type="text/xml+oembed" or application/xml+oembed
                    var oembedSignature = source.IndexOf("type=\"text/xml+oembed\"", StringComparison.OrdinalIgnoreCase);
                    if (oembedSignature == -1) {
                        oembedSignature = source.IndexOf("type=\"application/xml+oembed\"", StringComparison.OrdinalIgnoreCase);
                    }
                    if (oembedSignature != -1) {
                        var tagStart = source.Substring(0, oembedSignature).LastIndexOf('<');
                        var tagEnd = source.IndexOf('>', oembedSignature);
                        var tag = source.Substring(tagStart, tagEnd - tagStart);
                        var matches = new Regex("href=\"([^\"]+)\"").Matches(tag);
                        if (matches.Count > 0) {
                            var href = matches[0].Groups[1].Value;
                            try {
                                var content = webClient.DownloadString(HttpUtility.HtmlDecode(href));
                                doc = XDocument.Parse(content);
                            } catch {
                                // bubble exception
                            }
                        }
                    }
                }
            } catch {
                doc = null;
            }

            if (doc == null) {
                doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("oembed")
                    );
            }

            return doc;
        }

        private Uri GetRedirectUri(string url) {
            Uri myUri = new Uri(url);
            // Create a 'HttpWebRequest' object for the specified url.
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(myUri);
            // Send the request and wait for response.
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            if (!myUri.Equals(myHttpWebResponse.ResponseUri)) {
                myUri = myHttpWebResponse.ResponseUri;
            }

            // Release resources of response object.
            myHttpWebResponse.Close();

            return myUri;
        }
    }
}