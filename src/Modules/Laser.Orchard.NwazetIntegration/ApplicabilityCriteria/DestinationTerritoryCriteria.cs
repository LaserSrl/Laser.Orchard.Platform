using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Newtonsoft.Json;
using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class DestinationTerritoryCriteria : IApplicabilityCriterionProvider {
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly ITerritoryPartRecordService _territoryPartRecordService;
        private readonly IAddressConfigurationSettingsService _addressConfigurationSettingsService;

        public DestinationTerritoryCriteria(
            ITerritoriesRepositoryService territoriesRepositoryService,
            IAddressConfigurationService addressConfigurationService,
            ITerritoryPartRecordService territoryPartRecordService,
            IAddressConfigurationSettingsService addressConfigurationSettingsService) {

            _territoriesRepositoryService = territoriesRepositoryService;
            _addressConfigurationService = addressConfigurationService;
            _territoryPartRecordService = territoryPartRecordService;
            _addressConfigurationSettingsService = addressConfigurationSettingsService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private readonly Dictionary<int, List<int>> hierarchyTerritoriesIds = new Dictionary<int, List<int>>();

        public void Describe(DescribeCriterionContext describe) {
            describe
                .For("Destination",
                    T("Destination in territories"),
                    T("Destination in territories"))
                .Element("Destination in territories",
                    T("Destination in territories"),
                    T("Destination in territories Criterion"),
                    (ctx) => ApplyCriteria(ctx, (b) => b),
                    DisplayTrueCriteria,
                    DestinationTerritoryForm.FormName)
                .Element("Destination not in territories",
                    T("Destination not in territories"),
                    T("Destination not in territories Criterion"),
                    (ctx) => ApplyCriteria(ctx, (b) => !b),
                    DisplayFalseCriteria,
                    DestinationTerritoryForm.FormName);
        }

        public void ApplyCriteria(CriterionContext context) {
            if (context.IsApplicable) {
                // in the postalcode we pass, after the postal code, 
                // a list of territory Ids separated by semicolon.
                var zipSplit = (context
                    .ApplicabilityContext
                    .ZipCode ?? "")
                    .Split(new char[] { ';' }, StringSplitOptions.None);
                var applicable = false;
                if (zipSplit.Any()) {
                    var ids = zipSplit
                        .Skip(1)
                        .Select(s => {
                            int tmp = 0;
                            int.TryParse(s, out tmp);
                            return tmp;
                        })
                        .Where(i => i > 0);

                    if (ids.Count() > 0) {
                        // populates the list of the fathers of the territory only once
                        // the dictionary contains the id of the territory and the list of all its fathers until the last
                        if (hierarchyTerritoriesIds.Count() == 0) {
                            List<int> listParents = new List<int>();
                            var hierarchyId = _addressConfigurationSettingsService.ShippingCountriesHierarchy.Id;
                            var territoryId = ids.ToList()[ids.Count() - 1];
                            List<int> territoriesIds = new List<int>();
                            territoriesIds.Add(territoryId);
                            var parentId = _territoryPartRecordService.GetParentTerritoryId(territoryId, hierarchyId);
                            // if the territory has no relatives the list will contain only its own id
                            // otherwise it will call the method that populates its list
                            if (parentId != 0) {
                                listParents = _territoryPartRecordService.
                                    GetListOfParentIds(parentId,
                                            hierarchyId,
                                            territoriesIds);
                            }
                            else {
                                listParents = territoriesIds;
                            }
                            hierarchyTerritoriesIds.Add(territoryId, listParents);
                        }

                        // these ids are for the InternalTerritoryRecords
                        // As soon as one of them is among the configured territories for
                        // the criterion, or is a child of a configured territory, the
                        // criterion is known to apply.
                        if (hierarchyTerritoriesIds.Count() > 0) {
                            foreach (var idsT in hierarchyTerritoriesIds.Values) {
                                var destinationInternalRecords = idsT.Select(i =>
                                    _territoriesRepositoryService.GetTerritoryInternal(i));
                                var selectedTerritories = (List<TerritoryTag>)(JsonConvert
                                    .DeserializeObject<List<TerritoryTag>>(
                                        context.State["Territories"]?.ToString() ?? "[]"));
                                if (destinationInternalRecords.Any(tir =>
                                    selectedTerritories.Any(st =>
                                        st.NameHash.Equals(tir.NameHash)))) {
                                    // a territory we are sending stuff to is among those selected
                                    applicable = true;
                                }
                            }
                        }
                        context.IsApplicable &= applicable;
                    }
                    else {
                        context.IsApplicable = false;
                    }
                }
               
            }

        }

        public void ApplyCriteria(CriterionContext context,
            // Use outerCriterion to negate the test, so we can easily do
            // contains / doesn't contain
            Func<bool, bool> outerCriterion) {

            if (context.IsApplicable) {
                // in the postalcode we pass, after the postal code, 
                // a list of territory Ids separated by semicolon.
                var zipSplit = (context
                    .ApplicabilityContext
                    .ZipCode ?? "")
                    .Split(new char[] { ';' }, StringSplitOptions.None);
                var applicable = false;
                if (zipSplit.Any()) {
                    var ids = zipSplit
                        .Skip(1)
                        .Select(s => {
                            int tmp = 0;
                            int.TryParse(s, out tmp);
                            return tmp;
                        })
                        .Where(i => i > 0);

                    // populates the list of the fathers of the territory only once
                    // the dictionary contains the id of the territory and the list of all its fathers until the last
                    if (ids.Count() > 0) {
                        if (hierarchyTerritoriesIds.Count() == 0) {
                            List<int> listParents = new List<int>();
                            var hierarchyId = _addressConfigurationSettingsService.ShippingCountriesHierarchy.Id;
                            var territoryId = ids.ToList()[ids.Count() - 1];
                            List<int> territoriesIds = new List<int>();
                            territoriesIds.Add(territoryId);
                            var parentId = _territoryPartRecordService.GetParentTerritoryId(territoryId, hierarchyId);
                            // if the territory has no relatives the list will contain only its own id
                            // otherwise it will call the method that populates its list
                            if (parentId != 0) {
                                listParents = _territoryPartRecordService.
                                    GetListOfParentIds(parentId,
                                            hierarchyId,
                                            territoriesIds);
                            }
                            else {
                                listParents = territoriesIds;
                            }
                            hierarchyTerritoriesIds.Add(territoryId, listParents);
                        }

                        // these ids are for the InternalTerritoryRecords
                        // As soon as one of them is among the configured territories for
                        // the criterion, or is a child of a configured territory, the
                        // criterion is known to apply.
                        if (hierarchyTerritoriesIds.Count() > 0) {
                            foreach (var idsT in hierarchyTerritoriesIds.Values) {
                                var destinationInternalRecords = idsT.Select(i =>
                                    _territoriesRepositoryService.GetTerritoryInternal(i));
                                var selectedTerritories = (List<TerritoryTag>)(JsonConvert
                                    .DeserializeObject<List<TerritoryTag>>(
                                        context.State["Territories"]?.ToString() ?? "[]"));
                                if (destinationInternalRecords.Any(tir =>
                                    selectedTerritories.Any(st =>
                                        st.NameHash.Equals(tir.NameHash)))) {
                                    // a territory we are sending stuff to is among those selected
                                    applicable = true;
                                }
                            }
                        }
                        context.IsApplicable &= outerCriterion(applicable);
                    }
                    else {
                        // Added the else because the first time he enters to do this check
                        // it does not have the id list compiled and therefore does not do any checking
                        // IsApplicable does not have to do any reasoning other than denying that the check was successful
                        // this else was added after we noticed that in the negative condition
                        // the outerCriterion converted the applicable assignment to false
                        context.IsApplicable = false;
                    }
                }
            }
        }

        public LocalizedString DisplayTrueCriteria(CriterionContext context) {
            return T("Destination is in a group of territories (perhaps as a child).");
        }
        public LocalizedString DisplayFalseCriteria(CriterionContext context) {
            return T("Destination is not in a group of territories (perhaps as a child).");
        }
    }
}