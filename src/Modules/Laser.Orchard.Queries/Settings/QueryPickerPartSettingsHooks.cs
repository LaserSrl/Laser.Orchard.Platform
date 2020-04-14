using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Queries.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Laser.Orchard.Queries.Settings {
    public class QueryPickerPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition) {

            if (definition.PartDefinition.Name != "QueryPickerPart") yield break;
            var model = definition.Settings.GetModel<QueryPickerPartSettings>();


            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "QueryPickerPart") yield break;

            var model = new QueryPickerPartSettings();
            updateModel.TryUpdateModel(model, "QueryPickerPartSettings", null, null);
            builder.WithSetting("QueryPickerPartSettings.IsForHqlQueries",
            ((bool)model.IsForHqlQueries).ToString());
            yield return DefinitionTemplate(model);
        }
    }
}