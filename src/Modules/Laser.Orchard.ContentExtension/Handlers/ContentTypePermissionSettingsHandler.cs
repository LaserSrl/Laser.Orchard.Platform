using Laser.Orchard.ContentExtension.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.ContentExtension.Handlers {
    public class ContentTypePermissionSettingsHandler : ContentHandler {
        public ContentTypePermissionSettingsHandler() {
            Filters.Add(new ActivatingFilter<ContentTypePermissionSettingsPart>("Site"));
        }
    }
}