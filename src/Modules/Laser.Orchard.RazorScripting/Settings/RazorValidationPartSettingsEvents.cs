using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using System.Collections.Generic;

namespace Laser.Orchard.RazorScripting.Settings {

    public class RazorValidationPartSettingsEvents : ContentDefinitionEditorEventsBase {
        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "RazorValidationPart")
                yield break;

            var settings = definition.Settings.GetModel<RazorValidationPartSettings>();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "RazorValidationPart")
                yield break;

            var settings = new RazorValidationPartSettings();

            if (updateModel.TryUpdateModel(settings, "RazorValidationPartSettings", null, null)) {
                builder.WithSetting("RazorValidationPartSettings.Script", settings.Script);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}