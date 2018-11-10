using KrakeDefaultTheme.Settings.Models;
using Orchard.ContentManagement.Handlers;

namespace KrakeDefaultTheme.Settings.Handlers {
    public class KrakeDefaultThemeSettingsHandler : ContentHandler {
        public KrakeDefaultThemeSettingsHandler() {
            Filters.Add(new ActivatingFilter<KrakeDefaultThemeSettingsPart>("Site"));
        }
    }
}