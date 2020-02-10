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

        TerritoryHierarchyPart ConfiguredHierarchy =>
            _settingsService.GetConfiguredHierarchy();

        public TerritoryPart GetCountry(int internalId) {
            if (_settingsService.SelectedCountryIds.Contains(internalId)) {
                return SingleTeritory(internalId);
            }
            return null;
        }

        public TerritoryPart GetCity(int internalId) {
            if (_settingsService.SelectedCityIds.Contains(internalId)) {
                return SingleTeritory(internalId);
            }
            return null;
        }

        private TerritoryPart SingleTeritory(int internalId) {
            return _territoriesService.GetTerritoriesQuery(ConfiguredHierarchy)
                .Where(tpr => tpr.TerritoryInternalRecord.Id == internalId)
                .Slice(0, 1)
                .FirstOrDefault();
        }

        public IEnumerable<TerritoryPart> GetAllCountries() {

            return _territoriesService.GetTerritoriesQuery(ConfiguredHierarchy)
                .Where(tpr => _settingsService.SelectedCountryIds.Contains(tpr.TerritoryInternalRecord.Id))
                .List();
        }

        public IEnumerable<TerritoryPart> GetAllProvinces() {

            return _territoriesService.GetTerritoriesQuery(ConfiguredHierarchy)
                .Where(tpr => _settingsService.SelectedProvinceIds.Contains(tpr.TerritoryInternalRecord.Id))
                .List();
        }

        public IEnumerable<TerritoryPart> GetAllProvinces(
            TerritoryPart country, TerritoryPart city) {
            var allProvinces = GetAllProvinces(country);
            if (city == null) {
                return allProvinces;
            }
            // only provinces that contain the city
            return allProvinces
                .Where(pr => GetChildren(pr, _settingsService.SelectedCityIds)
                    .Any(tp => tp.Record.TerritoryInternalRecord.Id == city.Record.TerritoryInternalRecord.Id));
        }

        public IEnumerable<TerritoryPart> GetAllProvinces(TerritoryPart country) {
            var root = country;
            // make sure root we'll use belongs to hierarchy
            if (country.HierarchyPart.Id != ConfiguredHierarchy.Id) {
                root = SingleTeritory(country.Record.TerritoryInternalRecord.Id);
            }
            if (root == null) {
                // if the root is not valid for the hierarchy, we cannot return 
                // any province.
                return Enumerable.Empty<TerritoryPart>();
            }

            return GetChildren(root, _settingsService.SelectedProvinceIds);
        }

        public IEnumerable<TerritoryPart> GetAllCities() {

            return _territoriesService.GetTerritoriesQuery(ConfiguredHierarchy)
                .Where(tpr => _settingsService.SelectedCityIds.Contains(tpr.TerritoryInternalRecord.Id))
                .List();
        }
        
        public IEnumerable<TerritoryPart> GetAllCities(TerritoryPart parent) {
            var root = parent;
            // make sure root we'll use belongs to hierarchy
            if (parent.HierarchyPart.Id != ConfiguredHierarchy.Id) {
                root = SingleTeritory(parent.Record.TerritoryInternalRecord.Id);
            }
            if (root == null) {
                // if the root is not valid for the hierarchy, we cannot return 
                // any province.
                return Enumerable.Empty<TerritoryPart>();
            }
            // usually parent will be either a country or a province, but it doesn't
            // really affect our code here.
            return GetChildren(root, _settingsService.SelectedCityIds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="selection">This is an array of int that represents a list of Ids
        /// for selected territories.</param>
        /// <returns></returns>
        private IEnumerable<TerritoryPart> GetChildren(TerritoryPart parent, int[] selection) {
            // we need root's children of all levels within hierarchy
            // that correspond to the territories that have been selected
            // for provinces.
            // Depth first recursion
            var allRecords = GetAllChildrenRecords(parent.Record.Children);
            var selectedRecords = allRecords
                .Where(tpr => selection.Contains(tpr.TerritoryInternalRecord.Id));
            return _contentManager
                .GetMany<TerritoryPart>(
                    selectedRecords.Select(r => r.Id),
                    // Consider eventually using the version from the hierarchy?
                    VersionOptions.Published,
                    QueryHints.Empty);
        }

        private IEnumerable<TerritoryPartRecord> GetAllChildrenRecords(
            IEnumerable<TerritoryPartRecord> records) {
            var result = new List<TerritoryPartRecord>(records);
            if (records.Any()) {
                // if there are children, add those as well as their children
                result.AddRange(GetAllChildrenRecords(records.SelectMany(r => r.Children)));
            }
            return result;
        }

    }
}