using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.ButtonToWorkflows.Settings {
    public class ButtonsEvents : ContentDefinitionEditorEventsBase {
        private readonly INotifier _notifier;
        public Localizer T { get; set; }
        public ButtonsEvents(INotifier notifier) {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name == "ButtonToWorkflowsPart") {
                var model = definition.Settings.GetModel<ButtonsSetting>();
                //ButtonsSetting bts= new ButtonsSetting();
                //bts.ButtonNumber=tmp.ButtonNumber.ToString().Split(',');
                if (model.ButtonNumber == null) model.ButtonNumber = new string[] { "" };
                model.ButtonNumber = model.ButtonNumber[0].Split(',');
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ButtonToWorkflowsPart") yield break;
            var model = new ButtonsSetting();
            if (updateModel.TryUpdateModel(model, "ButtonsSetting", null, null)) {
                if (model.ButtonNumber != null) {
                    builder.WithSetting("ButtonsSetting.ButtonNumber", String.Join(",", model.ButtonNumber));
                } else
                    _notifier.Add(NotifyType.Warning, T("Setting -> Buttons : must be configured"));
            }
            yield return DefinitionTemplate(model);
        }
    }
}