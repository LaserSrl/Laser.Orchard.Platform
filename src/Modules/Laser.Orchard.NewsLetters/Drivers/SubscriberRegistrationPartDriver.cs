using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Laser.Orchard.NewsLetters.Models;
using Laser.Orchard.NewsLetters.Services;
using Laser.Orchard.NewsLetters.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.NewsLetters.Drivers {
    public class SubscriberRegistrationPartDriver : ContentPartDriver<SubscriberRegistrationPart> {

        private readonly INewsletterServices _newslServices;
        private readonly IOrchardServices _orchardServices;
        private readonly RequestContext _requestContext;
        private readonly IContentManager _contentManager;

        public SubscriberRegistrationPartDriver(IOrchardServices orchardServices, 
            RequestContext requestContext, 
            INewsletterServices newsletterDefinitionServices,
            IContentManager contentManager) {

            _newslServices = newsletterDefinitionServices;
            _orchardServices = orchardServices;
            _requestContext = requestContext;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "NewsLetters"; }
        }

        protected override DriverResult Display(SubscriberRegistrationPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_SubscriberRegistration_SummaryAdmin",
                        () => shapeHelper.Parts_SubscriberRegistration_SummaryAdmin());
            }
            if (displayType == "Summary") {
                return ContentShape("Parts_SubscriberRegistration_Summary",
                        () => shapeHelper.Parts_SubscriberRegistration_Summary());
            }
            if (displayType == "Detail") {
                return ContentShape("Parts_SubscriberRegistration",
                        () => shapeHelper.Parts_SubscriberRegistration());
            }
            return null;
        }
        protected override DriverResult Editor(SubscriberRegistrationPart part, dynamic shapeHelper) {
            return ContentShape("Parts_SubscriberRegistration_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/SubscriberRegistration_Edit",
                                 Model: part,
                                 Prefix: Prefix));
        }

        protected override DriverResult Editor(SubscriberRegistrationPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("SubscriberRegistrationPartError", T("SubscriberRegistration Error"));
            }
            var selectedNews = HttpContext.Current.Request.Form[Prefix + ".Subscription_Newsletters_Ids"];

            if (selectedNews == null)
                updater.AddModelError("SubscriberRegistrationPartNewsletterError", T("Subscription for newsletters not found"));
            else 
            {
                selectedNews = String.Join(",", selectedNews.Split(',').Where(w => w != "false"));

                if (string.IsNullOrEmpty(selectedNews))
                    updater.AddModelError("SubscriberRegistrationPartValidationError", T("Subscription for newsletters is mandatory"));
                else
                    part.NewsletterDefinitionIds = selectedNews;
            }

            return Editor(part, shapeHelper);
        }


        #region [ Import/Export ]
        protected override void Exporting(SubscriberRegistrationPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("NewsletterDefinitionIds",
                string.Join(@"/,\", _newslServices
                    .GetNewsletterDefinition(part.NewsletterDefinitionIds, VersionOptions.Published)
                    .Select(def => _contentManager.GetItemMetadata(def.ContentItem).Identity.ToString())));

            context.Element(part.PartDefinition.Name).SetAttributeValue("PermitCumulativeRegistrations", 
                part.PermitCumulativeRegistrations);
        }

        protected override void Importing(SubscriberRegistrationPart part, ImportContentContext context) {
            var importedNewsletterDefinitionIds = context.Attribute(part.PartDefinition.Name, "NewsletterDefinitionIds");
            if (importedNewsletterDefinitionIds != null) {
                var identifiers = importedNewsletterDefinitionIds.Split(new string[] { @"/,\" }, StringSplitOptions.RemoveEmptyEntries);
                if (identifiers != null && identifiers.Any()) {
                    part.NewsletterDefinitionIds = string.Join(",",
                        identifiers.Select(context.GetItemFromSession).Select(contentItem => contentItem.Id));
                }
            }

            var importedPermitCumulativeRegistrations = context.Attribute(part.PartDefinition.Name, "PermitCumulativeRegistrations");
            if (importedPermitCumulativeRegistrations != null) {
                part.PermitCumulativeRegistrations = bool.Parse(importedPermitCumulativeRegistrations);
            }

        }
        #endregion

    }
}