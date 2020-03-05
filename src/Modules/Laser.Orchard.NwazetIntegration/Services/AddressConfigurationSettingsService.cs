using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class AddressConfigurationSettingsService : IAddressConfigurationSettingsService {

        private readonly ISiteService _siteService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public AddressConfigurationSettingsService(
            ISiteService siteService,
            ICacheManager cacheManager,
            ISignals signals,
            IContentManager contentManager,
            ILocalizationService localizationService,
            ITerritoriesRepositoryService territoriesRepositoryService,
            IWorkContextAccessor workContextAccessor) {

            _siteService = siteService;
            _cacheManager = cacheManager;
            _signals = signals;
            _contentManager = contentManager;
            _localizationService = localizationService;
            _territoriesRepositoryService = territoriesRepositoryService;
            _workContextAccessor = workContextAccessor;
        }

        public TerritoryHierarchyPart GetConfiguredHierarchy(CultureInfo culture) {
            return GetConfiguredHierarchy(culture.Name);
        }
        public TerritoryHierarchyPart GetConfiguredHierarchy(string culture = "") {
            if (string.IsNullOrWhiteSpace(culture)) {
                culture = _workContextAccessor.GetContext().CurrentCulture;
            }
            return GetFromCache(_hierarchyForCultureBaseCacheKey + culture, () => {
                var localizedHierarchy = ShippingCountriesHierarchies
                    .FirstOrDefault(thp =>
                        // localizable
                        thp.Is<LocalizationPart>()
                        // desired culture
                        && thp.As<LocalizationPart>()
                            .Culture.Culture
                            .Equals(culture, StringComparison.InvariantCultureIgnoreCase));

                return localizedHierarchy ?? ShippingCountriesHierarchy;
            });

        }

        #region cache keys
        private const string _settingsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.Settings";
        private const string _hierarchyCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.ShippingCountriesHierarchy";
        private const string _hierarchiesCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.ShippingCountriesHierarchies";
        private const string _territoryIdsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedTerritoryIds";
        private const string _territoryRecordsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedTerritoryRecords";
        private const string _countryIdsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedCountryIds";
        private const string _countryTerritoryRecordsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedCountryTerritoryRecords";
        private const string _provinceIdsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedProvinceIds";
        private const string _provinceTerritoryRecordsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedProvinceTerritoryRecords";
        private const string _cityIdsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedCityIds";
        private const string _cityTerritoryRecordsCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.SelectedCityTerritoryRecords";
        private const string _hierarchyForCultureBaseCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.GetConfiguredHierarchy.";
        private const string _countryCodesCacheKey =
            "Laser.Orchard.NwazetIntegration.Services.AddressConfigurationSettingsService.CountryISOCodes";
        #endregion

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

        public int[] SelectedTerritoryIds {
            get {
                return GetFromCache(_territoryIdsCacheKey, () => {
                    return Settings != null
                        ? SelectedCityIds
                            .Union(SelectedProvinceIds)
                            .Union(SelectedCountryIds)
                            .ToArray()
                        : new int[] { };
                });
            }
        }

        public IEnumerable<TerritoryInternalRecord> SelectedTerritoryRecords {
            get {
                return GetFromCache(_territoryRecordsCacheKey, () => {
                    return _territoriesRepositoryService
                        .GetTerritories(SelectedTerritoryIds);
                });
            }
        }

        private int[] SelectedIdsForType(TerritoryAdministrativeType adminType) {
            var hierarchyIds = ShippingCountriesHierarchies
                .Select(h => h.Record.Id)
                .ToArray();
            return _contentManager
                .Query<TerritoryAdministrativeTypePart, TerritoryAdministrativeTypePartRecord>()
                .Where(tatpr => tatpr.AdministrativeType == adminType)
                .Join<TerritoryPartRecord>()
                .Where(tpr =>
                    hierarchyIds
                        .Contains(tpr.Hierarchy.Id))
                .List()
                .Where(tp => tp.Record.TerritoryInternalRecord != null)
                .Select(tp => tp.Record.TerritoryInternalRecord.Id)
                .Distinct()
                .ToArray();
        }

        public int[] SelectedCountryIds {
            get {
                return GetFromCache(_countryIdsCacheKey, () => {
                    if (ShippingCountriesHierarchies.Any()) {
                        return SelectedIdsForType(TerritoryAdministrativeType.Country);
                    }
                    return new int[] { };
                });
            }
        }

        public IEnumerable<TerritoryInternalRecord> SelectedCountryTerritoryRecords {
            get {
                return GetFromCache(_countryTerritoryRecordsCacheKey, () => {
                    return _territoriesRepositoryService
                        .GetTerritories(SelectedCountryIds);
                });
            }
        }

        public int[] SelectedProvinceIds {
            get {
                return GetFromCache(_provinceIdsCacheKey, () => {
                    if (ShippingCountriesHierarchies.Any()) {
                        return SelectedIdsForType(TerritoryAdministrativeType.Province);
                    }
                    return new int[] { };
                });
            }
        }

        public IEnumerable<TerritoryInternalRecord> SelectedProvinceTerritoryRecords {
            get {
                return GetFromCache(_provinceTerritoryRecordsCacheKey, () => {
                    return _territoriesRepositoryService
                        .GetTerritories(SelectedProvinceIds);
                });
            }
        }

        public int[] SelectedCityIds {
            get {
                return GetFromCache(_cityIdsCacheKey, () => {
                    if (ShippingCountriesHierarchies.Any()) {
                        return SelectedIdsForType(TerritoryAdministrativeType.City);
                    }
                    return new int[] { };
                });
            }
        }

        public IEnumerable<TerritoryInternalRecord> SelectedCityTerritoryRecords {
            get {
                return GetFromCache(_cityTerritoryRecordsCacheKey, () => {
                    return _territoriesRepositoryService
                        .GetTerritories(SelectedCityIds);
                });
            }
        }

        //public IEnumerable<CountryAlpha2> CountryISOCodes {
        //    get {
        //        return GetFromCache(_countryCodesCacheKey, () => {
        //            return Settings != null
        //                ? Settings.CountryCodes
        //                : new CountryAlpha2[] { };
        //        });
        //    }
        //}

        //public string GetCountryISOCode(int id) {
        //    return CountryISOCodes
        //        .FirstOrDefault(cc => cc.TerritoryId == id)
        //        .ISOCode ?? string.Empty;
        //}
    }
}
