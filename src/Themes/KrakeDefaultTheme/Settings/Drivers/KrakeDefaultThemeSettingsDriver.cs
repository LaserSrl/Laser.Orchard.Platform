using KrakeDefaultTheme.Settings.Models;
using KrakeDefaultTheme.Settings.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace KrakeDefaultTheme.Settings.Drivers {
    public class KrakeDefaultThemeSettingsDriver : ContentPartDriver<KrakeDefaultThemeSettingsPart> {
        private readonly IThemeSettingsService _themeSettingsService;
        public KrakeDefaultThemeSettingsDriver(IThemeSettingsService themeSettingsService) {
            _themeSettingsService = themeSettingsService;
        }
        protected override void Exporting(KrakeDefaultThemeSettingsPart part, ExportContentContext context) {
            var settings = _themeSettingsService.GetSettings();
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("HeaderLogoUrl", settings.HeaderLogoUrl);
            root.SetAttributeValue("PlaceholderLogoUrl", settings.PlaceholderLogoUrl);
            root.SetAttributeValue("BaseLineText", settings.BaseLineText);
        }
        protected override void Importing(KrakeDefaultThemeSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            if (root == null) {
                return;
            }
            var settings = _themeSettingsService.GetSettings();
            settings.HeaderLogoUrl = root.Attribute("HeaderLogoUrl") != null ? root.Attribute("HeaderLogoUrl").Value : "";
            settings.PlaceholderLogoUrl = root.Attribute("PlaceholderLogoUrl") != null ? root.Attribute("PlaceholderLogoUrl").Value : "";
            settings.BaseLineText = root.Attribute("BaseLineText") != null ? root.Attribute("BaseLineText").Value : "";
        }
    }
}