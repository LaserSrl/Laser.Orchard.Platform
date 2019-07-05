using Orchard;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.OutputCache.Services;
using System;
using System.Web;

namespace Laser.Orchard.Accessibility.Services
{
    public class AccessibilityServices : IAccessibilityServices
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly ISignals _signals;
        private readonly ICacheService _cacheService;
        public ILogger Logger { get; set; }

        public AccessibilityServices(IOrchardServices orchardServices, ShellSettings shellSettings, ISignals signals, ICacheService cacheService)
        {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
            _signals = signals;
            _cacheService = cacheService;
            Logger = NullLogger.Instance;
        }

        private void setCookie(string cookieValue)
        {
            // calcola il path corretto per il cookie
            string path = new Utils().GetTenantBaseUrl(_shellSettings);

            // setta il cookie
            HttpCookie cook = new HttpCookie(Utils.AccessibilityCookieName);
            cook.Path = path;
            cook.Value = cookieValue;
            if (cookieValue == "")
            {
                // elimina il cookie
                cook.Expires = DateTime.UtcNow.AddMonths(-1);
            }
            else
            {
                cook.Expires = DateTime.UtcNow.AddMonths(1);
            }
            if(_orchardServices.WorkContext.HttpContext.Response.HeadersWritten == false) {
                _orchardServices.WorkContext.HttpContext.Response.SetCookie(cook);
            }
        }

        public void SetTextOnly()
        {
            setCookie(Utils.AccessibilityTextOnly);
        }

        public void SetNormal()
        {
            setCookie(Utils.AccessibilityNormal);
        }

        public void SetHighContrast()
        {
            setCookie(Utils.AccessibilityHighContrast);
        }
    }
}