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

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class TerritoryAddressTypePartDriver :
        ContentPartDriver<TerritoryAddressTypePart> {
        private readonly IAddressConfigurationSettingsService _addressConfigurationSettingsService;
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;

        public TerritoryAddressTypePartDriver(
            IAddressConfigurationSettingsService addressConfigurationSettingsService,
            ITerritoriesRepositoryService territoriesRepositoryService) {

            _addressConfigurationSettingsService = addressConfigurationSettingsService;
            _territoriesRepositoryService = territoriesRepositoryService;

            T = NullLocalizer.Instance;
        }

        protected override string Prefix {
            get { return "TerritoryAddressTypePart"; }
        }
        public Localizer T { get; set; }

        protected override DriverResult Editor(TerritoryAddressTypePart part, dynamic shapeHelper) {
            return ContentShape("Parts_Territory_AddressType_Edit", () => {
                // only show for territories
                var territory = part.As<TerritoryPart>();
                if (territory == null) {
                    return null;
                }
                // we put this configuration for territories in any possible hierarchy,
                // not only the one for the advanced address settings
                return shapeHelper.EditorTemplate(
                     TemplateName: "Parts/Territory.AddressType",
                     Model: new TerritoryAddressTypeEditViewModel(part),
                     Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(TerritoryAddressTypePart part, IUpdateModel updater, dynamic shapeHelper) {
            var territory = part.As<TerritoryPart>();
            // only if the content is a territory
            if (territory != null) {
                var vm = new TerritoryAddressTypeEditViewModel();
                if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                    part.Shipping = vm.Shipping;
                    part.Billing = vm.Billing;
                    part.Record.TerritoryInternalRecord = territory.Record.TerritoryInternalRecord;
                }
            }
            return Editor(part, shapeHelper);
        }

        protected override void Importing(TerritoryAddressTypePart part, ImportContentContext context) {
            var importedBool = context.Attribute(part.PartDefinition.Name, "Shipping");
            bool value;
            if (bool.TryParse(importedBool, out value)) {
                part.Shipping = value;
            }
            importedBool = context.Attribute(part.PartDefinition.Name, "Billing");
            if (bool.TryParse(importedBool, out value)) {
                part.Billing = value;
            }

            var internalIdentity = context.Attribute(part.PartDefinition.Name, "TerritoryInternalRecordId");
            if (internalIdentity == null) {
                part.Record.TerritoryInternalRecord = null;
            } else {
                var internalRecord = _territoriesRepositoryService.GetTerritoryInternal(internalIdentity);
                part.Record.TerritoryInternalRecord = internalRecord;
            };
        }

        protected override void Exporting(TerritoryAddressTypePart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            element
                .SetAttributeValue("Shipping", part.Shipping.ToString());
            element
                .SetAttributeValue("Billing", part.Billing.ToString());

            if (part.Record.TerritoryInternalRecord != null) {
                element.SetAttributeValue("TerritoryInternalRecordId", part.Record.TerritoryInternalRecord.Name);
            }
        }
    }
}