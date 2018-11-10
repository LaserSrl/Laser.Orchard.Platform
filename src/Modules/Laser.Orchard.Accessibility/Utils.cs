using Orchard;
using Orchard.Environment.Configuration;
using System;
using System.Web;

namespace Laser.Orchard.Accessibility
{
    public class Utils
    {
        public const string AccessibilityCookieName = "Accessibility";
        public const string AccessibilityTextOnly = "textonly";
        public const string AccessibilityNormal = "normal";
        public const string AccessibilityHighContrast = "highcontrast";

        /// <summary>
        /// Get the value of the cookie with the specified name.
        /// If there is more than one cookie with that name, the first one is returned.
        /// Usually, this is the more specialized cookie in terms of domain and path.
        /// This is useful if you want a tenant-specific cookie.
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public string GetTenantCookieValue(string cookieName, HttpRequestBase request)
        {
            string result = "";
            HttpCookie cookie = null;

            // cicla perché potrebbero esserci più cookie con lo stesso nome e path differente
            for (int i = 0; i < request.Cookies.Count; i++)
            {
                cookie = request.Cookies[i];
                if (cookie.Name == cookieName)
                {
                    result = cookie.Value;
                    // esce dopo aver trovato il primo cookie col nome cercato perché dovrebbe
                    // essere quello più specifico come dominio e path, quindi specifico del tenant
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Get the base URL of the current tenant.
        /// </summary>
        /// <param name="shellSettings"></param>
        /// <returns></returns>
        public string GetTenantBaseUrl(ShellSettings shellSettings)
        {
            // calcola il path di base del tenant corrente
            string path = "/"; 
            string tenantPath = shellSettings.RequestUrlPrefix ?? "";
            string operation = HttpContext.Current.Request.QueryString.ToString();   //_orchardServices.WorkContext.HttpContext.Request.QueryString.ToString();
            string appPath = HttpContext.Current.Request.ApplicationPath; // _orchardServices.WorkContext.HttpContext.Request.ApplicationPath;

            if (tenantPath == "")
            {
                path = appPath;
            }
            else
            {
                appPath = (appPath.EndsWith("/")) ? appPath : appPath + "/";
                path = appPath + tenantPath;
            }

            return path;
        }
    }
}