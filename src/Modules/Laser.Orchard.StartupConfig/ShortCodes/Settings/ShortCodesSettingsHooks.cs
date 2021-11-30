using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using Laser.Orchard.StartupConfig.ShortCodes.Settings.Models;
using Laser.Orchard.StartupConfig.ShortCodes.Settings.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.ShortCodes.Settings {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ShortCodesSettingsHooks : ContentDefinitionEditorEventsBase {
        private readonly IEnumerable<IShortCodeProvider> _shortCodeProviders;

        public ShortCodesSettingsHooks(IEnumerable<IShortCodeProvider> shortCodeProviders) {
            _shortCodeProviders = shortCodeProviders;
        }
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name != "TextField")
                yield break;

            var model = new ShortCodesSettingsViewModel();
            model.Populate(definition.Settings.GetModel<ShortCodesSettings>(), _shortCodeProviders);
            yield return DefinitionTemplate(model, "ShortCodes/ShortCodesSettings", "ShortCodesSettings");
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "TextField")
                yield break;

            var model = new ShortCodesSettingsViewModel();
            updateModel.TryUpdateModel(model, "ShortCodesSettings", null, null);
            builder.WithSetting("ShortCodesSettings.EnabledShortCodes", string.Join(",", model.EnabledShortCodes));
            yield return DefinitionTemplate(model);
        }


        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "BodyPart") 
                yield break;

            var model = new ShortCodesSettingsViewModel();
            model.Populate(definition.Settings.GetModel<ShortCodesSettings>(), _shortCodeProviders);
            yield return DefinitionTemplate(model, "ShortCodes/ShortCodesSettings", "ShortCodesSettings");
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "BodyPart") 
                yield break;

            var model = new ShortCodesSettingsViewModel();
            updateModel.TryUpdateModel(model, "ShortCodesSettings", null, null);
            builder.WithSetting("ShortCodesSettings.EnabledShortCodes", string.Join(",",model.EnabledShortCodes));
            yield return DefinitionTemplate(model);
        }
    }
}
