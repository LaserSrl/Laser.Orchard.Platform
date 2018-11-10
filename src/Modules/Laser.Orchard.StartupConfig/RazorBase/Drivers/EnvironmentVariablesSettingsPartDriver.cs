using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class EnvironmentVariablesSettingsPartDriver : ContentPartDriver<EnvironmentVariablesSettingsPart> {
        private const string TEMPLATENAME = "Parts/EnvironmentVariablesSettings";
        private const string SHAPENAME = "Parts_EnvironmentVariablesSettings_Edit";

        public EnvironmentVariablesSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "EnvironmentVariablesSettings"; } }

        protected override DriverResult Editor(EnvironmentVariablesSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(EnvironmentVariablesSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(SHAPENAME, () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TEMPLATENAME, Model: part, Prefix: Prefix);
            })
                .OnGroup("Razor");
        }
    }
}