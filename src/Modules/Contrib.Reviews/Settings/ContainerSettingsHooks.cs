using System.Collections.Generic;

using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Contrib.Reviews.Settings {
    
    public class ContainerSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ReviewsPart")
                yield break;

            var model = definition.Settings.GetModel<ReviewTypePartSettings>();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ReviewsPart")
                yield break;

            var model = new ReviewTypePartSettings();
            updateModel.TryUpdateModel(model, "ReviewTypePartSettings", null, null);
            builder.WithSetting("ReviewTypePartSettings.ShowStars", model.ShowStars.ToString());

            yield return DefinitionTemplate(model);
        }
    }
}