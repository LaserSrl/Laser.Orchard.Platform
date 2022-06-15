using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class TerritoryISO3166CodePartDriver :
        ContentPartDriver<TerritoryISO3166CodePart> {
        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;

        public TerritoryISO3166CodePartDriver(
            ITerritoriesRepositoryService territoriesRepositoryService) {

            _territoriesRepositoryService = territoriesRepositoryService;

        }

        protected override string Prefix {
            get { return "TerritoryISO3166CodePart"; }
        }

        protected override DriverResult Editor(TerritoryISO3166CodePart part, dynamic shapeHelper) {
            return ContentShape("Parts_Territory_ISO3166Code_Edit", () => {
                // only show for territories
                var territory = part.As<TerritoryPart>();
                if (territory == null) {
                    return null;
                }
                return shapeHelper.EditorTemplate(
                     TemplateName: "Parts/Territory.ISO3166Code",
                     Model: new TerritoryISO3166CodeEditViewModel(part),
                     Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(TerritoryISO3166CodePart part, IUpdateModel updater, dynamic shapeHelper) {
            var territory = part.As<TerritoryPart>();
            // only if the content is a territory
            if (territory != null) {
                var vm = new TerritoryISO3166CodeEditViewModel();
                if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                    part.ISO3166Code = vm.ISO3166Code;
                    part.Record.TerritoryInternalRecord = territory.Record.TerritoryInternalRecord;
                }
            }
            return Editor(part, shapeHelper);
        }

        protected override void Importing(TerritoryISO3166CodePart part, ImportContentContext context) {
            part.ISO3166Code = context.Attribute(part.PartDefinition.Name, "ISO3166Code");

            var internalIdentity = context.Attribute(part.PartDefinition.Name, "TerritoryInternalRecordId");
            if (internalIdentity == null) {
                part.Record.TerritoryInternalRecord = null;
            } else {
                var internalRecord = _territoriesRepositoryService.GetTerritoryInternal(internalIdentity);
                part.Record.TerritoryInternalRecord = internalRecord;
            }
        }

        protected override void Exporting(TerritoryISO3166CodePart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            element
                .SetAttributeValue("ISO3166Code", 
                    part.ISO3166Code != null ? part.ISO3166Code.ToString() : string.Empty);

            if (part.Record != null && part.Record.TerritoryInternalRecord != null) {
                element.SetAttributeValue("TerritoryInternalRecordId", part.Record.TerritoryInternalRecord.Name);
            }
        }
    }
}