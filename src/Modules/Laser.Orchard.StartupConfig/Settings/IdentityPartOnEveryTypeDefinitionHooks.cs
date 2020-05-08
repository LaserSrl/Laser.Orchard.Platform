using Orchard.ContentManagement.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentTypes.Events;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Laser.Orchard.StartupConfig.Settings {
    public class IdentityPartOnEveryTypeContentDefinitionEventHandler : ContentDefinitionEditorEventsBase, IContentDefinitionEventHandler {
        //We want all our ContentTypes top have an identity, to allwo proper cloning/importing/exporting
        //Some parts contribute to an item's identity, so if those are in the type we don't need to add an identity part
        private readonly string[] PartsWithIdentity = { "IdentityPart", "AutoroutePart", "UserPart" };

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;

        public IdentityPartOnEveryTypeContentDefinitionEventHandler(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices) {

            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        private Localizer T;

        private bool TypeHasIdentity(string typeName) {
            return TypeHasIdentity(_contentDefinitionManager.GetTypeDefinition(typeName));
        }
        private bool TypeHasIdentity(ContentTypeDefinition definition) {
            return definition
                .Parts.Any(pa =>
                    PartsWithIdentity.Contains(pa.PartDefinition.Name));
        }
        private void AddIdentityToType(string typeName) {
            _contentDefinitionManager
                .AlterTypeDefinition(typeName, builder => {
                    builder.WithIdentity();
                });
            _orchardServices.Notifier.Information(T("The content type must have a form of identity. IdentityPart was added."));
        }
        private void EnsureIdentity(ContentTypePartContext context) {
            if (!TypeHasIdentity(context.ContentTypeName)) {
                //if the type has no identity, we add one back in
                AddIdentityToType(context.ContentTypeName);
            }
        }
        private void EnsureIdentity(ContentTypeContext context) {
            if (!TypeHasIdentity(context.ContentTypeDefinition)) {
                AddIdentityToType(context.ContentTypeDefinition.Name);
            }
        }

        public void ContentPartDetached(ContentPartDetachedContext context) {
            EnsureIdentity(context);
        }
        
        public void ContentTypeCreated(ContentTypeCreatedContext context) {
            EnsureIdentity(context);
        }

        public void ContentTypeImported(ContentTypeImportedContext context) {
            EnsureIdentity(context);
        }

        public override void TypeEditorUpdating(ContentTypeDefinitionBuilder builder) {
            if (!TypeHasIdentity(builder.Current)) {
                builder.WithPart("IdentityPart");
            }
        }

        #region not implemented interface methods
        public void ContentFieldAttached(ContentFieldAttachedContext context) { }

        public void ContentFieldDetached(ContentFieldDetachedContext context) { }

        public void ContentPartAttached(ContentPartAttachedContext context) { }

        public void ContentPartCreated(ContentPartCreatedContext context) { }

        public void ContentPartImported(ContentPartImportedContext context) { }

        public void ContentPartImporting(ContentPartImportingContext context) { }

        public void ContentTypeImporting(ContentTypeImportingContext context) { }

        public void ContentTypeRemoved(ContentTypeRemovedContext context) { }

        public void ContentPartRemoved(ContentPartRemovedContext context) { }
        #endregion
    }
}