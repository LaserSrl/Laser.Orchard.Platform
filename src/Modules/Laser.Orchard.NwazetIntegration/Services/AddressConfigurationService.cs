using Laser.Orchard.NwazetIntegration.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
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
            var territory = SingleTerritory(internalId);
            if (territory != null
                && territory.Is<TerritoryAdministrativeTypePart>()
                && territory.As<TerritoryAdministrativeTypePart>().AdministrativeType == TerritoryAdministrativeType.Country) {
                return territory;
            }
            return null;
        }

        public TerritoryPart GetCountry(string name) {
            var territory = SingleTerritory(name);
            if (territory != null
                && territory.Is<TerritoryAdministrativeTypePart>()
                && territory.As<TerritoryAdministrativeTypePart>().AdministrativeType == TerritoryAdministrativeType.Country) {
                return territory;
            }
            return null;
        }

        public TerritoryPart GetCity(int internalId) {
            var territory = SingleTerritory(internalId);
            if (territory != null
                && territory.Is<TerritoryAdministrativeTypePart>()
                && territory.As<TerritoryAdministrativeTypePart>().AdministrativeType == TerritoryAdministrativeType.City) {
                return territory;
            }
            return null;
        }

        public TerritoryPart GetCity(string name) {
            var territory = SingleTerritory(name);
            if (territory != null
                && territory.Is<TerritoryAdministrativeTypePart>()
                && territory.As<TerritoryAdministrativeTypePart>().AdministrativeType == TerritoryAdministrativeType.City) {
                return territory;
            }
            return null;
        }

        public TerritoryPart GetProvince(int internalId) {
            var territory = SingleTerritory(internalId);
            if (territory != null
                && territory.Is<TerritoryAdministrativeTypePart>()
                && territory.As<TerritoryAdministrativeTypePart>().AdministrativeType == TerritoryAdministrativeType.Province) {
                return territory;
            }
            return null;
        }

        public TerritoryPart GetProvince(string name) {
            var territory = SingleTerritory(name);
            if (territory != null
                && territory.Is<TerritoryAdministrativeTypePart>()
                && territory.As<TerritoryAdministrativeTypePart>().AdministrativeType == TerritoryAdministrativeType.Province) {
                return territory;
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
            }
            else {
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

        // Refactorized START
        public IEnumerable<TerritoryPart> GetAllProvinces(
            TerritoryPart country, TerritoryPart city) {
            if (city != null) {
                return GetTerritoryParents(new TerritoryQueryContext {
                    TerritoryAdministrativeType = TerritoryAdministrativeType.Province,
                    ForTerritory = city
                });

            }
            else {
                return GetTerritoryChildren(new TerritoryQueryContext {
                    TerritoryAdministrativeType = TerritoryAdministrativeType.Province,
                    ForTerritory = country
                });
            }
        }
        public IEnumerable<TerritoryPart> GetAllProvinces(
            AddressRecordType addressRecordType, TerritoryPart country, TerritoryPart city) {
            if (city != null) {
                return GetTerritoryParents(new TerritoryQueryContext {
                    AddressRecordType = addressRecordType,
                    TerritoryAdministrativeType = TerritoryAdministrativeType.Province,
                    ForTerritory = city
                });

            }
            else {
                return GetTerritoryChildren(new TerritoryQueryContext {
                    AddressRecordType = addressRecordType,
                    TerritoryAdministrativeType = TerritoryAdministrativeType.Province,
                    ForTerritory = country
                });
            }

        }

        public IEnumerable<TerritoryPart> GetAllProvinces(TerritoryPart country) {
            return GetTerritoryChildren(new TerritoryQueryContext {
                TerritoryAdministrativeType = TerritoryAdministrativeType.Province,
                ForTerritory = country
            });
        }

        public IEnumerable<TerritoryPart> GetAllProvinces(
            AddressRecordType addressRecordType, TerritoryPart country) {
            return GetTerritoryChildren(new TerritoryQueryContext {
                AddressRecordType = addressRecordType,
                TerritoryAdministrativeType = TerritoryAdministrativeType.Province,
                ForTerritory = country
            });
        }

        public IEnumerable<TerritoryPart> GetAllCities() {

            return _territoriesService.GetTerritoriesQuery(ConfiguredHierarchy)
                // only those marked as city
                .Join<TerritoryAdministrativeTypePartRecord>()
                .Where(tat => tat.AdministrativeType == TerritoryAdministrativeType.City)
                .List();
        }

        public IEnumerable<TerritoryPart> GetAllCities(TerritoryPart parent) {
            return GetTerritoryChildren(new TerritoryQueryContext {
                TerritoryAdministrativeType = TerritoryAdministrativeType.City,
                ForTerritory = parent
            });
        }

        public IEnumerable<TerritoryPart> GetAllCities(
            AddressRecordType addressRecordType, TerritoryPart parent) {
            return GetTerritoryChildren(new TerritoryQueryContext {
                AddressRecordType = addressRecordType,
                TerritoryAdministrativeType = TerritoryAdministrativeType.City,
                ForTerritory = parent
            });
        }

        public IEnumerable<TerritoryPart> GetAllCities(
            AddressRecordType addressRecordType, TerritoryPart parent,
            string nameQuery, int maxOptions = 20) {
            return GetTerritoryChildren(new TerritoryQueryContext {
                AddressRecordType = addressRecordType,
                TerritoryAdministrativeType = TerritoryAdministrativeType.City,
                Filter = nameQuery,
                ForTerritory = parent
            });
        }
        // Refactorized END

        private IEnumerable<TerritoryPart> GetTerritoryChildren(TerritoryQueryContext context) {
            IEnumerable<TerritoryPart> list;
            var root = context.ForTerritory;
            // make sure root we'll use belongs to hierarchy
            if (root.HierarchyPart.Id != ConfiguredHierarchy.Id) {
                root = SingleTerritory(root.Record.TerritoryInternalRecord.Id);
            }
            if (root == null) {
                // if the root is not valid for the hierarchy, we cannot return 
                // any city.
                return Enumerable.Empty<TerritoryPart>();
            }

            var rootPath = root.TerritoriesFullPath;
            var query = _contentManager.Query();
            query = query.Join<TerritoryPartRecord>()
            .Where(x => x.TerritoriesFullPath.StartsWith(rootPath) && x.Hierarchy.Id == ConfiguredHierarchy.Id);

            if (context.AddressRecordType.HasValue) {
                query = query.Join<TerritoryAdministrativeTypePartRecord>()
                    .Where(x => x.AdministrativeType == context.TerritoryAdministrativeType.Value);
            }
            if (context.AddressRecordType.HasValue) {
                if (context.AddressRecordType == AddressRecordType.ShippingAddress) {
                    query = query.Join<TerritoryAddressTypePartRecord>()
                        .Where(x => x.Shipping);
                }
                else if (context.AddressRecordType == AddressRecordType.BillingAddress) {
                    query = query.Join<TerritoryAddressTypePartRecord>()
                        .Where(x => x.Billing);
                }
            }
            if (!string.IsNullOrWhiteSpace(context.Filter)) {
                query = query.Join<TitlePartRecord>()
                    .Where(x => x.Title.Contains(context.Filter))
                    .OrderBy(x => x.Title);
            }
            else {
                query = query.Join<TitlePartRecord>()
                    .OrderBy(x => x.Title);

            }
            if (context.MaxResultItems.HasValue) {
                list = query.ForPart<TerritoryPart>().Slice(0, context.MaxResultItems.Value);
            }
            else {
                list = query.ForPart<TerritoryPart>().List();
            }
            return list;

        }
        private IEnumerable<TerritoryPart> GetTerritoryParents(TerritoryQueryContext context) {
            IEnumerable<TerritoryPart> list;
            var root = context.ForTerritory;
            // make sure root we'll use belongs to hierarchy
            if (root.HierarchyPart.Id != ConfiguredHierarchy.Id) {
                root = SingleTerritory(root.Record.TerritoryInternalRecord.Id);
            }
            if (root == null) {
                // if the root is not valid for the hierarchy, we cannot return 
                // any city.
                return Enumerable.Empty<TerritoryPart>();
            }

            var rootPath = root.TerritoriesFullPath;
            var parentIds = rootPath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Select(x => {
                int id = 0;
                int.TryParse(x, out id);
                return id;
            }).ToList();
            parentIds.ToList().Remove(root.Id);
            var queryHints = new QueryHints()
                    .ExpandRecords<TerritoryAdministrativeTypePartRecord, TitlePartRecord>();
            var query = _contentManager.Query();

            query = query
                .Join<TerritoryAdministrativeTypePartRecord>()
                .Where(x => x.AdministrativeType == context.TerritoryAdministrativeType.Value && parentIds.Contains(x.Id))
                .Join<TitlePartRecord>()
                .OrderBy(x => x.Title);

            if (context.MaxResultItems.HasValue) {
                list = query.ForPart<TerritoryPart>().Slice(0, context.MaxResultItems.Value);
            }
            else {
                list = query.ForPart<TerritoryPart>().List();
            }
            return list;

        }
    }

    internal class TerritoryQueryContext {
        internal TerritoryQueryContext() {
            Filter = "";
        }
        public AddressRecordType? AddressRecordType { get; set; }
        public TerritoryPart ForTerritory { get; set; }
        public string Filter { get; set; }
        public TerritoryAdministrativeType? TerritoryAdministrativeType { get; set; }
        public int? MaxResultItems { get; set; }
    }
}