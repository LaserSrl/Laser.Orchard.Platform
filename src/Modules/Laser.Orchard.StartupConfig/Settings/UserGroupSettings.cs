using Laser.Orchard.StartupConfig.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Settings {
       [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UserGroupSettings {
           public bool Required { get; set; }
    }
}