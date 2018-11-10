using Laser.Orchard.GDPR.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.Settings {
    [OrchardFeature("Laser.Orchard.GDPR.ContentPickerFieldExtension")]
    public class ContentPickerFieldGDPRPartFieldEditorEvents : GDPRPartFieldEditorEventsBase {
        

        public ContentPickerFieldGDPRPartFieldEditorEvents(
            IContentDefinitionManager contentDefinitionManager)
            : base(contentDefinitionManager) {

        }
        
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (!_typeHasGDPRPart) {
                yield break;
            }
            //we only do stuff for ContentPickerFields
            if (definition.FieldDefinition.Name != "ContentPickerField") {
                yield break;
            }

            var settings = definition
                .Settings
                .GetModel<ContentPickerFieldGDPRPartFieldSettings>()
                ?? new ContentPickerFieldGDPRPartFieldSettings(); // avoid null settings
            
            yield return DefinitionTemplate(MakeViewModel(settings, definition.Name));
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(
            ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (!_typeHasGDPRPart) {
                yield break;
            }
            //we only do stuff for ContentPickerFields
            if (builder.FieldType != "ContentPickerField") {
                yield break;
            }

            var vm = new ContentPickerFieldGDPRPartFieldSettingsViewModel();
            if (updateModel.TryUpdateModel(vm, "ContentPickerFieldGDPRPartFieldSettingsViewModel", null, null)) {
                var settings = vm.Settings;
                ContentPickerFieldGDPRPartFieldSettings.SetValues(builder, settings);
            }
        }

        /// <summary>
        /// Create a ViewModel based on the settings provided. This method is really simple to begin
        /// with, to the point where it's not even really needed, but I create it already so that when
        /// the model becomes more complex I don't have to refactor code.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private ContentPickerFieldGDPRPartFieldSettingsViewModel MakeViewModel(
            ContentPickerFieldGDPRPartFieldSettings settings, string fieldName) {
            return new ContentPickerFieldGDPRPartFieldSettingsViewModel {
                Settings = settings,
                AnonymizationDivId = AnonymizationDivId(fieldName),
                ErasureDivId = ErasureDivId(fieldName)
            };
        }
    }
}