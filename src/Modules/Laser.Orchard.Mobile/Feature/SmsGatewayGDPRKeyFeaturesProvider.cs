using Laser.Orchard.GDPR.Features;
using Orchard.Environment.Features;

namespace Laser.Orchard.Mobile.Feature {
    public class SmsGatewayGDPRKeyFeaturesProvider : BaseKeyFeaturesProvider {

        public SmsGatewayGDPRKeyFeaturesProvider(
            IFeatureManager featureManager) :
            base(featureManager) {

            KeyFeatureNames = new string[] {
                    "Laser.Orchard.GDPR.SmsGatewayExtension"
            };
        }
    }
}