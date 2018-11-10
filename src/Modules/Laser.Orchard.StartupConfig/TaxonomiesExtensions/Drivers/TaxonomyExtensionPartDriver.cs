using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Drivers {

    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TaxonomyExtensionPartDriver : ContentPartCloningDriver<TaxonomyExtensionPart> {

        protected override string Prefix { get { return "TaxonomyExtension"; } }

        protected override DriverResult Editor(TaxonomyExtensionPart part, dynamic shapeHelper) {
            return ContentShape("Parts_TaxonomyExtension_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/TaxonomyExtension_Edit", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(TaxonomyExtensionPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(TaxonomyExtensionPart part, ExportContentContext context) {
            var OrderBy = (OrderType)part.OrderBy;
            context.Element(part.PartDefinition.Name).SetAttributeValue("OrderBy", OrderBy);
        }

        protected override void Importing(TaxonomyExtensionPart part, ImportContentContext context) {
            var importedOrderBy = context.Attribute(part.PartDefinition.Name, "OrderBy");
            if (importedOrderBy != null) {
                part.OrderBy = (OrderType)Enum.Parse(typeof(OrderType), importedOrderBy);
            }
        }

        protected override void Cloning(TaxonomyExtensionPart originalPart, TaxonomyExtensionPart clonePart, CloneContentContext context) {
            clonePart.OrderBy = originalPart.OrderBy;
        }
    }
}


