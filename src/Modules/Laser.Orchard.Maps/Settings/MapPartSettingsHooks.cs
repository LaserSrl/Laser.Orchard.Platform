using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Maps.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Laser.Orchard.Maps.Settings {
    public class MapPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition) {

            if (definition.PartDefinition.Name != "MapPart") yield break;
            var model = definition.Settings.GetModel<MapPartSettings>();


            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "MapPart") yield break;

            var model = new MapPartSettings();
            updateModel.TryUpdateModel(model, "MapPartSettings", null, null);
            builder.WithSetting("MapPartSettings.Required",
            ((bool)model.Required).ToString());
            builder.WithSetting("MapPartSettings.HideMapSource",
            ((bool)model.HideMapSource).ToString());
            builder.WithSetting("MapPartSettings.HintText",
            model.HintText); 
            yield return DefinitionTemplate(model);
        }
    }
}