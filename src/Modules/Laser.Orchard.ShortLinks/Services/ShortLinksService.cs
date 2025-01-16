using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace Laser.Orchard.ShortLinks.Services {
    public class ShortLinksService : IShortLinksService {
        public Localizer T {get;set;}
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;

        public ShortLinksService(IOrchardServices orchardServices, INotifier notifier) {
            _orchardServices = orchardServices;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public string GetShortLink(ContentPart part) {
            var longuri = GetFullAbsoluteUrl(part);
            return GetShortLink(longuri);
        }

        public string GetShortLink(string myurl) {
            if (String.IsNullOrWhiteSpace(myurl)) return "";
            var shorturl = "";
            var setting = _orchardServices.WorkContext.CurrentSite.As<Laser.Orchard.ShortLinks.Models.ShortLinksSettingsPart>();
            if (string.IsNullOrEmpty(setting.GoogleApiKey)) {
                _notifier.Add(NotifyType.Error, T("No Shorturl Setting Found"));
            }
            else {
                // Firebase doesn't provide url shortened service anymore, so TinyUrl is now used.
                //var apiurl = string.Format("https://firebasedynamiclinks.googleapis.com/v1/shortLinks?key={0}", setting.GoogleApiKey);
                var apiurl = string.Format("https://api.tinyurl.com/create?api_token={0}", setting.GoogleApiKey);
                var request = (HttpWebRequest)WebRequest.Create(apiurl);
                //var postData = string.Format("{{\"dynamicLinkInfo\": {{\"dynamicLinkDomain\": \"{0}\",\"link\": \"{1}\"}},\"suffix\": {{\"option\": \"{2}\"}}}}", setting.DynamicLinkDomain, myurl, setting.HasSensitiveData ? @"UNGUESSABLE" : @"SHORT");
                var postData = string.Format("{{\"url\": \"{0}\", \"domain\": \"{1}\", \"description\": \"{2}\"}}", myurl, setting.DynamicLinkDomain, "string");
                request.Method = "POST";
                request.ContentType = "application/json";
                using (var stream = new StreamWriter(request.GetRequestStream())) {
                    stream.Write(postData);
                    stream.Flush();
                    stream.Close();
                }
                var response = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream())) {
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsondict = serializer.Deserialize<Dictionary<string, object>>(streamReader.ReadToEnd());
                    var responseData = jsondict["data"];
                    if (responseData != null) {
                        shorturl = ((dynamic)responseData)["tiny_url"].ToString();
                    }
                    //shorturl = jsondict["shortLink"].ToString();
                 }
            }
            return shorturl;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public string GetFullAbsoluteUrl(ContentPart part) {
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            return urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(part));
        }
    }
}