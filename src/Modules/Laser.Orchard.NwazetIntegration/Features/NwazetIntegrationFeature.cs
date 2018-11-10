using Orchard.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions.Models;
using Orchard.ContentManagement.MetaData;

namespace Laser.Orchard.NwazetIntegration.Features {
    public class NwazetIntegrationFeature : IFeatureEventHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public NwazetIntegrationFeature(
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;
        }

        public void Enabled(Feature feature) {
            if (feature.Descriptor.Id == "Laser.Orchard.NwazetIntegration") {
                //attach part to CommunicationContact
                if (!_contentDefinitionManager.GetTypeDefinition("CommunicationContact")
                        .Parts.Any(pa => pa.PartDefinition.Name == "NwazetContactPart")) {
                    _contentDefinitionManager.AlterTypeDefinition("CommunicationContact", builder => {
                        builder.WithPart("NwazetContactPart");
                    });
                }
            }
        }

        #region Not implemented portion of IFeatureEventHandler
        public void Disabled(Feature feature) { }

        public void Disabling(Feature feature) { }
        
        public void Enabling(Feature feature) { }

        public void Installed(Feature feature) { }

        public void Installing(Feature feature) { }

        public void Uninstalled(Feature feature) { }

        public void Uninstalling(Feature feature) { }
        #endregion
    }
}