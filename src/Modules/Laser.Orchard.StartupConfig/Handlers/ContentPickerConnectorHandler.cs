using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.Settings;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Handlers {
    [OrchardFeature("Laser.Orchard.StartupConfig.RelationshipsEnhancer")]
    public class ContentPickerConnectorHandler : ContentHandler {
        IContentExtensionsServices _contenExtensionsServices;

        public ContentPickerConnectorHandler(IContentExtensionsServices contenExtensionsServices) {
            _contenExtensionsServices = contenExtensionsServices;

        }
        protected override void Loading(LoadContentContext context) {
            base.Loading(context);
            var connector = context.ContentItem.Parts.Where(w => w.PartDefinition.Name == typeof(ContentPickerConnectorPart).Name).Cast<ContentPickerConnectorPart>().SingleOrDefault();
            if (connector == null) return;
            var fieldsFilterSettings = connector.Settings.GetModel<ContentPickerConnectorSettings>().FieldsFilter;
            if (fieldsFilterSettings != null) {
                var contentTypesFilter = fieldsFilterSettings.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                if (contentTypesFilter.Length > 0)
                    connector._parentContent.Loader(() => _contenExtensionsServices.ContentPickerParents(context.ContentItem.Id, contentTypesFilter));
                else
                    return;
            }
        }

    }
}