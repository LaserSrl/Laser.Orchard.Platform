using Laser.Orchard.GDPR.Features;
using Orchard.Environment.Features;

namespace Laser.Orchard.OpenAuthentication.Features {
    public class OpenAuthKeyFeaturesProvider : BaseKeyFeaturesProvider {
        public OpenAuthKeyFeaturesProvider(
            IFeatureManager featureManager) :
            base(featureManager) {

            KeyFeatureNames = new string[] {
                    "Laser.Orchard.GDPR.OpenAuthExtension"
                };
        }
    }
}