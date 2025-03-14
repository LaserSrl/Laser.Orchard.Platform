using Laser.Orchard.Mobile.Services;
using Orchard.Environment;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Feature {

    [OrchardFeature("Laser.Orchard.MobileCommunicationImport")]
    public class MobileCommunicationImportFeature : IFeatureEventHandler {
 
        private readonly IPushNotificationService _pushNotificationService;


        public MobileCommunicationImportFeature(IPushNotificationService pushNotificationService) {

            _pushNotificationService = pushNotificationService;
   
        }

        public void Disabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Disabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Enabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            if (feature.Descriptor.Id == "Laser.Orchard.MobileCommunicationImport") {
                _pushNotificationService.Synchronize();
            }
        }

        public void Enabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Installed(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Installing(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Uninstalled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Uninstalling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }
    }
}