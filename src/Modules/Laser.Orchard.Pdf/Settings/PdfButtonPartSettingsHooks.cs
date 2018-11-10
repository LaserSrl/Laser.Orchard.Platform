using Laser.Orchard.Pdf.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;

namespace Laser.Orchard.Pdf.Settings {
    public class PdfButtonPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "PdfButtonPart") yield break;
            var model = definition.Settings.GetModel<PdfButtonPartSettings>();
            string toParse = "";
            if (definition.Settings.TryGetValue("PdfButtonPartSettings.PdfButtons", out toParse)) {
                model.LoadStringToList(toParse);
            }
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "PdfButtonPart") yield break;
            var model = new PdfButtonPartSettings();
            updateModel.TryUpdateModel(model, "PdfButtonPartSettings", null, null);

            // carica ogni campo dei settings
            builder.WithSetting("PdfButtonPartSettings.PdfButtons", model.ParseListToString());

            yield return DefinitionTemplate(model);
        }
    }
}