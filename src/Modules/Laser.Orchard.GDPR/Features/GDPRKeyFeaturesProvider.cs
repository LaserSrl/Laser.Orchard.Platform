using Orchard.Environment.Features;

namespace Laser.Orchard.GDPR.Features {
    public class GDPRKeyFeaturesProvider : BaseKeyFeaturesProvider {

        public GDPRKeyFeaturesProvider(
            IFeatureManager featureManager) :
            base(featureManager) {

            KeyFeatureNames = new string[] {
                    "Laser.Orchard.GDPR.Workflows",
                    "Laser.Orchard.GDPR.ContentPickerFieldExtension",
                    "Laser.Orchard.GDPR.MediaExtension"
                };
        }
        

    }
}