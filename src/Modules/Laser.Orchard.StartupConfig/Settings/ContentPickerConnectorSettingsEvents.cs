using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Settings {
    
    [OrchardFeature("Laser.Orchard.StartupConfig.RelationshipsEnhancer")]
    public class ContentPickerConnectorSettingsEvents : ContentDefinitionEditorEventsBase {
        protected string Prefix {
            get { return "ContentPickerConnectorSettings"; }
        }
        public ContentPickerConnectorSettingsEvents() {
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ContentPickerConnectorPart") // parte di cui voglio definire i settings
                yield break;

            var model = definition.Settings.GetModel<ContentPickerConnectorSettings>();
            yield return DefinitionTemplate(model, "Parts/ContentPickerConnector.Settings", Prefix);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ContentPickerConnectorPart")
                yield break;

            var settings = new ContentPickerConnectorSettings();

            if (updateModel.TryUpdateModel(settings, Prefix, null, null)) {
                settings.Build(builder); // salva le impostazioni su db
                yield return DefinitionTemplate(settings, "Parts/ContentPickerConnectorPart.Settings", Prefix);
            }
        }
    }
}
