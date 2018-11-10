using Laser.Orchard.CommunicationGateway.Services;
using Orchard.Environment;

namespace Laser.Orchard.CommunicationGateway.Features {

    public class CommunicationFeature : IFeatureEventHandler {
        private readonly ICommunicationService _communicationService;

        public CommunicationFeature(ICommunicationService communicationService) {
            _communicationService = communicationService;
        }

        public void Disabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Disabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //throw new NotImplementedException();
        }

        public void Enabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            //if (feature.Descriptor.Id == "Laser.Orchard.CommunicationGateway") {
            //    _communicationService.Synchronize();
            //}
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