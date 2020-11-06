using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Data;
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
        private readonly IRepository<TerritoryInternalRecord> _territoryInternalRecord;
        private readonly ITerritoryPartRecordService _territoryPartRecordService;

        public AddressConfigurationService(
            IAddressConfigurationSettingsService settingsService,
            ITerritoriesService territoriesService,
            IContentManager contentManager,
            ITerritoriesRepositoryService territoriesRepositoryService,
            IRepository<TerritoryInternalRecord> territoryInternalRecord,
            ITerritoryPartRecordService territoryPartRecordService) {

            _settingsService = settingsService;
            _territoriesService = territoriesService;
            _contentManager = contentManager;
            _territoriesRepositoryService = territoriesRepositoryService;
            _territoryInternalRecord = territoryInternalRecord;
            _territoryPartRecordService = territoryPartRecordService;

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

        public TerritoryPart GetProvince(int internalId) {
            if (_settingsService.SelectedProvinceIds.Contains(internalId)) {
                return SingleTerritory(internalId);
            }
            return null;
        }

        public TerritoryPart GetProvince(string name) {
            var tp = SingleTerritory(name);
            if (tp != null) {
                return _settingsService.SelectedProvinceIds.Contains(tp.Record.TerritoryInternalRecord.Id)
                    ? tp // return it if it is a province
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

        private IContentQuery<TerritoryPart> CountriesQuery() {
            return _territoriesService
                .GetTerritoriesQuery(ConfiguredHierarchy)
                // Is country
                .Join<TerritoryAdministrativeTypePartRecord>()
                .Where(tat => tat.AdministrativeType == TerritoryAdministrativeType.Country);
        }

        public IEnumerable<TerritoryPart> GetAllCountries() {

            return CountriesQuery()
                .List();
        }

        public IEnumerable<TerritoryPart> GetAllCountries(AddressRecordType addressRecordType) {
            var query = CountriesQuery()
                // only those marked for the given record type
                .Join<TerritoryAddressTypePartRecord>();
            if (addressRecordType == AddressRecordType.ShippingAddress) {
                return query
                    .Where(tatpr => tatpr.Shipping)
                .List();
            } else {
                return query
                    .Where(tatpr => tatpr.Billing)
                .List();
            }
        }

        public IEnumerable<SelectListItem> CountryOptions(int id = -1) {
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
                }).OrderBy(sli => sli.Text));
            return options;
        }

        public IEnumerable<SelectListItem> CountryOptions(
            AddressRecordType addressRecordType, int id = -1) {
            var countries = GetAllCountries(addressRecordType);
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
                }).OrderBy(sli => sli.Text));
            return options;
        }

        private IContentQuery<TerritoryPart> ProvincesQuery() {
            return _territoriesService
                .GetTerritoriesQuery(ConfiguredHierarchy)
                // Only those marked as province
                .Join<TerritoryAdministrativeTypePartRecord>()
                .Where(tat => tat.AdministrativeType == TerritoryAdministrativeType.Province);
        }

        public IEnumerable<TerritoryPart> GetAllProvinces() {

            return ProvincesQuery()
                .List();
        }

        public IEnumerable<TerritoryPart> GetAllProvinces(
            TerritoryPart country, TerritoryPart city) {
            if (city == null) {
                return GetAllProvinces(country);
            }

            var provinces = new List<TerritoryPart>();
            var parent = city.ParentPart;
            while (parent != null) {
                var adminType = parent.As<TerritoryAdministrativeTypePart>();
                if (adminType != null && adminType.AdministrativeType == TerritoryAdministrativeType.Province) {
                    provinces.Add(parent);
                }
                parent = parent.ParentPart;
            }
            // only provinces that contain the city
            return provinces;
        }
        public IEnumerable<TerritoryPart> GetAllProvinces(
            AddressRecordType addressRecordType, TerritoryPart country, TerritoryPart city) {

            if (city == null) {
                return GetAllProvinces(addressRecordType, country);
            }

            var provinces = new List<TerritoryPart>();
            var parent = city.ParentPart;
            while (parent != null) {
                var adminType = parent.As<TerritoryAdministrativeTypePart>();
                if (adminType != null && adminType.AdministrativeType == TerritoryAdministrativeType.Province) {
                    provinces.Add(parent);
                }
                parent = parent.ParentPart;
            }
            // only provinces that contain the city
            return provinces;
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
        public IEnumerable<TerritoryPart> GetAllProvinces(
            AddressRecordType addressRecordType, TerritoryPart country) {
            var root = country;
            // make sure root we'll use belongs to hierarchy
            if (country.Record.Hierarchy.Id != ConfiguredHierarchy.Id) {
                root = SingleTerritory(country.Record.TerritoryInternalRecord.Id);
            }
            if (root == null) {
                // if the root is not valid for the hierarchy, we cannot return 
                // any province.
                return Enumerable.Empty<TerritoryPart>();
            }

            var provinceIds = _settingsService
                .SelectedProvinceIds;


            return GetSomeChildren(addressRecordType, root, provinceIds, 0, 512);

        }

        public IEnumerable<TerritoryPart> GetAllCities() {

            return _territoriesService.GetTerritoriesQuery(ConfiguredHierarchy)
                // only those marked as city
                .Join<TerritoryAdministrativeTypePartRecord>()
                .Where(tat => tat.AdministrativeType == TerritoryAdministrativeType.City)
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

        public IEnumerable<TerritoryPart> GetAllCities(
            AddressRecordType addressRecordType, TerritoryPart parent) {
            var root = parent;
            // make sure root we'll use belongs to hierarchy
            if (parent.HierarchyPart.Id != ConfiguredHierarchy.Id) {
                root = SingleTerritory(parent.Record.TerritoryInternalRecord.Id);
            }
            if (root == null) {
                // if the root is not valid for the hierarchy, we cannot return 
                // any city.
                return Enumerable.Empty<TerritoryPart>();
            }
            
            var cityIds = _settingsService
                .SelectedCityIds;

            return GetSomeChildren(addressRecordType, root, cityIds, 0, 512);
        }

        public IEnumerable<TerritoryPart> GetAllCities(
            AddressRecordType addressRecordType, TerritoryPart parent, 
            string nameQuery, int maxOptions = 20) {
            var root = parent;
            // make sure root we'll use belongs to hierarchy
            if (parent.HierarchyPart.Id != ConfiguredHierarchy.Id) {
                root = SingleTerritory(parent.Record.TerritoryInternalRecord.Id);
            }
            if (root == null) {
                // if the root is not valid for the hierarchy, we cannot return 
                // any city.
                return Enumerable.Empty<TerritoryPart>();
            }

            //var filtered = new List<TerritoryPart>();
            var cityIds = _settingsService
                .SelectedCityIdsByName(nameQuery);

            return GetSomeChildren(addressRecordType, root, cityIds, maxOptions, maxOptions);
        }
        
        private IEnumerable<TerritoryPart>  GetSomeChildren(
            AddressRecordType addressRecordType, TerritoryPart root,
            int[] childrenIds, int limitCount = 0,
            int maxOptions = 20) {

            var filtered = new List<TerritoryPart>();

            for (int i = 0; (limitCount <= 0 ? true : filtered.Count() < limitCount); i++) {
                var childrenQuery = _territoriesService
                    // Query for territories in hierarchy
                    .GetTerritoriesQuery(ConfiguredHierarchy)
                    // only those marked as city and that contain the query text
                    .Where(tpr => childrenIds.Skip(i * maxOptions).Take(maxOptions).ToArray()
                        .Contains(tpr.TerritoryInternalRecord.Id));
                IEnumerable<TerritoryPart> children;
                if (addressRecordType == AddressRecordType.ShippingAddress) {
                    children = childrenQuery
                        // only those marked for the given record type
                        .Join<TerritoryAddressTypePartRecord>()
                        .Where(tatpr => tatpr.Shipping)
                        .Slice(i * maxOptions, maxOptions);
                } else {
                    children = childrenQuery
                        // only those marked for the given record type
                        .Join<TerritoryAddressTypePartRecord>()
                        .Where(tatpr => tatpr.Billing)
                        .Slice(i * maxOptions, maxOptions);
                }

                filtered.AddRange(children
                    .Where(tp => {
                        var p = tp.ParentPart;
                        while (p != null) {
                            if (p == root) {
                                return true;
                            }
                            p = p.ParentPart;
                        }
                        return false;
                    }));
                if (children.Count() < maxOptions) {
                    // we arelady had fewer than maxOptions cities to choose from
                    break;
                }
            }
            if (limitCount <= 0) {
                return filtered;
            } else {
                return filtered.Take(limitCount);
            }
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
            // based on the Ids.
            // Depth first recursion
            var allRecords = GetAllChildrenRecords(_territoryPartRecordService.GetTerritoriesChild(parent));//parent.Record.Children
            var selectedRecords = allRecords
                .Where(tpr => selection.Contains(tpr.TerritoryInternalRecord.Id));

            // GetMany will break if there are too many ids, so we query a bunch at a time
            int tried = 0;
            var children = new List<TerritoryPart>();
            while (tried < selectedRecords.Count()) {
                children.AddRange(
                    _contentManager
                        .GetMany<TerritoryPart>(
                            selectedRecords.Skip(tried).Take(1000).Select(r => r.Id),
                            // Consider eventually using the version from the hierarchy?
                            VersionOptions.Published,
                            QueryHints.Empty));
                tried += 1000;
            }

            return children;
        }

        private IEnumerable<TerritoryPartRecord> GetAllChildrenRecords(
            IEnumerable<TerritoryPartRecord> records) {
            var result = new List<TerritoryPartRecord>(records);
            if (records.Any()) {
                // if there are children, add those as well as their children
                result.AddRange(GetAllChildrenRecords(records.SelectMany(r => r.Children)));
            }
            return result.Where(tpr => tpr != null && tpr.TerritoryInternalRecord != null);
        }

        private IEnumerable<TerritoryPart> GetAllChildrenParts(
            IEnumerable<TerritoryPart> parts) {
            var result = new List<TerritoryPart>(parts);
            if (parts.Any()) {
                // if there are children, add those as well as their children
                result.AddRange(
                    GetAllChildrenParts(
                        parts.SelectMany(r => 
                            r.Children.AsPart<TerritoryPart>())));
            }
            return result;
        }

    }
}