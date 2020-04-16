using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class TerritoryAdministrativeTypePartDriver :
        ContentPartDriver<TerritoryAdministrativeTypePart> {

        private readonly IAddressConfigurationSettingsService _addressConfigurationSettingsService;
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;

        public TerritoryAdministrativeTypePartDriver(
            IAddressConfigurationSettingsService addressConfigurationSettingsService,
            ITerritoriesRepositoryService territoriesRepositoryService) {

            _addressConfigurationSettingsService = addressConfigurationSettingsService;
            _territoriesRepositoryService = territoriesRepositoryService;

            T = NullLocalizer.Instance;
        }

        protected override string Prefix {
            get { return "TerritoryAdministrativeTypePart"; }
        }
        public Localizer T { get; set; }

        protected override DriverResult Editor(TerritoryAdministrativeTypePart part, dynamic shapeHelper) {
            return ContentShape("Parts_Territory_AdministrativeType_Edit", () => {
                // only show for territories
                var territory = part.As<TerritoryPart>();
                if (territory == null) {
                    return null;
                }
                // we put this configuration for territories in any possible hierarchy,
                // not only the one for the advanced address settings
                return shapeHelper.EditorTemplate(
                     TemplateName: "Parts/Territory.AdministrativeType",
                     Model: new TerritoryAdministrativeTypeEditViewModel(part) {
                         AdministrativeTypes = new SelectListItem[] {
                             new SelectListItem() {
                                 Value = TerritoryAdministrativeType.None.ToString(),
                                 Text = T("Undefined").Text
                             },
                             new SelectListItem() {
                                 Value = TerritoryAdministrativeType.Country.ToString(),
                                 Text = T("Country").Text
                             },
                             new SelectListItem() {
                                 Value = TerritoryAdministrativeType.Province.ToString(),
                                 Text = T("Province").Text
                             },
                             new SelectListItem() {
                                 Value = TerritoryAdministrativeType.City.ToString(),
                                 Text = T("City").Text
                             }
                         }
                     },
                     Prefix: Prefix);
            });
        }
        protected override DriverResult Editor(TerritoryAdministrativeTypePart part, IUpdateModel updater, dynamic shapeHelper) {
            var territory = part.As<TerritoryPart>();
            // only if the content is a territory
            if (territory != null) {
                var vm = new TerritoryAdministrativeTypeEditViewModel();
                if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                    part.AdministrativeType = vm.AdministrativeType;
                    part.Record.TerritoryInternalRecord = territory.Record.TerritoryInternalRecord;
                }
            }
            return Editor(part, shapeHelper);
        }

        protected override void Importing(TerritoryAdministrativeTypePart part, ImportContentContext context) {
            var importedType = context.Attribute(part.PartDefinition.Name, "AdministrativeType");
            TerritoryAdministrativeType value;
            if (Enum.TryParse(importedType, out value)) {
                part.AdministrativeType = value;
            }

            var internalIdentity = context.Attribute(part.PartDefinition.Name, "TerritoryInternalRecordId");
            if (internalIdentity == null) {
                part.Record.TerritoryInternalRecord = null;
            } else {
                var internalRecord = _territoriesRepositoryService.GetTerritoryInternal(internalIdentity);
                part.Record.TerritoryInternalRecord = internalRecord;
            }
        }

        protected override void Exporting(TerritoryAdministrativeTypePart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            element
                .SetAttributeValue("AdministrativeType", part.AdministrativeType.ToString());

            if (part.Record.TerritoryInternalRecord != null) {
                element.SetAttributeValue("TerritoryInternalRecordId", part.Record.TerritoryInternalRecord.Name);
            }
        }
    }
}