using Laser.Orchard.GDPR.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.Settings {
    [OrchardFeature("Laser.Orchard.GDPR.MediaExtension")]
    public class MediaLibraryPickerFieldGDPRPartFieldEditorEvents : GDPRPartFieldEditorEventsBase {

        public MediaLibraryPickerFieldGDPRPartFieldEditorEvents(
            IContentDefinitionManager contentDefinitionManager)
            : base(contentDefinitionManager) {

        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (!_typeHasGDPRPart) {
                yield break;
            }
            //we only do stuff for MediaLibraryPickerFields
            if (definition.FieldDefinition.Name != "MediaLibraryPickerField") {
                yield break;
            }

            var settings = definition
                .Settings
                .GetModel<MediaLibraryPickerFieldGDPRPartFieldSettings>()
                ?? new MediaLibraryPickerFieldGDPRPartFieldSettings(); // avoid null settings

            yield return DefinitionTemplate(MakeViewModel(settings, definition.Name));
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(
            ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (!_typeHasGDPRPart) {
                yield break;
            }
            //we only do stuff for MediaLibraryPickerFields
            if (builder.FieldType != "MediaLibraryPickerField") {
                yield break;
            }

            var vm = new MediaLibraryPickerFieldGDPRPartFieldSettingsViewModel();
            if (updateModel.TryUpdateModel(vm, "MediaLibraryPickerFieldGDPRPartFieldSettingsViewModel", null, null)) {
                var settings = vm.Settings;
                MediaLibraryPickerFieldGDPRPartFieldSettings.SetValues(builder, settings);
            }
        }

        /// <summary>
        /// Create a ViewModel based on the settings provided. This method is really simple to begin
        /// with, to the point where it's not even really needed, but I create it already so that when
        /// the model becomes more complex I don't have to refactor code.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private MediaLibraryPickerFieldGDPRPartFieldSettingsViewModel MakeViewModel(
            MediaLibraryPickerFieldGDPRPartFieldSettings settings, string fieldName) {
            return new MediaLibraryPickerFieldGDPRPartFieldSettingsViewModel {
                Settings = settings,
                AnonymizationDivId = AnonymizationDivId(fieldName),
                ErasureDivId = ErasureDivId(fieldName)
            };
        }
    }
}