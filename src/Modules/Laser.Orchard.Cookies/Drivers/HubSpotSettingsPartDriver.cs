using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Laser.Orchard.Cookies.Drivers {
    public class HubSpotSettingsPartDriver : ContentPartDriver<HubSpotSettingsPart> {

        private const string templateName = "Parts/HubSpotSettings";
        public Localizer T { get; set; }
        protected override string Prefix { get { return "HubSpotSettingsPartDriver"; } }

        public HubSpotSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(HubSpotSettingsPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(HubSpotSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_HubSpotSettings_Edit", () =>
            {
                if (updater != null)
                {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: templateName, Model: part, Prefix: Prefix);
            });
        }
    }
}