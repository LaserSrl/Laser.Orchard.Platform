using Orchard;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Accessibility
{
    public class AccessibilityFeature : IFeatureEventHandler
    {
        private readonly IOrchardServices _services;
        private readonly ISignals _signals;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;

        public AccessibilityFeature(IOrchardServices services, ISignals signals, IOutputCacheStorageProvider cacheStorageProvider)
        {
            _services = services;
            _signals = signals;
            _cacheStorageProvider = cacheStorageProvider;
        }

        public void Disabled(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            if (feature.Descriptor.Name == "Laser.Orchard.Accessibility")
            {
                // imposta la cache in modo che non tenga più conto del cookie "Accessibility"
                var cacheSettings = _services.WorkContext.CurrentSite.ContentItem.Parts.OfType<CacheSettingsPart>().First();

                if (cacheSettings != null)
                {
                    string vary = cacheSettings.VaryByRequestCookies ?? "";
                    List<string> cookieList = vary.Split(',').ToList();
                    
                    if (cookieList.Contains(Utils.AccessibilityCookieName))
                    {
                        cookieList.Remove(Utils.AccessibilityCookieName);
                        vary = string.Join(",", cookieList);
                        cacheSettings.VaryByRequestCookies = vary;
                        _signals.Trigger(CacheSettings.CacheKey);
                    }
                }

                // svuota la cache
                _cacheStorageProvider.RemoveAll();
            }
        }

        public void Disabling(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Enabled(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            if (feature.Descriptor.Name == "Laser.Orchard.Accessibility")
            {
                // imposta la cache in modo che tenga conto del cookie "Accessibility"
                var cacheSettings = _services.WorkContext.CurrentSite.ContentItem.Parts.OfType<CacheSettingsPart>().First();

                if (cacheSettings != null)
                {
                    string vary = cacheSettings.VaryByRequestCookies ?? "";
                    char[] separators = { ',' };
                    List<string> cookieList = vary.Split(separators, System.StringSplitOptions.RemoveEmptyEntries).ToList();

                    if (cookieList.Contains(Utils.AccessibilityCookieName) == false)
                    {
                        cookieList.Add(Utils.AccessibilityCookieName);
                        vary = string.Join(",", cookieList);
                        cacheSettings.VaryByRequestCookies = vary;
                        _signals.Trigger(CacheSettings.CacheKey);
                    }
                }

                // svuota la cache
                _cacheStorageProvider.RemoveAll();
            }
        }

        public void Enabling(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Installed(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Installing(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Uninstalled(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }

        public void Uninstalling(global::Orchard.Environment.Extensions.Models.Feature feature)
        {
            //throw new NotImplementedException();
        }
    }
}