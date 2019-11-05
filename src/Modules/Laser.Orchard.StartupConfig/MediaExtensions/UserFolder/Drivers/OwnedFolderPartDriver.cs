using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.MediaExtensions.UserFolder.Models;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.MediaExtensions.UserFolder.Drivers {

    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class OwnedFolderPartDriver : ContentPartCloningDriver<OwnedFolderPart> {

        protected override string Prefix { get { return "OwnedFolder"; } }

        protected override DriverResult Editor(OwnedFolderPart part, dynamic shapeHelper) {
            return ContentShape("Parts_OwnedFolder_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/OwnedFolder_Edit", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(OwnedFolderPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(OwnedFolderPart part, ExportContentContext context) {
            var folderName = part.FolderName;
            context.Element(part.PartDefinition.Name).SetAttributeValue("FolderName", folderName);
        }

        protected override void Importing(OwnedFolderPart part, ImportContentContext context) {
            var importedFolderName = context.Attribute(part.PartDefinition.Name, "FolderName");
            if (importedFolderName != null) {
                part.FolderName = importedFolderName;
            }
        }

        protected override void Cloning(OwnedFolderPart originalPart, OwnedFolderPart clonePart, CloneContentContext context) {
            clonePart.FolderName = originalPart.FolderName;
        }
    }
}


