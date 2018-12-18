using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Laser.Orchard.PrivateMedia.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.Fields;
using Orchard.MediaLibrary.Models;

namespace Laser.Orchard.PrivateMedia.Handlers {

    [OrchardSuppressDependency("Orchard.MediaLibrary.Handlers.MediaLibraryPickerFieldHandler")]
    public class PrivateMediaLibraryPickerFieldHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public PrivateMediaLibraryPickerFieldHandler(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
        }

        protected override void Loaded(LoadContentContext context) {
            base.Loaded(context);
            InitilizeLoader(context.ContentItem);
        }

        private void InitilizeLoader(ContentItem contentItem) {
            var fields = contentItem.Parts.SelectMany(x => x.Fields.OfType<MediaLibraryPickerField>());
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null) {
                return;
            }
            bool filtroprivate = false;
            if (!_orchardServices.Authorizer.Authorize(PrivateMediaPermissions.AccessAllPrivateMedia)) {
                filtroprivate = true;
            }
            foreach (var field in fields) {
                var localField = field;
                Lazy<IEnumerable<MediaPart>> value;
                 if (filtroprivate)
                    value = new Lazy<IEnumerable<MediaPart>>(() => _contentManager.GetMany<MediaPart>(localField.Ids, VersionOptions.Published, QueryHints.Empty).Where(x => !x.ContentItem.As<PrivateMediaPart>().IsPrivate).ToList());
                else
                    value = new Lazy<IEnumerable<MediaPart>>(() => _contentManager.GetMany<MediaPart>(localField.Ids, VersionOptions.Published, QueryHints.Empty).ToList());
                var fInfo = field.GetType().GetField("_contentItems", BindingFlags.NonPublic | BindingFlags.Instance);
                fInfo.SetValue(field, value);
            }
        }
    }
}