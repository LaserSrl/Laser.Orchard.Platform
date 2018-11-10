using Laser.Orchard.Accessibility.Models;
using Laser.Orchard.Accessibility.Services;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.Accessibility.Drivers
{
    public class AccessibilityPartDriver : ContentPartDriver<AccessibilityPart>
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        public Localizer t;
        public ILogger Logger { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Accessibility"; }
        }

        public AccessibilityPartDriver(IOrchardServices orchardServices, ShellSettings shellSetting)
        {
            _orchardServices = orchardServices;
            _shellSettings = shellSetting;
            t = NullLocalizer.Instance;
        }

        protected override DriverResult Display(AccessibilityPart part, string displayType, dynamic shapeHelper)
        {
            // calcola l'url del controller
            string tenantPath = new Utils().GetTenantBaseUrl(_shellSettings);
            tenantPath = (tenantPath.EndsWith("/")) ? tenantPath : tenantPath + "/";
            tenantPath = tenantPath + "Laser.Orchard.Accessibility/Accessibility";

            // gestisce il tipo di visualizzazione
            if (displayType == "Summary")
                return ContentShape("Parts_Accessibility_Summary",
                    () => shapeHelper.Parts_Accessibility_Summary());
            if (displayType == "SummaryAdmin")
                return ContentShape("Parts_Accessibility_SummaryAdmin",
                    () => shapeHelper.Parts_Accessibility_SummaryAdmin());

            // visualizzazione di dettaglio

            // se il cookie non esiste lo crea come "normal"
            var accessibilityServices = _orchardServices.WorkContext.Resolve<IAccessibilityServices>();
            if (_orchardServices.WorkContext.HttpContext.Request.Cookies.Get(Utils.AccessibilityCookieName) == null)
            {
                accessibilityServices.SetNormal();
            }

            return ContentShape("Parts_Accessibility",
                () => shapeHelper.Parts_Accessibility(Url: tenantPath));
        }
    }
}
