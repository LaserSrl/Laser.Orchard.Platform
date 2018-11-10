using Laser.Orchard.GDPR.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Settings {
    public class GDPRPartPartEditorEvents : ContentDefinitionEditorEventsBase {

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INotifier _notifier;

        public GDPRPartPartEditorEvents(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier) {

            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            // do stuff only if there is a GDPRPart, but not if this is a GDPRPart
            if (definition.PartDefinition.Name == "GDPRPart") {
                yield break;
            }

            if (!definition.ContentTypeDefinition.Parts.Any(ctpd =>
                ctpd.PartDefinition.Name == "GDPRPart")) {
                yield break;
            }

            // the type has a GDPRPart, and it is not the current part
            var settings = definition
                .Settings
                .GetModel<GDPRPartPartSettings>()
                ?? new GDPRPartPartSettings(); // avoid null settings

            yield return DefinitionTemplate(MakeViewModel(settings));
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            // we have to do stuff only if the type has a GDPRPart, and we are not processing 
            // the GDPRPart right now
            if (builder.Name == "GDPRPart") {
                yield break;
            }

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(builder.TypeName);
            if (!typeDefinition.Parts.Any(ctpd =>
                ctpd.PartDefinition.Name == "GDPRPart")) {
                yield break;
            }

            // If the update fails for any part, it will be marked as failed for all of them, because the
            // UpdateModel will contain errors. The issue is of course with the dictionaries that contain
            // the Property-Value pairs for the handlers that use reflection.
            var vm = new GDPRPartPartSettingsViewModel();
            if (updateModel.TryUpdateModel(vm, "GDPRPartPartSettingsViewModel", null, null)) {
                var settings = vm.Settings;
                GDPRPartPartSettings.SetValues(builder, settings);
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
        private GDPRPartPartSettingsViewModel MakeViewModel(GDPRPartPartSettings settings) {
            return new GDPRPartPartSettingsViewModel { Settings = settings };
        }
    }
}