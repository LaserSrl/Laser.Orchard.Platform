using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CulturePicker.Services {
    public class LocalizableRouteContext {
        private NameValueCollection _querystringToLocalizeCollection;
        private NameValueCollection _querystringLocalizedCollection;
        public LocalizableRouteContext(string urlToLocalize, string querystringToLocalize, string culture) {
            QuerystringToLocalizeCollection = HttpUtility.ParseQueryString(querystringToLocalize);
            UrlToLocalize = urlToLocalize;
            Culture = culture;
        }

        public NameValueCollection QuerystringToLocalizeCollection {
            get { return _querystringToLocalizeCollection; }
            set {
                _querystringToLocalizeCollection = value;
            }
        }
        public NameValueCollection QuerystringLocalizedCollection {
            get { return _querystringLocalizedCollection; }
            set {
                if (_querystringLocalizedCollection == null) {
                    _querystringLocalizedCollection = new NameValueCollection();

                }
                _querystringLocalizedCollection = value;
            }
        }
        public string QuerystringToLocalize {
            get {
                if (_querystringToLocalizeCollection == null) { return ""; }
                return SanitizeQuerystring(string.Join("&", _querystringToLocalizeCollection.AllKeys.Where(key => !string.IsNullOrWhiteSpace(_querystringToLocalizeCollection[key])).Select(key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(_querystringToLocalizeCollection[key])))));
            }
        }
        public string UrlToLocalize { get; set; }
        public string Culture { get; set; }
        public string UrlLocalized { get; set; }
        public string QuerystringLocalized {
            get {
                if (_querystringLocalizedCollection == null) { return ""; }
                return SanitizeQuerystring(string.Join("&", _querystringLocalizedCollection.AllKeys.Where(key => !string.IsNullOrWhiteSpace(_querystringLocalizedCollection[key])).Select(key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(_querystringLocalizedCollection[key])))));
            }
        }
        public string RedirectLocalUrl {
            get {
                UrlLocalized = UrlLocalized ?? UrlToLocalize;
                var querystring = QuerystringLocalized ?? QuerystringToLocalize;

                return string.Format("~/{0}{1}", UrlLocalized, !string.IsNullOrWhiteSpace(querystring) ? "?" + querystring : "");
            }
        }
        private static string SanitizeQuerystring(string querystring) {
            var sanitized = querystring;
            if (!string.IsNullOrWhiteSpace(querystring)) {
                sanitized = querystring.StartsWith("?") ? querystring.Substring(1) : querystring; //removes "?"
            }
            return sanitized;
        }

    }
}