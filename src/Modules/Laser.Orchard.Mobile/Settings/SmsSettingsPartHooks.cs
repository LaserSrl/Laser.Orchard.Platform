using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Settings {
    //[OrchardFeature("Laser.Orchard.Sms")]
    //public class SmsSettingsPartHooks : ContentDefinitionEditorEventsBase {
    //    public override IEnumerable<TemplateViewModel> TypePartEditor(
    //        ContentTypePartDefinition definition) {

    //            if (definition.PartDefinition.Name != "SmsSettingsPart") yield break;
    //            var model = definition.Settings.GetModel<SmsSettingsPart>();


    //        yield return DefinitionTemplate(model);
    //    }

    //    public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
    //        ContentTypePartDefinitionBuilder builder,
    //        IUpdateModel updateModel) {

    //            if (builder.Name != "SmsSettingsPart") yield break;

    //            var model = new SmsSettingsPart();
    //            updateModel.TryUpdateModel(model, "SmsSettingsPart", null, null);
    //        builder.WithSetting("MapPartSettings.Required",
    //        ((bool)model.Required).ToString());
    //        yield return DefinitionTemplate(model);
    //    }
    //}
}