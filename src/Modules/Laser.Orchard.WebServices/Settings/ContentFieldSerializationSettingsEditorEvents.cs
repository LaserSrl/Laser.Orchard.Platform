using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.WebServices.Settings {
    public class ContentFieldSerializationSettingsEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public ContentFieldSerializationSettingsEditorEvents(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            var settings = definition.Settings.GetModel<ContentFieldSerializationSettings>();
            
            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var settings = new ContentFieldSerializationSettings();
            if (updateModel.TryUpdateModel(settings, "ContentFieldSerializationSettings", null, null)) {
                ContentFieldSerializationSettings.SetValues(builder, settings.AllowSerialization);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}