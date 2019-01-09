using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CulturePicker.Services {
    public class LocalizableRouteContext {

        /// <summary>
        /// private variable to store the querystring to localize. This variable is set when the QuerystringToLocalizeCollection is set;
        /// </summary>
        private NameValueCollection _querystringToLocalizeCollection;

        /// <summary>
        /// private variable to store the querystring after a localization action. This variable is set when the QuerystringLocalizedCollection is set;
        /// </summary>
        private NameValueCollection _querystringLocalizedCollection;

        public LocalizableRouteContext(string urlToLocalize, string querystringToLocalize, string culture) {
            QuerystringToLocalizeCollection = HttpUtility.ParseQueryString(querystringToLocalize);
            UrlToLocalize = urlToLocalize;
            Culture = culture;
        }

        public bool UrlIsLocalized { get; private set; }
        public bool QuerystringIsLocalized { get; private set; }

        public NameValueCollection QuerystringToLocalizeCollection {
            get { return _querystringToLocalizeCollection; }
            set {
                _querystringToLocalizeCollection = value;
            }
        }
        /// <summary>
        /// returns the localized querystring collection if a localization provider has been fired. 
        /// </summary>
        public NameValueCollection QuerystringLocalizedCollection {
            get { return QuerystringIsLocalized ? _querystringLocalizedCollection : _querystringToLocalizeCollection; }
            set {
                QuerystringIsLocalized = true;
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
        private string _ulrlLocalized;
        public string UrlLocalized {
            get {
                return UrlIsLocalized ? _ulrlLocalized : UrlToLocalize;
            }
            set {
                UrlIsLocalized = true;
                _ulrlLocalized = value;
            }
        }
        public string QuerystringLocalized {
            get {
                if (QuerystringLocalizedCollection == null) { return ""; }
                return SanitizeQuerystring(string.Join("&", QuerystringLocalizedCollection.AllKeys.Where(key => !string.IsNullOrWhiteSpace(QuerystringLocalizedCollection[key])).Select(key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(QuerystringLocalizedCollection[key])))));
            }
        }
        public string RedirectLocalUrl {
            get {
                return string.Format("~/{0}{1}", UrlLocalized.StartsWith("~/")? UrlLocalized.Substring(2) : UrlLocalized, !string.IsNullOrWhiteSpace(QuerystringLocalized) ? "?" + QuerystringLocalized : "");
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