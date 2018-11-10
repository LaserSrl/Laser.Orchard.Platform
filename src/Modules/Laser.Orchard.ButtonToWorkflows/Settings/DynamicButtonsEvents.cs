using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;

namespace Laser.Orchard.ButtonToWorkflows.Settings {
    public class DynamicButtonEvents : ContentDefinitionEditorEventsBase {

        public Localizer T { get; set; }

        public DynamicButtonEvents() {
            T = NullLocalizer.Instance;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name == "DynamicButtonToWorkflowsPart") {
                var model = definition.Settings.GetModel<DynamicButtonsSetting>();

                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "DynamicButtonToWorkflowsPart") yield break;

            var model = new DynamicButtonsSetting();
            if (updateModel.TryUpdateModel(model, "DynamicButtonsSetting", null, null)) {
                builder.WithSetting("DynamicButtonsSetting.Buttons", model.Buttons);
            }

            yield return DefinitionTemplate(model);
        }
    }
}