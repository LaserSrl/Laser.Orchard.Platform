using Laser.Orchard.StartupConfig.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.AuditTrail.Handlers {
    [OrchardFeature("Laser.Orchard.AuditTrail")]
    public class AuditTrailOutputSettingsPartHandler : ContentHandler {

        public AuditTrailOutputSettingsPartHandler() {

            Filters.Add(new ActivatingFilter<AuditTrailOutputSettingsPart>("Site"));

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private void GetMetadata(GetContentItemMetadataContext context, AuditTrailOutputSettingsPart part) {
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Audit Trail")));
        }
    }
}