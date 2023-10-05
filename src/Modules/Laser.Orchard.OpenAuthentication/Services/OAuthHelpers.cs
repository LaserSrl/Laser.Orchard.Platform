using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services {
    public static class OAuthHelpers {
        public static Uri BuildUri(string baseUri, NameValueCollection queryParameters) {
            var q = System.Web.HttpUtility.ParseQueryString(string.Empty);
            q.Add(queryParameters);
            var builder = new UriBuilder(baseUri) { Query = q.ToString() };
            return builder.Uri;
        }
    }
}