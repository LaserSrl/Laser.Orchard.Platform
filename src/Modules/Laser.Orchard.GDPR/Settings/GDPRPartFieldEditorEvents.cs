using Laser.Orchard.GDPR.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.UI.Notify;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.Settings {
    public class GDPRPartFieldEditorEvents : GDPRPartFieldEditorEventsBase {
        
        private readonly INotifier _notifier;

        public GDPRPartFieldEditorEvents(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier) 
            : base(contentDefinitionManager){
            
            _notifier = notifier;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (!_typeHasGDPRPart) {
                yield break;
            }

            var settings = definition
                .Settings
                .GetModel<GDPRPartFieldSettings>() 
                ?? new GDPRPartFieldSettings(); // avoid null settings

            yield return DefinitionTemplate(MakeViewModel(settings, definition.Name));
        }


        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (!_typeHasGDPRPart) {
                yield break;
            }

            var vm = new GDPRPartFieldSettingsViewModel();
            if (updateModel.TryUpdateModel(vm, "GDPRPartFieldSettingsViewModel", null, null)) {
                var settings = vm.Settings;
                GDPRPartFieldSettings.SetValues(builder, settings);
            } else {
                _notifier.Error(T("There was an issue updating the GDPR configuration for {0}.", builder.Name));
                yield break;
            }
        }

        /// <summary>
        /// Create a ViewModel based on the settings provided. This method is really simple to begin
        /// with, to the point where it's not even really needed, but I create it already so that when
        /// the model becomes more complex I don't have to refactor code.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private GDPRPartFieldSettingsViewModel MakeViewModel(
            GDPRPartFieldSettings settings, string fieldName) {
            return new GDPRPartFieldSettingsViewModel {
                Settings = settings,
                AnonymizationDivId = AnonymizationDivId(fieldName),
                ErasureDivId = ErasureDivId(fieldName)
            };
        }
    }
}