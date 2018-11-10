using Laser.Orchard.GDPR.Features;
using Orchard.Environment.Features;

namespace Laser.Orchard.Mobile.Feature {
    public class SmsExtensionGDPRKeyFeaturesProvider : BaseKeyFeaturesProvider {

        public SmsExtensionGDPRKeyFeaturesProvider(
            IFeatureManager featureManager) :
            base(featureManager) {

            KeyFeatureNames = new string[] {
                    "Laser.Orchard.GDPR.SmsExtension"
            };
        }
    }
}