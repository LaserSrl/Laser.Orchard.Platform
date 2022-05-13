using Laser.Orchard.HiddenFields.Handlers;
using Laser.Orchard.HiddenFields.Services;
using Laser.Orchard.HiddenFields.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using Orchard.Services;
using Orchard.Tasks.Scheduling;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.HiddenFields.Settings {
    public class HiddenStringFieldsSettingsEvents : ContentDefinitionEditorEventsBase {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private readonly IClock _clock;
        public Localizer T;

        private string _contentTypeName;

        public HiddenStringFieldsSettingsEvents(
            IContentDefinitionManager contentDefinitionManager,
            IScheduledTaskManager scheduledTaskManager,
            IClock clock) {

            _contentDefinitionManager = contentDefinitionManager;
            _scheduledTaskManager = scheduledTaskManager;
            _clock = clock;

            T = NullLocalizer.Instance;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(
            ContentPartFieldDefinition definition){

                if (definition.FieldDefinition.Name == "HiddenStringField") {
                    var model = new HiddenStringFieldSettingsEventsViewModel {
                        Settings = definition.Settings.GetModel<HiddenStringFieldSettings>(),
                        ProcessVariant = HiddenStringFieldUpdateProcessVariant.None,
                        ProcessVariants = GetVariants()
                    };
                    yield return DefinitionTemplate(model);
                } else {
                    yield break;
                }
        }

        public override void TypeEditorUpdated(ContentTypeDefinitionBuilder builder) {
            _contentTypeName = builder.Current.Name;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(
            ContentPartFieldDefinitionBuilder builder,IUpdateModel updateModel) {
                
            if (builder.FieldType == "HiddenStringField") {
                var model = new HiddenStringFieldSettingsEventsViewModel {
                    Settings = new HiddenStringFieldSettings(),
                    ProcessVariant = HiddenStringFieldUpdateProcessVariant.None,
                    ProcessVariants = GetVariants()
                };
                if (updateModel.TryUpdateModel(model, "HiddenStringFieldSettingsEventsViewModel", null, null)) {
                    builder.WithSetting("HiddenStringFieldSettings.Tokenized", model.Settings.Tokenized.ToString());
                    builder.WithSetting("HiddenStringFieldSettings.TemplateString", model.Settings.TemplateString);
                    builder.WithSetting("HiddenStringFieldSettings.AutomaticAdjustmentOnEdit", model.Settings.AutomaticAdjustmentOnEdit.ToString());

                    if (model.ProcessVariant != HiddenStringFieldUpdateProcessVariant.None) {
                        // TODO: Check task isn't already scheduled.
                        var completeFieldName = _contentTypeName + "_" + builder.PartName + "_" + builder.Name;
                        _scheduledTaskManager.CreateTask(HiddenStringFieldValueUpdateTaskHandler.UPDATEVALUES_TASK + "_" + completeFieldName, _clock.UtcNow.AddMinutes(1), null);
                    }

                    model.ProcessVariant = HiddenStringFieldUpdateProcessVariant.None;
                    yield return DefinitionTemplate(model);
                }
            } else {
                yield break;
            }
        }

        private IEnumerable<SelectListItem> GetVariants() {
            var variantDescriptions = new Dictionary<HiddenStringFieldUpdateProcessVariant, string>() {
                { HiddenStringFieldUpdateProcessVariant.None, T("None").Text },
                { HiddenStringFieldUpdateProcessVariant.All, T("All fields").Text },
                { HiddenStringFieldUpdateProcessVariant.Empty, T("Empty fields").Text }
            };

            return variantDescriptions.Select(kvp =>
                new SelectListItem {
                    Selected = false,
                    Text = kvp.Value,
                    Value = kvp.Key.ToString()
                });
        }
    }
}