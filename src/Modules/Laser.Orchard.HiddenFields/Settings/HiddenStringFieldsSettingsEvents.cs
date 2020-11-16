using Laser.Orchard.HiddenFields.Services;
using Laser.Orchard.HiddenFields.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Settings {
    public class HiddenStringFieldsSettingsEvents : ContentDefinitionEditorEventsBase {

        private readonly IHiddenStringFieldUpdateProcessor _updateProcessor;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public HiddenStringFieldsSettingsEvents(
            IHiddenStringFieldUpdateProcessor updateProcessor,
            IContentDefinitionManager contentDefinitionManager) {

            _updateProcessor = updateProcessor;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(
            ContentPartFieldDefinition definition){

                if (definition.FieldDefinition.Name == "HiddenStringField") {
                    var model = new HiddenStringFieldSettingsEventsViewModel {
                        Settings = definition.Settings.GetModel<HiddenStringFieldSettings>(),
                        ProcessVariant = HiddenStringFieldUpdateProcessVariant.None,
                        ProcessVariants = _updateProcessor.GetVariants()
                    };
                    yield return DefinitionTemplate(model);
                } else {
                    yield break;
                }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(
            ContentPartFieldDefinitionBuilder builder,IUpdateModel updateModel) {
                
            if (builder.FieldType == "HiddenStringField") {
                var model = new HiddenStringFieldSettingsEventsViewModel {
                    Settings = new HiddenStringFieldSettings(),
                    ProcessVariant = HiddenStringFieldUpdateProcessVariant.None,
                    ProcessVariants = _updateProcessor.GetVariants()
                };
                if (updateModel.TryUpdateModel(model, "HiddenStringFieldSettingsEventsViewModel", null, null)) {
                    builder.WithSetting("HiddenStringFieldSettings.Tokenized", model.Settings.Tokenized.ToString());
                    builder.WithSetting("HiddenStringFieldSettings.TemplateString", model.Settings.TemplateString);
                    builder.WithSetting("HiddenStringFieldSettings.AutomaticAdjustmentOnEdit", model.Settings.AutomaticAdjustmentOnEdit.ToString());

                    _updateProcessor.AddTask(model.ProcessVariant, model.Settings, builder);

                    model.ProcessVariant = HiddenStringFieldUpdateProcessVariant.None;
                    yield return DefinitionTemplate(model);
                }
            } else {
                yield break;
            }
        }
    }
}