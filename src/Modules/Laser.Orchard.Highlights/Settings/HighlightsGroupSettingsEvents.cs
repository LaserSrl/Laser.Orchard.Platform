using System.Collections.Generic;
using Laser.Orchard.Highlights.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Highlights.Settings {

    public class HighlightsGroupSettingsEvents : ContentDefinitionEditorEventsBase {
        private readonly IHighlightsService _highlightsService;
        protected string Prefix {
            get { return "HighlightsGroupSettings"; }
        }
        public HighlightsGroupSettingsEvents(IHighlightsService highlightsService) {
            _highlightsService = highlightsService;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "HighlightsGroupPart") // parte di cui voglio definire i settings
                yield break;

            var model = definition.Settings.GetModel<HighlightsGroupSettings>();
            model.AvailablePlugins = _highlightsService.GetAvailablePlugins();
            yield return DefinitionTemplate(model, "Parts/HighlightsGroup.Settings", Prefix);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "HighlightsGroupPart")
                yield break;

            var settings = new HighlightsGroupSettings();

            if (updateModel.TryUpdateModel(settings, Prefix, null, null)) {
                settings.Build(builder);
                settings.AvailablePlugins = _highlightsService.GetAvailablePlugins();

                yield return DefinitionTemplate(settings, "Parts/HighlightsGroup.Settings", Prefix);
            }
        }
    }
}
