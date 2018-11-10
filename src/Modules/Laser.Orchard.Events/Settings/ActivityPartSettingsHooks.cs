using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Events.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Laser.Orchard.Events.Settings {
    public class ActivityPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition) {

            if (definition.PartDefinition.Name != "ActivityPart") yield break;
            var model = definition.Settings.GetModel<ActivityPartSettings>();


            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "ActivityPart") yield break;

            var model = new ActivityPartSettings();
            updateModel.TryUpdateModel(model, "ActivityPartSettings", null, null);
            builder.WithSetting("ActivityPartSettings.UseRecurrences",
            ((bool)model.UseRecurrences).ToString());
            builder.WithSetting("ActivityPartSettings.SingleDate",
            ((bool)model.SingleDate).ToString());
            builder.WithSetting("ActivityPartSettings.DateTimeType",
            ((DateTimeTypes)model.DateTimeType).ToString());
            builder.WithSetting("ActivityPartSettings.DateStartLabel",
                model.DateStartLabel != null ? model.DateStartLabel.ToString() : "");
            builder.WithSetting("ActivityPartSettings.DateEndLabel",
                model.DateEndLabel != null ? model.DateEndLabel.ToString() : "");
            builder.WithSetting("ActivityPartSettings.ActivityPartLabel",
                model.ActivityPartLabel != null ? model.ActivityPartLabel.ToString() : "");
            yield return DefinitionTemplate(model);
        }
    }
}
