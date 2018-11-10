using Laser.Orchard.GDPR.Features;
using Orchard.Environment.Features;

namespace Laser.Orchard.CommunicationGateway.Features {
    public class ContactGDPRKeyFeaturesProvider : BaseKeyFeaturesProvider {

        public ContactGDPRKeyFeaturesProvider(
            IFeatureManager featureManager) :
            base(featureManager) {
            KeyFeatureNames = new string[] {
                "Laser.Orchard.GDPR.ContactExtension"
                };
        }

    }
}