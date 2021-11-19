using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Laser.Orchard.StartupConfig.ShortCodes.Settings.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Laser.Orchard.StartupConfig.ShortCodes.Settings {
    public class ContentShortCodeSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "BodyPart") //TODO: Add Fields here
                yield break;

            var model = definition.Settings.GetModel<ContentShortCodeSettings>();
            yield return DefinitionTemplate(model, "ShortCodes/ContentShortCodeSettings", "ContentShortCodeSettings");
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "BodyPart") //TODO: Add Fields here
                yield break;

            var model = new ContentShortCodeSettings();
            updateModel.TryUpdateModel(model, "ContentShortCodeSettings", null, null);
            builder.WithSetting("ContentShortCodeSettings.DisplayedContentTypes", model.DisplayedContentTypes);
            builder.WithSetting("ContentShortCodeSettings.Enabled", model.Enabled.ToString());

            yield return DefinitionTemplate(model);
        }
    }
}
