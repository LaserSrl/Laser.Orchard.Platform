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