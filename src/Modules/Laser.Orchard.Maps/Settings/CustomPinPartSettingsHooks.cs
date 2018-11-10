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
using Newtonsoft.Json;
using Orchard.Services;

namespace Laser.Orchard.Maps.Settings {
    public class CustomPinPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition) {

            if (definition.PartDefinition.Name != "CustomPinPart") yield break;
            var model = definition.Settings.GetModel<CustomPinPartSettings>();


            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "CustomPinPart") yield break;

            var model = new CustomPinPartSettings();
            updateModel.TryUpdateModel(model, "CustomPinPartSettings", null, null);
            builder.WithSetting("CustomPinPartSettings.ReplacedColor",
            model.ReplacedColor);
            builder.WithSetting("CustomPinPartSettings.AlternateColor",
            model.AlternateColor);
            builder.WithSetting("CustomPinPartSettings.PinUrl",
            model.PinUrl);
            builder.WithSetting("CustomPinPartSettings.WaterMarkUrl",
            model.WaterMarkUrl);
            builder.WithSetting("CustomPinPartSettings.TargetAreaDefinitions",
            new DefaultJsonConverter().Serialize(model.TargetArea));
            yield return DefinitionTemplate(model);
        }
    }
}