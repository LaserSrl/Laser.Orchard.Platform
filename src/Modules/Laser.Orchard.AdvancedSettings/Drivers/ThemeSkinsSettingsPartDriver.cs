using Laser.Orchard.AdvancedSettings.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.AdvancedSettings.Drivers {
    [OrchardFeature("Laser.Orchard.ThemeSkins")]
    public class ThemeSkinsSettingsPartDriver : ContentPartDriver<ThemeSkinsSettingsPart>{
        private const string TemplateName = "Settings/ThemeSkinsSettings";
        protected override string Prefix => "ThemeSkinsSettingsPart";

        protected override DriverResult Editor(ThemeSkinsSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Settings_ThemeSkinsSettingsPart_Edit",
                () => {
                    return shapeHelper.EditorTemplate(
                        TemplateName: TemplateName,
                        Model: part,
                        Prefix: Prefix
                        );
                })
                .OnGroup(ThemeSkinsSettingsPart.EditorGroupId);
        }

        protected override DriverResult Editor(ThemeSkinsSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(part, Prefix, null, null)) {
                if (part.AvailableSkinNames == null
                        || part.AvailableSkinNames.Length == 0
                        || part.AvailableSkinNames.Contains(ThemeSkinsSettingsPart.AllSkinsValue)) {
                    part.AvailableSkinNames = new string[] { ThemeSkinsSettingsPart.AllSkinsValue };
                }
            }
            return Editor(part, shapeHelper);
        }

        protected override void Importing(ThemeSkinsSettingsPart part, ImportContentContext context) {
            var partName = part.PartDefinition.Name;
            var root = context.Data.Element(partName);
            if (root == null) {
                return;
            }

            var names = context.Attribute(partName, "AvailableSkinNames")
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Any()) {
                part.AvailableSkinNames = names;
            } else {
                part.AvailableSkinNames = new string[] { ThemeSkinsSettingsPart.AllSkinsValue };
            }
        }

        protected override void Exporting(ThemeSkinsSettingsPart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            var names = new List<string>();
            if (part.AvailableSkinNames == null
                || part.AvailableSkinNames.Length == 0
                || part.AvailableSkinNames.Contains(ThemeSkinsSettingsPart.AllSkinsValue)) {
                names.Add(ThemeSkinsSettingsPart.AllSkinsValue);
            } else {
                names.AddRange(part.AvailableSkinNames);
            }
            element.SetAttributeValue("AvailableSkinNames", string.Join(",", names));
        }
    }
}