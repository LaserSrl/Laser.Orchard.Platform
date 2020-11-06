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

        public DestinationTerritoryCriteria(
            ITerritoriesRepositoryService territoriesRepositoryService,
            IAddressConfigurationService addressConfigurationService,
            ITerritoryPartRecordService territoryPartRecordService) {

            _territoriesRepositoryService = territoriesRepositoryService;
            _addressConfigurationService = addressConfigurationService;
            _territoryPartRecordService = territoryPartRecordService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
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
                    // these ids are for the InternalTerritoryRecords
                    // As soon as one of them is among the configured territories for
                    // the criterion, or is a child of a configured territory, the
                    // criterion is known to apply.
                    if (ids.Any()) {
                        var destinationInternalRecords = ids.Select(i =>
                            _territoriesRepositoryService.GetTerritoryInternal(i));
                        var selectedTerritories = (List<TerritoryTag>)(JsonConvert
                            .DeserializeObject<List<TerritoryTag>>(
                                context.State["Territories"]?.ToString() ?? "[]"));
                        if (destinationInternalRecords.Any(tir =>
                            selectedTerritories.Any(st =>
                                st.NameHash.Equals(tir.NameHash)))) {
                            // a territory we are sending stuff to is among those selected
                            applicable = true;
                        } else {
                            // check the children of selected territories in the shipping hierarchy
                            // we don't want to be in this branch because this is slower
                            // so for example it would be better to configure a criteria for all
                            // the EU countries rather than for the EU, I think.
                            var selectedInternalIds = selectedTerritories
                                .Select(tt => tt.InternalId);
                            foreach (var internalId in selectedInternalIds) {
                                var part = _addressConfigurationService.SingleTerritory(internalId);
                                if (part != null) {
                                    //var records = part.Record.Children;
                                    var records = _territoryPartRecordService.GetTerritoriesChild(part);
                                    // while we haven't found that we are done
                                    while (!applicable
                                        // and there still are records to check
                                        && records != null && records.Any()) {
                                        if (records.Any(tir =>
                                            destinationInternalRecords.Any(dir =>
                                                dir.NameHash.Equals(tir.TerritoryInternalRecord.NameHash)))) {
                                            // a territory we are sending stuff to is among those selected
                                            applicable = true;
                                            break;
                                        }
                                        // prepare for next iteration
                                        records = records.SelectMany(r => _territoryPartRecordService.GetTerritoriesChild(r)).ToList();
                                    }
                                }

                                if (applicable) {
                                    // we found that we are ok, so stop checking
                                    break;
                                }
                            }
                        }

                    }
                }
                context.IsApplicable &= applicable;
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
                    // these ids are for the InternalTerritoryRecords
                    // As soon as one of them is among the configured territories for
                    // the criterion, or is a child of a configured territory, the
                    // criterion is known to apply.
                    if (ids.Any()) {
                        var destinationInternalRecords = ids.Select(i =>
                            _territoriesRepositoryService.GetTerritoryInternal(i));
                        var selectedTerritories = (List<TerritoryTag>)(JsonConvert
                            .DeserializeObject<List<TerritoryTag>>(
                                context.State["Territories"]?.ToString() ?? "[]"));
                        if (destinationInternalRecords.Any(tir =>
                            selectedTerritories.Any(st =>
                                st.NameHash.Equals(tir.NameHash)))) {
                            // a territory we are sending stuff to is among those selected
                            applicable = true;
                        } else {
                            // check the children of selected territories in the shipping hierarchy
                            // we don't want to be in this branch because this is slower
                            // so for example it would be better to configure a criteria for all
                            // the EU countries rather than for the EU, I think.
                            var selectedInternalIds = selectedTerritories
                                .Select(tt => tt.InternalId);
                            foreach (var internalId in selectedInternalIds) {
                                var part = _addressConfigurationService.SingleTerritory(internalId);
                                if (part != null) {
                                    var records = _territoryPartRecordService
                                        .GetTerritoriesChild(part); //part.Record.Children;
                                    // while we haven't found that we are done
                                    while (!applicable
                                        // and there still are records to check
                                        && records != null && records.Any()) {
                                        if (records.Any(tir =>
                                            destinationInternalRecords.Any(dir =>
                                                dir.NameHash.Equals(tir.TerritoryInternalRecord.NameHash)))) {
                                            // a territory we are sending stuff to is among those selected
                                            applicable = true;
                                            break;
                                        }
                                        // prepare for next iteration
                                        records = records.SelectMany(r => _territoryPartRecordService.GetTerritoriesChild(r)).ToList();
                                    }
                                }

                                if (applicable) {
                                    // we found that we are ok, so stop checking
                                    break;
                                }
                            }
                        }

                    }
                }
                context.IsApplicable &= outerCriterion(applicable);
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