using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class AddressConfigurationSettingsService : IAddressConfigurationSettingsService {

        private readonly ISiteService _siteService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;

        public AddressConfigurationSettingsService(
            ISiteService siteService,
            ICacheManager cacheManager,
            ISignals signals,
            IContentManager contentManager,
            ILocalizationService localizationService,
            ITerritoriesRepositoryService territoriesRepositoryService) {

            _siteService = siteService;
            _cacheManager = cacheManager;
            _signals = signals;
            _contentManager = contentManager;
            _localizationService = localizationService;
            _territoriesRepositoryService = territoriesRepositoryService;
        }

        private const string _settingsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.Settings";
        private const string _hierarchyCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.ShippingCountriesHierarchy";
        private const string _hierarchiesCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.ShippingCountriesHierarchies";
        private const string _territoryNamesCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedTerritoryNames";
        private const string _territoryRecordsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedTerritoryRecords";
        
        private T GetFromCache<T>(string cacheKey, Func<T> method) {
            return _cacheManager.Get(cacheKey, true, ctx => {
                // invalidation signal 
                ctx.Monitor(_signals.When(Constants.CacheEvictSignal));
                // cache
                return method();
            });
        }

        private AddressConfigurationSiteSettingsPart Settings {
            get {
                return GetFromCache(_settingsCacheKey, () => {
                    return _siteService.GetSiteSettings()
                        .As<AddressConfigurationSiteSettingsPart>();
                });
            }
        }

        public TerritoryHierarchyPart ShippingCountriesHierarchy {
            get {
                return GetFromCache(_hierarchyCacheKey, () => {
                    return Settings != null && Settings.ShippingCountriesHierarchyId > 0
                        ? _contentManager.Get<TerritoryHierarchyPart>(Settings.ShippingCountriesHierarchyId)
                        : null;
                });
            }
        }

        public IEnumerable<TerritoryHierarchyPart> ShippingCountriesHierarchies {
            get {
                return GetFromCache(_hierarchiesCacheKey, () => {
                    if (ShippingCountriesHierarchy != null) {
                        var myList = ShippingCountriesHierarchy.Is<LocalizationPart>()
                            ? _localizationService
                                .GetLocalizations(ShippingCountriesHierarchy) // may fail if the
                                ?.Select(lp => lp.As<TerritoryHierarchyPart>())
                                ?.ToList()
                            : new List<TerritoryHierarchyPart>();
                        // GetLocalizations is not prescribed to include the item whose localizations
                        // we are querying for (the default implementation does not)
                        myList.Add(ShippingCountriesHierarchy);
                        return myList;
                    }
                    return Enumerable.Empty<TerritoryHierarchyPart>();
                });
            }
        }

        public string[] SelectedTerritoryNames {
            get {
                return GetFromCache(_territoryNamesCacheKey, () => {
                    return Settings != null 
                        ? Settings.SelectedTerritories
                        : new string[] { };
                });
            }
        }

        public IEnumerable<TerritoryInternalRecord> SelectedTerritoryRecords {
            get {
                return GetFromCache(_territoryRecordsCacheKey,() => {
                    return _territoriesRepositoryService
                        .GetTerritories(SelectedTerritoryNames);
                });
            }
        }
    }
}
