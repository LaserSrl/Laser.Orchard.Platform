using KrakeAdmin.Settings.Models;
using KrakeAdmin.Settings.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KrakeAdmin.Settings.Drivers {
    public class KrakeAdminThemeSettingsDriver : ContentPartDriver<KrakeAdminThemeSettingsPart> {
        private readonly IThemeSettingsService _themeSettingsService;
        public KrakeAdminThemeSettingsDriver(IThemeSettingsService themeSettingsService) {
            _themeSettingsService = themeSettingsService;
        }
        protected override void Exporting(KrakeAdminThemeSettingsPart part, ExportContentContext context) {
            var settings = _themeSettingsService.GetSettings();
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("HeaderLogoUrl", settings.HeaderLogoUrl);
            root.SetAttributeValue("PlaceholderLogoUrl", settings.PlaceholderLogoUrl);
            root.SetAttributeValue("PlaceholderSmallLogoUrl", settings.PlaceholderSmallLogoUrl);
            root.SetAttributeValue("BaseLineText", settings.BaseLineText);
        }
        protected override void Importing(KrakeAdminThemeSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            if (root == null) {
                return;
            }
            var settings = _themeSettingsService.GetSettings();
            settings.HeaderLogoUrl = root.Attribute("HeaderLogoUrl") != null ? root.Attribute("HeaderLogoUrl").Value : "";
            settings.PlaceholderLogoUrl = root.Attribute("PlaceholderLogoUrl") != null ? root.Attribute("PlaceholderLogoUrl").Value : "";
            settings.PlaceholderSmallLogoUrl = root.Attribute("PlaceholderSmallLogoUrl") != null ? root.Attribute("PlaceholderSmallLogoUrl").Value : "";
            settings.BaseLineText = root.Attribute("BaseLineText") != null ? root.Attribute("BaseLineText").Value : "";
        }
    }
}