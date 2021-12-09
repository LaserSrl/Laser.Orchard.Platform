using Laser.Orchard.StartupConfig.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Settings;

namespace Laser.Orchard.StartupConfig.AuditTrail.Features {
    [OrchardFeature("Laser.Orchard.AuditTrail")]
    public class AuditTrailFeatureEventHandler : IFeatureEventHandler {

        private readonly ISiteService _siteService;

        public AuditTrailFeatureEventHandler(
            ISiteService siteService) {

            _siteService = siteService;
        }


        public void Enabled(Feature feature) {
            if (feature.Descriptor.Id == "Laser.Orchard.AuditTrail") {
                var site = _siteService.GetSiteSettings();
                if (site != null) {
                    var part = site.As<AuditTrailOutputSettingsPart>();
                    if (part != null) {
                        part.EventViewerSourceName = site.SiteName;
                    }
                }
            }
        }

        #region Not used interface method
        public void Disabled(Feature feature) {
        }

        public void Disabling(Feature feature) {
        }

        public void Enabling(Feature feature) {
        }

        public void Installed(Feature feature) {
        }

        public void Installing(Feature feature) {
        }

        public void Uninstalled(Feature feature) {
        }

        public void Uninstalling(Feature feature) {
        }
        #endregion
    }
}