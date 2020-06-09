using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.DisplayManagement;
using Orchard.Environment.Features;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Laser.Orchard.TemplateManagement.Activities {
    public class TemplatedEmailEditForm : IFormProvider {
        private readonly ITemplateService _templateServices;
        private readonly IFeatureManager _featureManager;
        private readonly IContentManager _contentManager;

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public TemplatedEmailEditForm(IShapeFactory shapeFactory, ITemplateService templateServices, IFeatureManager featureManager, IContentManager contentManager) {
            Shape = shapeFactory;
            _templateServices = templateServices;
            _featureManager = featureManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var jobsQueueEnabled = _featureManager.GetEnabledFeatures().Any(x => x.Id == "Orchard.JobsQueue");
                    var f = Shape.Form(
                        Id: "ActionEmail",
                        _Type: Shape.FieldSet(
                            Title: T("Send to"),
                            _RecipientOwner: Shape.Radio(
                                Id: "recipient-owner",
                                Name: "Recipient",
                                Value: "owner",
                                Title: T("Owner"),
                                Description: T("The owner of the content item in context, such as a blog post's author.")
                            ),
                            _RecipientAuthor: Shape.Radio(
                                Id: "recipient-author",
                                Name: "Recipient",
                                Value: "author",
                                Title: T("Author"),
                                Description: T("The current user when this action executes.")
                            ),
                            _RecipientAdmin: Shape.Radio(
                                Id: "recipient-admin",
                                Name: "Recipient",
                                Value: "admin",
                                Title: T("Site Admin"),
                                Description: T("The site administrator.")
                            ),
                            _RecipientOther: Shape.Radio(
                                Id: "recipient-other",
                                Name: "Recipient",
                                Value: "other",
                                Title: T("Other:")
                            ),
                            _OtherEmails: Shape.Textbox(
                                Id: "recipient-other-email",
                                Name: "RecipientOther",
                                Title: T("E-mail"),
                                Description: T("Specify a comma-separated list of e-mail recipients."),
                                Classes: new[] { "large", "text", "tokenized" }
                            ),
                            _CCEmails: Shape.Textbox(
                                Id: "recipient-cc-email",
                                Name: "RecipientCC",
                                Title: T("CC"),
                                Description: T("Specify a comma-separated list of e-mail CC recipients."),
                                Classes: new[] { "large", "text", "tokenized" }
                            ),
                            _BCCEmails: Shape.Textbox(
                                Id: "recipient-bcc-email",
                                Name: "RecipientBCC",
                                Title: T("BCC"),
                                Description: T("Specify a comma-separated list of e-mail BCC recipients."),
                                Classes: new[] { "large", "text", "tokenized" }
                            ),
                               _FromEmails: Shape.Textbox(
                                Id: "from-email",
                                Name: "FromEmail",
                                Title: T("From"),
                                Description: T("Specify email sender."),
                                Classes: new[] { "large", "text", "tokenized" }
                            ),
                            _ReplyTo: Shape.Textbox(
                                Id: "reply-to",
                                Name: "ReplyTo",
                                Title: T("ReplyTo"),
                                Description: T("Specify reply to."),
                                Classes: new[] { "large", "text", "tokenized" }
                            ),
                            _NotifyReadEmail: Shape.Checkbox(
                                Id: "NotifyReadEmail",
                                Name: "NotifyReadEmail",
                                Title: T("Notify at Read Email"),
                                Description: T("Notify Read Email."),
                                Value: "NotifyReadEmail"
                            )
                        ),
                        _Parts: Shape.SelectList(
                            Id: "email-template", Name: "EmailTemplate",
                            Title: T("Default template"),
                            Description: T("A default template to format your email message."),
                                Size: 1,
                                Multiple: false
                        ),
                        _Template: Shape.Textbox(
                                Id: "custom-template-id",
                                Name: "CustomTemplateId",
                                Title: T("Custom template ID"),
                                Description: T("Specify a template ID to format your email message. Leave blank to use the default template."),
                                Classes: new[] { "large", "text", "tokenized" }
                        ),
                        _Attachments: Shape.Textbox(
                                Id: "attachment-list",
                                Name: "AttachmentList",
                                Title: T("Attachment Path List"),
                                Description: T("Specify a comma separated list of file phisical path (e.g. c:\\Temp\\dummy_file.txt)."),
                                Classes: new[] { "large", "text", "tokenized" }
                        )
                    );
                    if (jobsQueueEnabled) {
                        f._Type._Queued(Shape.Checkbox(
                                Id: "Queued", Name: "Queued",
                                Title: T("Queued"),
                                Checked: false, Value: "true",
                                Description: T("Check send it as a queued job.")));

                        f._Type._Priority(Shape.SelectList(
                                Id: "priority",
                                Name: "Priority",
                                Title: T("Priority"),
                                Description: ("The priority of this message.")
                            ));

                        f._Type._Priority.Add(new SelectListItem { Value = "-50", Text = T("Low").Text });
                        f._Type._Priority.Add(new SelectListItem { Value = "0", Text = T("Normal").Text });
                        f._Type._Priority.Add(new SelectListItem { Value = "50", Text = T("High").Text });
                    }
                    var allTemplates = _templateServices.GetTemplates().Where(w => !w.IsLayout);

                    foreach (var template in allTemplates) {
                        f._Parts.Add(new SelectListItem { Value = _contentManager.GetItemMetadata(template).Identity.ToString(), Text = template.Title });
                    }
                    return f;
                };


            context.Form("ActivityActionTemplatedEmail", form);
        }
    }

    public class MailFormsValidator : IFormEventHandler {
        public Localizer T { get; set; }

        public void Building(BuildingContext context) {
        }

        public void Built(BuildingContext context) {
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName != "ActivityActionTemplatedEmail") return;

            var recipientFormValue = context.ValueProvider.GetValue("Recipient");
            var recipient = recipientFormValue != null ? recipientFormValue.AttemptedValue : String.Empty;

            if (recipient == String.Empty) {
                context.ModelState.AddModelError("Recipient", T("You must select at least one recipient").Text);
            }

            if (context.ValueProvider.GetValue("RecipientOther").AttemptedValue == String.Empty && recipient == "other") {
                context.ModelState.AddModelError("RecipientOther", T("You must provide an e-mail address").Text);
            }
        }

        public void Validated(ValidatingContext context) {
        }
    }

}