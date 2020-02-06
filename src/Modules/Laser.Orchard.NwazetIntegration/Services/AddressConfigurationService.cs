using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class AddressConfigurationService : IAddressConfigurationService {
        private readonly IAddressConfigurationSettingsService _settingsService;
        private readonly ITerritoriesService _territoriesService;
        private readonly IContentManager _contentManager;

        public AddressConfigurationService(
            IAddressConfigurationSettingsService settingsService,
            ITerritoriesService territoriesService,
            IContentManager contentManager) {

            _settingsService = settingsService;
            _territoriesService = territoriesService;
            _contentManager = contentManager;
        }

        public TerritoryPart GetCountry(int internalId) {
            var hierarchy = _settingsService.GetConfiguredHierarchy();
            if (_settingsService.SelectedCountryIds.Contains(internalId)) {
                return _territoriesService.GetTerritoriesQuery(hierarchy)
                    .Where(tpr => tpr.TerritoryInternalRecord.Id == internalId)
                    .Slice(0, 1)
                    .FirstOrDefault();
            }
            return null;
        }

        public IEnumerable<TerritoryPart> GetAllCountries() {
            var hierarchy = _settingsService.GetConfiguredHierarchy();

            return _territoriesService.GetTerritoriesQuery(hierarchy)
                .Where(tpr => _settingsService.SelectedCountryIds.Contains(tpr.TerritoryInternalRecord.Id))
                .List();
        }

        public IEnumerable<TerritoryPart> GetAllProvinces() {
            var hierarchy = _settingsService.GetConfiguredHierarchy();

            return _territoriesService.GetTerritoriesQuery(hierarchy)
                .Where(tpr => _settingsService.SelectedProvinceIds.Contains(tpr.TerritoryInternalRecord.Id))
                .List();
        }

        public IEnumerable<TerritoryPart> GetAllProvinces(TerritoryPart country) {
            var hierarchy = _settingsService.GetConfiguredHierarchy();
            var root = country;
            // make sure root we'll use belongs to hierarchy
            if (country.HierarchyPart.Id != hierarchy.Id) {
                root = _territoriesService.GetTerritoriesQuery(hierarchy)
                    .Where(tpr => tpr.TerritoryInternalRecord.Id == country.Record.TerritoryInternalRecord.Id)
                    .Slice(0, 1)
                    .FirstOrDefault();
            }
            if (root == null) {
                // if the root is not valid for the hierarchy, we cannot return 
                // any province.
                return Enumerable.Empty<TerritoryPart>();
            }
            // we need root's children of all levels within hierarchy
            // that correspond to the territories that have been selected
            // for provinces.
            // Depth first recursion
            var allRecords = GetAllChildrenRecords(root.Record.Children);
            var provinceRecords = allRecords
                .Where(tpr => _settingsService
                    .SelectedProvinceIds.Contains(tpr.TerritoryInternalRecord.Id));
            return _contentManager
                .GetMany<TerritoryPart>(
                    provinceRecords.Select(r => r.Id),
                    // Consider eventually using the version from the hierarchy?
                    VersionOptions.Published,
                    QueryHints.Empty);
        }

        private IEnumerable<TerritoryPartRecord> GetAllChildrenRecords(
            IEnumerable<TerritoryPartRecord> records) {
            var result = new List<TerritoryPartRecord>(records);
            result.AddRange(GetAllChildrenRecords(records.SelectMany(r => r.Children)));
            return result;
        }

        public IEnumerable<TerritoryPart> GetAllCities() {
            var hierarchy = _settingsService.GetConfiguredHierarchy();

            return _territoriesService.GetTerritoriesQuery(hierarchy)
                .Where(tpr => _settingsService.SelectedCityIds.Contains(tpr.TerritoryInternalRecord.Id))
                .List();
        }
        
        public IEnumerable<TerritoryPart> GetAllCities(TerritoryPart parent) {
            // usualy parent will be either a country or a province
            var hierarchy = _settingsService.GetConfiguredHierarchy();

            return _territoriesService.GetTerritoriesQuery(hierarchy)
                .Where(tpr => _settingsService.SelectedCityIds.Contains(tpr.TerritoryInternalRecord.Id))
                .List();
        }
    }
}