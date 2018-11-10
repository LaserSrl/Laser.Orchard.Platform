using Laser.Orchard.GDPR.Features;
using Orchard.Environment.Features;

namespace Laser.Orchard.Mobile.Feature {
    public class PushGatewayGDPRKeyFeaturesProvider : BaseKeyFeaturesProvider {

        public PushGatewayGDPRKeyFeaturesProvider(
            IFeatureManager featureManager) :
            base(featureManager) {

            KeyFeatureNames = new string[] {
                    "Laser.Orchard.GDPR.PushGatewayExtension"
            };
        }
    }
}