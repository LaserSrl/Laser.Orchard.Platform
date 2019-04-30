using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System;

namespace Laser.Orchard.Cookies.Drivers {
    public class CookieSettingsPartDriver : ContentPartDriver<CookieSettingsPart> {
        public CookieSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        protected override string Prefix { get { return "CookieSettings"; } }

        protected override DriverResult Editor(CookieSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CookieSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_Cookie_Settings", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts.Cookie.Settings", Model: part, Prefix: Prefix);
            })
                .OnGroup("Cookies");
        }

        protected override void Exporting(CookieSettingsPart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);

            element.SetAttributeValue("cookiePosition", part.cookiePosition);
            element.SetAttributeValue("cookieDomain", part.cookieDomain);
            element.SetAttributeValue("cookieDiscreetReset", part.cookieDiscreetReset);
            element.SetAttributeValue("cookieDisable", part.cookieDisable);
            element.SetAttributeValue("showCookieResetButton", part.showCookieResetButton);
            element.SetAttributeValue("cookieCutter", part.cookieCutter);
        }

        protected override void Importing(CookieSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            if (root == null) {
                return;
            }
            var partName = part.PartDefinition.Name;
            // Properties of an enum type cannot be treated like the others
            var cookiePos = context.Attribute(partName, "cookiePosition");
            var cbp = CookieBannerPosition.Top; // default value
            if (Enum.TryParse(cookiePos, out cbp)) {
                part.cookiePosition = cbp;
            } else {
                part.cookiePosition = CookieBannerPosition.Top;
            }
            part.cookieDomain = GetAttribute<string>(context, partName, "cookieDomain");
            part.cookieDisable = GetAttribute<string>(context, partName, "cookieDisable");
            part.cookieDiscreetReset = GetAttribute<bool>(context, partName, "cookieDiscreetReset");
            part.cookiePolicyPage = GetAttribute<bool>(context, partName, "cookiePolicyPage");
            part.showCookieResetButton = GetAttribute<bool>(context, partName, "showCookieResetButton");
            part.cookieCutter = GetAttribute<bool>(context, partName, "cookieCutter");
        }

        private TV GetAttribute<TV>(ImportContentContext context, string partName, string elementName) {
            string value = context.Attribute(partName, elementName);
            if (value != null) {
                return (TV)Convert.ChangeType(value, typeof(TV));
            }
            return default(TV);
        }
    }
}