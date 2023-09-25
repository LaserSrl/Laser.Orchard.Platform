using Laser.Orchard.Cache.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.Cache.Drivers {
    [OrchardFeature("Laser.Orchard.NavigationCache")]
    public class CacheableMenuSettingsPartDriver : ContentPartCloningDriver<CacheableMenuSettingsPart> {

        public CacheableMenuSettingsPartDriver() { }

        protected override string Prefix => "CacheableMenuSettingsPart";

        protected override DriverResult Editor(CacheableMenuSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_CacheableMenuSettingsPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/CacheableMenuSettingsPart",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(CacheableMenuSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(CacheableMenuSettingsPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("IsFrontEndCacheable", part.IsFrontEndCacheable);
        }

        protected override void Importing(CacheableMenuSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var feCacheable = root.Attribute("IsFrontEndCacheable");
            if (feCacheable != null) {
                part.IsFrontEndCacheable = Convert.ToBoolean(feCacheable.Value);
            }
        }
    }
}