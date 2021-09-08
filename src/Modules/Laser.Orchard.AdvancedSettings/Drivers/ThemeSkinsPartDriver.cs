using Laser.Orchard.AdvancedSettings.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.Drivers {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsPartDriver : ContentPartDriver<ThemeSkinsPart> {

        protected override DriverResult Editor(ThemeSkinsPart part, dynamic shapeHelper) {
            return null; // base.Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(ThemeSkinsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return null; //base.Editor(part, updater, shapeHelper);
        }

        protected override void Importing(ThemeSkinsPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }
            part.SkinName = context.Attribute(part.PartDefinition.Name, "SkinName");
        }

        protected override void Exporting(ThemeSkinsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("SkinName", part.SkinName);
        }
    }
}