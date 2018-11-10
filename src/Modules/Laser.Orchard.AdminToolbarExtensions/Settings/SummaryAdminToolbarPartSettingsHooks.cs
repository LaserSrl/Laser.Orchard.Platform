using Laser.Orchard.AdminToolbarExtensions.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdminToolbarExtensions.Settings {
    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions")]
    public class SummaryAdminToolbarPartSettingsHooks : ContentDefinitionEditorEventsBase  {

        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition){
                
            if (definition.PartDefinition.Name != "SummaryAdminToolbarPart") yield break;
            var model = definition.Settings.GetModel<SummaryAdminToolbarPartSettings>();
            string toParse = "";
            if (definition.Settings.TryGetValue("SummaryAdminToolbarPartSettings.Labels", out toParse)) {
                model.ParseStringToList(toParse);
            }

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name == "SummaryAdminToolbarPart") {
                var model = new SummaryAdminToolbarPartSettings();
                updateModel.TryUpdateModel(model, "SummaryAdminToolbarPartSettings", null, null);
                //put actual values in the PartSettings
                builder.WithSetting("SummaryAdminToolbarPartSettings.Labels", model.ParseListToString());

                yield return DefinitionTemplate(model);
            }

            yield break;
        }
    }
}