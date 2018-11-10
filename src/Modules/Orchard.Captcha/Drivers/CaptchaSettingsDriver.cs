using Orchard.Captcha.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Orchard.Captcha.Drivers {
    public class CaptchaSettingsDriver : ContentPartDriver<CaptchaSettingsPart> {
        private readonly IOrchardServices _orchardServices;
        public CaptchaSettingsDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "CaptchaSettings"; } }

        protected override DriverResult Editor(CaptchaSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        //POST
        protected override DriverResult Editor(CaptchaSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            return ContentShape("Parts_Captcha_SiteSettings", () => 
            {
                if (updater != null) {
                    if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                        updater.AddModelError("CaptchaSettingsError", T("Captcha settings not saved"));
                    }
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts.Captcha.SiteSettings", Model: part, Prefix: Prefix);
            }).OnGroup("ReCaptcha");
        }
    }
}