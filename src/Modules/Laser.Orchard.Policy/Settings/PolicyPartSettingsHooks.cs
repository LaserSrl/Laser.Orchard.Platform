using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Policy;
using Laser.Orchard.Policy.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Laser.Orchard.Policy.Settings {
    public class PolicyPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition) {

            if (definition.PartDefinition.Name != "PolicyPart") yield break;
            var model = definition.Settings.GetModel<PolicyPartSettings>();
            if (model.PolicyTextReferences != null && model.PolicyTextReferences.Length > 0) {
                model.PolicyTextReferences = model.PolicyTextReferences[0].Split(',');
            }

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "PolicyPart") yield break;

            var model = new PolicyPartSettings();
            updateModel.TryUpdateModel(model, "PolicyPartSettings", null, null);
            if (model.PolicyTextReferences == null || model.PolicyTextReferences.Length == 0) {
                model.PolicyTextReferences = new string[] { "{All}" };
            } else if (model.PolicyTextReferences.Contains("{All}")) {
                model.PolicyTextReferences = new string[] { "{All}" };
            } else if (model.PolicyTextReferences.Contains("{DependsOnContent}")) {
                model.PolicyTextReferences = new string[] { "{DependsOnContent}" };
            }

            builder.WithSetting("PolicyPartSettings.IncludePendingPolicy",
            ((IncludePendingPolicyOptions)model.IncludePendingPolicy).ToString());
            builder.WithSetting("PolicyPartSettings.PolicyTextReferences",
            String.Join(",", model.PolicyTextReferences));
            yield return DefinitionTemplate(model);
        }
    }
}