using Laser.Orchard.PaymentGateway.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Settings {
    public class PayButtonPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "PayButtonPart") yield break;
            var model = definition.Settings.GetModel<PayButtonPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "PayButtonPart") yield break;
            var model = new PayButtonPartSettings();
            updateModel.TryUpdateModel(model, "PayButtonPartSettings", null, null);
            builder.WithSetting("PayButtonPartSettings.AmountField", model.AmountField);
            builder.WithSetting("PayButtonPartSettings.CurrencyField", model.CurrencyField);
            builder.WithSetting("PayButtonPartSettings.DefaultCurrency", model.DefaultCurrency);
            yield return DefinitionTemplate(model);
        }
    }
}