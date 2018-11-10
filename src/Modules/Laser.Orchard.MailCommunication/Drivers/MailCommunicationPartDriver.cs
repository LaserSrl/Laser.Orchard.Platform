using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.UI.Admin;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.MailCommunication.Drivers {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MailCommunicationPartDriver : ContentPartCloningDriver<MailCommunicationPart> {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ITemplateService _templateService;


        public MailCommunicationPartDriver(IContentManager contentManager, IOrchardServices orchardServices, ITemplateService templateService) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _templateService = templateService;
        }
        protected override string Prefix {
            get { return "MailCommunicationPart"; }
        }

        protected override DriverResult Display(MailCommunicationPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Summary")
                    return ContentShape("Parts_MailCommunication",
                        () => shapeHelper.Parts_MailCommunication(MailMessageSent: part.MailMessageSent, SendOnNextPublish: part.SendOnNextPublish, RecipientsNumber: part.RecipientsNumber, SentMailsNumber: part.SentMailsNumber));
                if (displayType == "SummaryAdmin")
                    return ContentShape("Parts_MailCommunication",
                        () => shapeHelper.Parts_MailCommunication(MailMessageSent: part.MailMessageSent, SendOnNextPublish: part.SendOnNextPublish, RecipientsNumber: part.RecipientsNumber, SentMailsNumber: part.SentMailsNumber));
                return null;
            } else {
                return null;
            }
        }

        protected override DriverResult Editor(MailCommunicationPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(MailCommunicationPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vModel = new CustomTemplatePickerViewModel {
                TemplateIdSelected = part.ContentItem.As<CustomTemplatePickerPart>().SelectedTemplate != null ? part.ContentItem.As<CustomTemplatePickerPart>().SelectedTemplate.Id : (int?)null,
                TemplatesList = _templateService.GetTemplates().Where(w => ((dynamic)w.ContentItem).CustomTemplate.ForEmailCommunication.Value == true)
            };

            if (updater != null) {
                if (updater.TryUpdateModel(part, Prefix, null, null) && updater.TryUpdateModel(vModel, Prefix, null, null)) {
                    if (vModel.TemplateIdSelected != null) {
                        part.ContentItem.As<CustomTemplatePickerPart>().SelectedTemplate = _contentManager.Get<TemplatePart>(vModel.TemplateIdSelected.Value);
                    }
                    //if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.MailTest") {
                    //    // Logica di invio mail forse meglio metterla in un handler > OnUpdated
                    //}
                }
            }

            var shapes = new List<DriverResult>();
            shapes.Add(ContentShape("Parts_MailCommunication_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MailCommunication_Edit", Model: part, Prefix: Prefix)));
            shapes.Add(ContentShape("Parts_MailCommunicationActions_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MailCommunicationActions_Edit", Model: part, Prefix: Prefix)));
            shapes.Add(ContentShape("Parts_CustomTemplatePickerOverride_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/CustomTemplatePickerOverride_Edit", Model: vModel, Prefix: Prefix)));
            return new CombinedResult(shapes);
        }

        protected override void Importing(MailCommunicationPart part, ImportContentContext context) {
            part.MailMessageSent = false;
            part.SendOnNextPublish = false;

            var importedSendToTestEmail = context.Attribute(part.PartDefinition.Name, "SendToTestEmail");
            if (importedSendToTestEmail != null) {
                part.SendToTestEmail = bool.Parse(importedSendToTestEmail);
            }

            var importedEmailForTest = context.Attribute(part.PartDefinition.Name, "EmailForTest");
            if (importedEmailForTest != null) {
                part.EmailForTest =importedEmailForTest;
            }

            part.SentMailsNumber = 0;

            var importedRecipientList = context.Attribute(part.PartDefinition.Name, "RecipientList");
            if (importedRecipientList != null) {
                part.RecipientList = importedRecipientList;
            }

            var importedUseRecipientList = context.Attribute(part.PartDefinition.Name, "UseRecipientList");
            if (importedUseRecipientList != null) {
                part.UseRecipientList = bool.Parse(importedUseRecipientList);
            }
        }

        protected override void Exporting(MailCommunicationPart part, ExportContentContext context) {
            //MailMessageSent non serve per l'import
            //SendOnNextPublish non serve per l'import
            context.Element(part.PartDefinition.Name).SetAttributeValue("SendToTestEmail", part.SendToTestEmail);
            context.Element(part.PartDefinition.Name).SetAttributeValue("EmailForTest", part.EmailForTest);
            //RecipientsNumber non serve per l'import
            //SentMailsNumber non serve per l'import
            context.Element(part.PartDefinition.Name).SetAttributeValue("RecipientList", part.RecipientList);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UseRecipientList", part.UseRecipientList);
        }

        protected override void Cloning(MailCommunicationPart originalPart, MailCommunicationPart clonePart, CloneContentContext context) {
            //do not clone MailMessageSent so that we can send it in the cloned part
            clonePart.MailMessageSent = false;
            clonePart.SendOnNextPublish = false;
            clonePart.SendToTestEmail = originalPart.SendToTestEmail;
            clonePart.EmailForTest = originalPart.EmailForTest;
            //do not clone RecipientsNumber because that is computed by the handler
            //do not clone SentMailsNumber because that is computed by the handler
            clonePart.UseRecipientList = originalPart.UseRecipientList;
            clonePart.RecipientList = originalPart.RecipientList;
        }
    }
}