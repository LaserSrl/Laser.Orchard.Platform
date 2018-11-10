using Laser.Orchard.Translator.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Laser.Orchard.Translator.Drivers
{
    public class TranslatorSettingsDriver : ContentPartDriver<TranslatorSettingsPart>
    {
        private const string templateName = "Parts/TranslatorSettings";

        public Localizer T { get; set; }

        public TranslatorSettingsDriver()
        {
            T = NullLocalizer.Instance;
        }

        protected override string Prefix { get { return "TranslatorSettings"; } }

        protected override DriverResult Editor(TranslatorSettingsPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_TranslatorSettings_Edit",
                                    () => shapeHelper.EditorTemplate(
                                          TemplateName: templateName,
                                          Model: part,
                                          Prefix: Prefix)).OnGroup("Translator");
        }

        protected override DriverResult Editor(TranslatorSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}