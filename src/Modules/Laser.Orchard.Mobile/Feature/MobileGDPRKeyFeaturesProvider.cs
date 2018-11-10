using Laser.Orchard.GDPR.Features;
using Orchard.Environment.Features;

namespace Laser.Orchard.Mobile.Feature {
    public class MobileGDPRKeyFeaturesProvider : BaseKeyFeaturesProvider {

        public MobileGDPRKeyFeaturesProvider(
            IFeatureManager featureManager) :
            base(featureManager) {

            KeyFeatureNames = new string[] {
                    "Laser.Orchard.GDPR.MobileExtension"
            };
        }
    }
}