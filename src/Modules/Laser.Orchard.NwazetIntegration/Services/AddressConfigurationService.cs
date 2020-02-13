using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class AddressConfigurationService : IAddressConfigurationService {
        private readonly IAddressConfigurationSettingsService _settingsService;
        private readonly ITerritoriesService _territoriesService;
        private readonly IContentManager _contentManager;
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;

        public AddressConfigurationService(
            IAddressConfigurationSettingsService settingsService,
            ITerritoriesService territoriesService,
            IContentManager contentManager,
            ITerritoriesRepositoryService territoriesRepositoryService) {

            _settingsService = settingsService;
            _territoriesService = territoriesService;
            _contentManager = contentManager;
            _territoriesRepositoryService = territoriesRepositoryService;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        TerritoryHierarchyPart ConfiguredHierarchy =>
            _settingsService.GetConfiguredHierarchy();

        public TerritoryPart GetCountry(int internalId) {
            if (_settingsService.SelectedCountryIds.Contains(internalId)) {
                return SingleTerritory(internalId);
            }
            return null;
        }

        public TerritoryPart GetCountry(string name) {
            var tp = SingleTerritory(name);
            if (tp != null) {
                return _settingsService.SelectedCountryIds.Contains(tp.Record.TerritoryInternalRecord.Id)
                    ? tp // return it if it is a country
                    : null;
            }
            return null;
        }

        public TerritoryPart GetCity(int internalId) {
            if (_settingsService.SelectedCityIds.Contains(internalId)) {
                return SingleTerritory(internalId);
            }
            return null;
        }

        public TerritoryPart GetCity(string name) {
            var tp = SingleTerritory(name);
            if (tp != null) {
                return _settingsService.SelectedCityIds.Contains(tp.Record.TerritoryInternalRecord.Id)
                    ? tp // return it if it is a city
                    : null;
            }
            return null;
        }

        public TerritoryPart SingleTerritory(int internalId) {
            return _territoriesService.GetTerritoriesQuery(ConfiguredHierarchy)
                .Where(tpr => tpr.TerritoryInternalRecord.Id == internalId)
                .Slice(0, 1)
                .FirstOrDefault();
        }

        public TerritoryPart SingleTerritory(string name) {
            var internalRecord = _territoriesRepositoryService.GetTerritoryInternal(name);
            if (internalRecord != null) {
                return SingleTerritory(internalRecord.Id);
            }
            return null;
        }

        public IEnumerable<TerritoryPart> GetAllCountries() {

            return _territoriesService.GetTerritoriesQuery(ConfiguredHierarchy)
                .Where(tpr => _settingsService.SelectedCountryIds.Contains(tpr.TerritoryInternalRecord.Id))
                .List();
        }

        public List<SelectListItem> CountryOptions(int id = -1) {
            var countries = GetAllCountries();
            var options = new List<SelectListItem>();
            options.Add(new SelectListItem() {
                Value = "-1",
                Text = T("Select a country").Text,
                Disabled = true,
                Selected = id <= 0
            });
            options.AddRange(countries
                .Select(tp => new SelectListItem() {
                    Value = tp.Record.TerritoryInternalRecord.Id.ToString(),
                    Text = _contentManager.GetItemMetadata(tp).DisplayText,
                    Selected = id == tp.Record.TerritoryInternalRecord.Id
                }));
            return options;
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
                root = SingleTerritory(country.Record.TerritoryInternalRecord.Id);
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
                root = SingleTerritory(parent.Record.TerritoryInternalRecord.Id);
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