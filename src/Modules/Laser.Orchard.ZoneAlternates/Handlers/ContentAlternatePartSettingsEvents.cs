using Laser.Orchard.ZoneAlternates.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ZoneAlternates.Handlers {
    public class ContentAlternatePartSettingsEvents : ContentDefinitionEditorEventsBase {

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition) {

            if (definition.PartDefinition.Name != "ContentAlternatePart") yield break;
            var model = definition.Settings.GetModel<ContentAlternatePartSettings>();


            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "ContentAlternatePart") yield break;

            var model = new ContentAlternatePartSettings();
            updateModel.TryUpdateModel(model, "ContentAlternatePartSettings", null, null);
            builder.WithSetting("ContentAlternatePartSettings.AlternateNames", model.AlternateNames);
            yield return DefinitionTemplate(model);
        }
    }
}