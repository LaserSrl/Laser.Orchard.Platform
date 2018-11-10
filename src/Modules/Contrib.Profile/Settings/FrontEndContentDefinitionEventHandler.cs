using Contrib.Profile.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Events;
using System.Collections.Generic;
using System.Linq;

namespace Contrib.Profile.Settings {
    /// <summary>
    /// This handles default settings related to front-end display/edit for several parts
    /// for which we know the desired behaviour.
    /// </summary>
    public class FrontEndContentDefinitionEventHandler : IContentDefinitionEventHandler {

        private readonly IEnumerable<IDefaultFrontEndSettingsProvider> _frontEndSettingsProviders;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public FrontEndContentDefinitionEventHandler(
            IEnumerable<IDefaultFrontEndSettingsProvider> frontEndSettingsProviders,
            IContentDefinitionManager contentDefinitionManager) {

            _frontEndSettingsProviders = frontEndSettingsProviders;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public void ContentPartAttached(ContentPartAttachedContext context) {
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentTypeName);
            if (context.ContentPartName == "ProfilePart" || TypeHasProfilePart(typeDefinition)) {
                //see whether in the type there are any default settings to process
                //We only execute the providers for the part that was attached. This means we can
                //change the default settings of some, and they will not be reset here.
                foreach (var provider in _frontEndSettingsProviders
                    .Where(prov => prov.ForParts().Contains(context.ContentPartName))) {

                    provider.ConfigureDefaultValues(typeDefinition);
                }
            }
        }
        public void ContentTypeCreated(ContentTypeCreatedContext context) {
            if (TypeHasProfilePart(context.ContentTypeDefinition)) {
                //see whether in the type there are any default settings to process
                foreach (var provider in _frontEndSettingsProviders) {
                    provider.ConfigureDefaultValues(context.ContentTypeDefinition);
                }
            }
        }

        public void ContentTypeImported(ContentTypeImportedContext context) {
            if (TypeHasProfilePart(context.ContentTypeDefinition)) {
                //see whether in the type there are any default settings to process
                foreach (var provider in _frontEndSettingsProviders) {
                    provider.ConfigureDefaultValues(context.ContentTypeDefinition);
                }
            }
        }

        public void ContentFieldAttached(ContentFieldAttachedContext context) {
            //this handles a special condition: all fields in the ProfilePart should by default be set
            //to be displayed/editable in front-end.
            if (context.ContentPartName == "ProfilePart") {
                var typeDefinitions = _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(td => TypeHasProfilePart(td));
                foreach(var typeDefinition in typeDefinitions) {
                    foreach (var provider in _frontEndSettingsProviders
                        .Where(prov => prov.ForParts().Contains("ProfilePart"))) {

                        provider.ConfigureDefaultValues(typeDefinition, context.ContentFieldTypeName + "." + context.ContentFieldName);
                    }
                }
            }
        }

        private bool TypeHasProfilePart(ContentTypeDefinition definition) {
            return definition.Parts
                .Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart");
        }

        #region not implemented IContentDefinitionEventHandler methods

        public void ContentFieldDetached(ContentFieldDetachedContext context) { }
        
        public void ContentPartCreated(ContentPartCreatedContext context) { }

        public void ContentPartDetached(ContentPartDetachedContext context) { }

        public void ContentPartImported(ContentPartImportedContext context) { }

        public void ContentPartImporting(ContentPartImportingContext context) { }

        public void ContentPartRemoved(ContentPartRemovedContext context) { }
        
        public void ContentTypeImporting(ContentTypeImportingContext context) { }

        public void ContentTypeRemoved(ContentTypeRemovedContext context) { }
        #endregion
    }
}