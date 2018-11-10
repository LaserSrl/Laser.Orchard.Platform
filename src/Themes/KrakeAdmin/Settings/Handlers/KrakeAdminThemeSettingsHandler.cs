using KrakeAdmin.Settings.Models;
using Orchard.ContentManagement.Handlers;

namespace KrakeAdmin.Settings.Handlers {
    public class KrakeAdminThemeSettingsHandler : ContentHandler {
        public KrakeAdminThemeSettingsHandler() {
            Filters.Add(new ActivatingFilter<KrakeAdminThemeSettingsPart>("Site"));
        }
    }
}