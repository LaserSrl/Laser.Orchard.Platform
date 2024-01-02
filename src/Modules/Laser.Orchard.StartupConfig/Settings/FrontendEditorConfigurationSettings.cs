using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using System.Globalization;

namespace Laser.Orchard.StartupConfig.Settings {
    [OrchardFeature("Laser.Orchard.StartupConfig.FrontendEditorConfiguration")]
    public class FrontendEditorConfigurationSettings {
        public bool AllowFrontEndEdit { get; set; }
        public static void SetValues(ContentTypePartDefinitionBuilder builder, bool allowEdit) {
            builder.WithSetting("FrontendEditorConfigurationSettings.AllowFrontEndEdit", 
                allowEdit.ToString(CultureInfo.InvariantCulture));
        }

        public static void SetValues(ContentPartFieldDefinitionBuilder builder, bool allowEdit) {
            builder.WithSetting("FrontendEditorConfigurationSettings.AllowFrontEndEdit", 
                allowEdit.ToString(CultureInfo.InvariantCulture));
        }
    }
}