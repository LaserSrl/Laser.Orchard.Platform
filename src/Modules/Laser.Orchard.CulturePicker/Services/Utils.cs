using System;
using System.Reflection;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.CulturePicker.Services {
    public static class Utils {
        //returns url based on the current http request
        //takes into account, that url can contain application path

        public static string GetReturnUrl(HttpRequestBase request, string prefix) {
            if (request.UrlReferrer == null) {
                return String.Empty;
            }
            string localUrl = GetAppRelativePath(request.UrlReferrer.AbsolutePath, request);
            if (!String.IsNullOrWhiteSpace(prefix)) {
                localUrl = localUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? localUrl.Substring(prefix.Length) : localUrl;
            }
            localUrl = localUrl.StartsWith("/") ? localUrl.Substring(1) : localUrl;
            localUrl = HttpUtility.UrlDecode(localUrl);
            //support for pre-encoded Unicode urls
            return HttpUtility.UrlDecode(localUrl);
        }

        public static string GetCleanUrl(HttpRequestBase request, string path, string prefix) {
            string localUrl = GetAppRelativePath(path, request);
            if (!String.IsNullOrWhiteSpace(prefix)) {
                localUrl = localUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) 
                    ? localUrl.Substring(prefix.Length) : localUrl;
            }
            localUrl = localUrl.StartsWith("/") ? localUrl.Substring(1) : localUrl;
            localUrl = HttpUtility.UrlDecode(localUrl);
            //support for pre-encoded Unicode urls
            return HttpUtility.UrlDecode(localUrl);
        }

        //Translates an ASP.NET path like /myapp/subdir/page.aspx 
        //into an application relative path: subdir/page.aspx. The
        //path returned is based of the application base and 

        public static string GetAppRelativePath(string logicalPath, HttpRequestBase request) {
            if (request.ApplicationPath == null) {
                return String.Empty;
            }

            logicalPath = logicalPath.ToLower();
            string appPath = request.ApplicationPath.ToLower();
            if (appPath != "/") {
                appPath += "/";
            }
            else {
                // Root web relative path is empty
                return logicalPath.StartsWith("/") ? logicalPath.Substring(1) : logicalPath;
            }

            return logicalPath.Replace(appPath, "");
        }

        public static Version GetOrchardVersion() {
            return new AssemblyName(typeof (ContentItem).Assembly.FullName).Version;
        }
    }
}