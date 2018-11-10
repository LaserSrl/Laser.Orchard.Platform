using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Laser.Orchard.NewsLetters.Models;
using Laser.Orchard.NewsLetters.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.NewsLetters.Drivers {
    public class NewsletterDefinitionPartDriver : ContentPartDriver<NewsletterDefinitionPart> {
        private readonly INewsletterServices _newslServices;
        private readonly IOrchardServices _orchardServices;
        private readonly RequestContext _requestContext;

        public NewsletterDefinitionPartDriver(IOrchardServices orchardServices, RequestContext requestContext, INewsletterServices newsletterDefinitionServices) {
            _newslServices = newsletterDefinitionServices;
            _orchardServices = orchardServices;
            _requestContext = requestContext;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "NewsLetters"; }
        }

        protected override DriverResult Display(NewsletterDefinitionPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_NewsletterDefinition_SummaryAdmin",
                        () => shapeHelper.Parts_NewsletterDefinition_SummaryAdmin());
            }
            if (displayType == "Summary") {
                return ContentShape("Parts_NewsletterDefinition_Summary",
                        () => shapeHelper.Parts_NewsletterDefinition_Summary());
            }
            if (displayType == "Detail") {
                return ContentShape("Parts_NewsletterDefinition",
                        () => shapeHelper.Parts_NewsletterDefinition());
            }
            return null;
        }
        protected override DriverResult Editor(NewsletterDefinitionPart part, dynamic shapeHelper) {
            return ContentShape("Parts_NewsletterDefinition_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/NewsletterDefinition_Edit",
                                 Model: part,
                                 Prefix: Prefix));
        }

        protected override DriverResult Editor(NewsletterDefinitionPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("NewsletterDefinitionPartError", T("NewsletterDefinition Error"));
            }
            return Editor(part, shapeHelper);
        }


        //#region [ Import/Export ]
        //protected override void Exporting(NewsletterDefinitionPart part, ExportContentContext context) {
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("TemplateRecord_Id", part.TemplateRecord_Id);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("ConfirmSubscrptionTemplateRecord_Id", part.ConfirmSubscrptionTemplateRecord_Id);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("DeleteSubscrptionTemplateRecord_Id", part.DeleteSubscrptionTemplateRecord_Id);
        //}


        //protected override void Importing(NewsletterDefinitionPart part, ImportContentContext context) {
            
        //    var importedTemplateRecord_Id = context.Attribute(part.PartDefinition.Name, "TemplateRecord_Id");
        //    if (importedTemplateRecord_Id != null) {
        //        part.TemplateRecord_Id = int.Parse(importedTemplateRecord_Id);
        //    }

        //    var importedConfirmSubscrptionTemplateRecord_Id = context.Attribute(part.PartDefinition.Name, "ConfirmSubscrptionTemplateRecord_Id");
        //    if (importedConfirmSubscrptionTemplateRecord_Id != null) {
        //        part.ConfirmSubscrptionTemplateRecord_Id = int.Parse(importedConfirmSubscrptionTemplateRecord_Id);
        //    }

        //    var importedDeleteSubscrptionTemplateRecord_Id = context.Attribute(part.PartDefinition.Name, "DeleteSubscrptionTemplateRecord_Id");
        //    if (importedDeleteSubscrptionTemplateRecord_Id != null) {
        //        part.DeleteSubscrptionTemplateRecord_Id = int.Parse(importedDeleteSubscrptionTemplateRecord_Id);
        //    }


        //}
        //#endregion
    }
}