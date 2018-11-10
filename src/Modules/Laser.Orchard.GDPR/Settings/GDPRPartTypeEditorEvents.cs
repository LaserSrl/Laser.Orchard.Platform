using Laser.Orchard.GDPR.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.Settings {
    public class GDPRPartTypeEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            // the settings make sense only for GDPRPart
            if (definition.PartDefinition.Name != "GDPRPart") {
                yield break;
            }

            var settings = definition
                .Settings
                .GetModel<GDPRPartTypeSettings>()
                ?? new GDPRPartTypeSettings(); //ensure that settings != null

            yield return DefinitionTemplate(MakeViewModel(settings));
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            // working on GDPRPart
            if (builder.Name != "GDPRPart") {
                yield break;
            }

            var vm = new GDPRPartTypeSettingsViewModel();
            if (updateModel.TryUpdateModel(vm, "GDPRPartTypeSettingsViewModel", null, null)) {
                var settings = vm.Settings;
                GDPRPartTypeSettings.SetValues(builder, settings);
            }
        }

        /// <summary>
        /// Create a ViewModel based on the settings provided. This method is really simple to begin
        /// with, to the point where it's not even really needed, but I create it already so that when
        /// the model becomes more complex I don't have to refactor code.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private GDPRPartTypeSettingsViewModel MakeViewModel(GDPRPartTypeSettings settings) {
            return new GDPRPartTypeSettingsViewModel { Settings = settings };
        }
    }
}