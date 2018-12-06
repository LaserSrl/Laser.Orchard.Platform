
using Laser.Orchard.Cookies.Models;
using Laser.Orchard.Cookies.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;

namespace Laser.Orchard.Cookies.Drivers {

    
    public class CookieLawPartDriver : ContentPartCloningDriver<CookieLawPart> {

        private readonly IWorkContextAccessor _workContextAccessor;

        public CookieLawPartDriver(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        protected override DriverResult Display(CookieLawPart part, string displayType, dynamic shapeHelper) {
            var workContext = _workContextAccessor.GetContext();
            var gdprScriptservice = workContext.Resolve<IGDPRScript>();
            var cookieSettings = workContext.CurrentSite.As<CookieSettingsPart>();

            return ContentShape("Parts_CookieLaw",
                () => shapeHelper.Parts_CookieLaw(CookieSettings: cookieSettings, CookieLawPart: part, GDPRScriptservice: gdprScriptservice));
        }

        protected override DriverResult Editor(CookieLawPart part, dynamic shapeHelper) {

            var workContext = _workContextAccessor.GetContext();
            var cookieSettings = workContext.CurrentSite.As<CookieSettingsPart>();
            var editModel = new Laser.Orchard.Cookies.ViewModels.CookieLawEditModel {
                CookieLaw = part,
                CookieSettings = cookieSettings
            };

            return ContentShape("Parts_CookieLaw_Edit",
                                () => shapeHelper.EditorTemplate(
                                      TemplateName: "Parts/CookieLawWidgetSettings",
                                      Model: editModel,
                                      Prefix: Prefix));
        }

        protected override DriverResult Editor(CookieLawPart part, IUpdateModel updater, dynamic shapeHelper) {

            var workContext = _workContextAccessor.GetContext();
            var cookieSettings = workContext.CurrentSite.As<CookieSettingsPart>();
            var editModel = new Laser.Orchard.Cookies.ViewModels.CookieLawEditModel {
                CookieLaw = part,
                CookieSettings = cookieSettings
            };

            updater.TryUpdateModel(editModel, Prefix, null, null);
            return Editor(editModel.CookieLaw, shapeHelper);
        }



        protected override void Exporting(CookieLawPart part, ExportContentContext context) {

            var element = context.Element(part.PartDefinition.Name);

            element.SetAttributeValue("cookieDiscreetLinkText", part.cookieDiscreetLinkText);
            element.SetAttributeValue("cookiePolicyPageMessage", part.cookiePolicyPageMessage);
            element.SetAttributeValue("cookieErrorMessage", part.cookieErrorMessage);
            element.SetAttributeValue("cookieAcceptButtonText", part.cookieAcceptButtonText);
            element.SetAttributeValue("cookieDeclineButtonText", part.cookieDeclineButtonText);
            element.SetAttributeValue("cookieResetButtonText", part.cookieResetButtonText);
            element.SetAttributeValue("cookieWhatAreLinkText", part.cookieWhatAreLinkText);
            element.SetAttributeValue("cookieAnalyticsMessage", part.cookieAnalyticsMessage);
            element.SetAttributeValue("cookiePolicyLink", part.cookiePolicyLink);
            element.SetAttributeValue("cookieMessage", part.cookieMessage);
            element.SetAttributeValue("cookieWhatAreTheyLink", part.cookieWhatAreTheyLink);
        }



        protected override void Importing(CookieLawPart part, ImportContentContext context) {

            var partName = part.PartDefinition.Name;

            part.cookieDiscreetLinkText = GetAttribute<string>(context, partName, "cookieDiscreetLinkText");
            part.cookiePolicyPageMessage = GetAttribute<string>(context, partName, "cookiePolicyPageMessage");
            part.cookieErrorMessage = GetAttribute<string>(context, partName, "cookieErrorMessage");
            part.cookieAcceptButtonText = GetAttribute<string>(context, partName, "cookieAcceptButtonText");
            part.cookieDeclineButtonText = GetAttribute<string>(context, partName, "cookieDeclineButtonText");
            part.cookieResetButtonText = GetAttribute<string>(context, partName, "cookieResetButtonText");
            part.cookieWhatAreLinkText = GetAttribute<string>(context, partName, "cookieWhatAreLinkText");
            part.cookieAnalyticsMessage = GetAttribute<string>(context, partName, "cookieAnalyticsMessage");
            part.cookiePolicyLink = GetAttribute<string>(context, partName, "cookiePolicyLink");
            part.cookieMessage = GetAttribute<string>(context, partName, "cookieMessage");
            part.cookieWhatAreTheyLink = GetAttribute<string>(context, partName, "cookieWhatAreTheyLink");
        }


        private TV GetAttribute<TV>(ImportContentContext context, string partName, string elementName) {
            string value = context.Attribute(partName, elementName);
            if (value != null) {
                return (TV)Convert.ChangeType(value, typeof(TV));
            }
            return default(TV);
        }


       

       

      
       
      


        protected override void Cloning(CookieLawPart originalPart, CookieLawPart clonePart, CloneContentContext context) {
            clonePart.cookieDiscreetLinkText = originalPart.cookieDiscreetLinkText;
            clonePart.cookiePolicyPageMessage = originalPart.cookiePolicyPageMessage;
            clonePart.cookieErrorMessage = originalPart.cookieErrorMessage;
            clonePart.cookieAcceptButtonText = originalPart.cookieAcceptButtonText;
            clonePart.cookieDeclineButtonText = originalPart.cookieDeclineButtonText;
            clonePart.cookieResetButtonText = originalPart.cookieResetButtonText;
            clonePart.cookieWhatAreLinkText = originalPart.cookieWhatAreLinkText;
            clonePart.cookieAnalyticsMessage = originalPart.cookieAnalyticsMessage;
            clonePart.cookiePolicyLink = originalPart.cookiePolicyLink;
            clonePart.cookieMessage = originalPart.cookieMessage;
            clonePart.cookieWhatAreTheyLink = originalPart.cookieWhatAreTheyLink;
        }
    }
}