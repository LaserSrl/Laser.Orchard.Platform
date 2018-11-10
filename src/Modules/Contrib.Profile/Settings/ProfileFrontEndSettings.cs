using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using System.Globalization;

namespace Contrib.Profile.Settings {
    public class ProfileFrontEndSettings {
        public bool AllowFrontEndEdit { get; set; } 
        public bool AllowFrontEndDisplay { get; set; }
        
        public static void SetValues(ContentTypePartDefinitionBuilder builder, bool allowDisplay, bool allowEdit) {
            builder.WithSetting("ProfileFrontEndSettings.AllowFrontEndEdit", allowEdit.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("ProfileFrontEndSettings.AllowFrontEndDisplay", allowDisplay.ToString(CultureInfo.InvariantCulture));
        }

        public static void SetValues(ContentPartFieldDefinitionBuilder builder, bool allowDisplay, bool allowEdit) {
            builder.WithSetting("ProfileFrontEndSettings.AllowFrontEndEdit", allowEdit.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("ProfileFrontEndSettings.AllowFrontEndDisplay", allowDisplay.ToString(CultureInfo.InvariantCulture));
        }
    }
}