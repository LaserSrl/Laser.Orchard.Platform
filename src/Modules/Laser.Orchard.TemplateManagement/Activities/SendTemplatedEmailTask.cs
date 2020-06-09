using Laser.Orchard.Commons.Extensions;
using Laser.Orchard.StartupConfig.Extensions;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Email.Models;
using Orchard.Email.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Security;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.UI.Notify;
using System.Collections.Specialized;
using Orchard.JobsQueue.Services;

namespace Laser.Orchard.TemplateManagement.Activities {

    [OrchardFeature("Laser.Orchard.TemplateEmailActivities")]
    public class MailActivity : Task {
        private readonly IOrchardServices _orchardServices;
        private readonly IMessageService _messageService;
        private readonly IMembershipService _membershipService;
        private readonly ITemplateService _templateServices;
        private readonly INotifier _notifier;
        private readonly IJobsQueueService _jobsQueueService;

        public const string MessageType = "ActionTemplatedEmail";

        public MailActivity(
            IMessageService messageService,
            IOrchardServices orchardServices,
            IMembershipService membershipService,
            ITemplateService templateServices,
            INotifier notifier,
            IJobsQueueService jobsQueueService
) {
            _messageService = messageService;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _templateServices = templateServices;
            _jobsQueueService = jobsQueueService;
            T = NullLocalizer.Instance;
            _notifier = notifier; ;
        }

        public Localizer T { get; set; }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Sent") };
        }

        public override string Form {
            get {
                return "ActivityActionTemplatedEmail";
            }
        }

        public override LocalizedString Category {
            get { return T("Messaging"); }
        }

        public override string Name {
            get { return "SendTemplatedEmail"; }
        }

        public override LocalizedString Description {
            get { return T("Sends an e-mail using a template to a specific user."); }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            string recipient = activityContext.GetState<string>("Recipient");
            string recipientCC = activityContext.GetState<string>("RecipientCC");
            string recipientBCC = activityContext.GetState<string>("RecipientBCC");
            string fromEmail = activityContext.GetState<string>("FromEmail");
            string replyTo = activityContext.GetState<string>("ReplyTo");
            string emailTemplate = activityContext.GetState<string>("EmailTemplate");
            bool NotifyReadEmail = activityContext.GetState<string>("NotifyReadEmail") == "NotifyReadEmail";
            string attachmentList = activityContext.GetState<string>("AttachmentList");
            var queued = activityContext.GetState<bool>("Queued");
            var priority = activityContext.GetState<int>("Priority");
            List<string> sendTo = new List<string>();
            List<string> sendCC = new List<string>();
            List<string> sendBCC = new List<string>();
            List<string> attachments = new List<string>();
            var templateId = 0;
            var customTemplateId = activityContext.GetState<string>("CustomTemplateId");

            if (!int.TryParse(customTemplateId, out templateId)) {
                var tmp = _orchardServices.ContentManager.ResolveIdentity(new ContentIdentity(emailTemplate));
                if (tmp != null) {
                    templateId = tmp.Id;
                }
                else {
                    int.TryParse(emailTemplate, out templateId);
                }
            }
            int contentVersion = 0;
            ContentItem content = null;
            if (workflowContext.Content != null) {
                contentVersion = workflowContext.Content.ContentItem.Version;
                content = _orchardServices.ContentManager.GetAllVersions(workflowContext.Content.Id).Single(w => w.Version == contentVersion); // devo ricalcolare il content altrimenti MediaParts (e forse tutti i lazy fields!) è null!
            }
            dynamic contentModel = new {
                ContentItem = content,
                FormCollection = _orchardServices.WorkContext.HttpContext == null ? new NameValueCollection() : _orchardServices.WorkContext.HttpContext.Request.Form,
                QueryStringCollection = _orchardServices.WorkContext.HttpContext == null ? new NameValueCollection() : _orchardServices.WorkContext.HttpContext.Request.QueryString,
                WorkflowContext = workflowContext
            };
            if (recipient == "owner") {
                if (content.Has<CommonPart>()) {
                    var owner = content.As<CommonPart>().Owner;
                    if (owner != null && owner.ContentItem != null && owner.ContentItem.Record != null) {
                        sendTo.AddRange(SplitList(owner.As<IUser>().Email));
                    }
                    sendTo.AddRange(SplitList(owner.As<IUser>().Email));
                }
            }
            else if (recipient == "author") {
                var user = _orchardServices.WorkContext.CurrentUser;

                // can be null if user is anonymous
                if (user != null && !String.IsNullOrWhiteSpace(user.Email)) {
                    sendTo.AddRange(SplitList(user.Email));
                }
            }
            else if (recipient == "admin") {
                var username = _orchardServices.WorkContext.CurrentSite.SuperUser;
                var user = _membershipService.GetUser(username);

                // can be null if user is no super user is defined
                if (user != null && !String.IsNullOrWhiteSpace(user.Email)) {
                    sendTo.AddRange(SplitList(user.As<IUser>().Email));
                }
            }
            else if (recipient == "other") {
                sendTo.AddRange(SplitList(activityContext.GetState<string>("RecipientOther")));
            }
            if (!String.IsNullOrWhiteSpace(recipientCC)) {
                sendCC.AddRange(SplitList(recipientCC));
            }
            if (!String.IsNullOrWhiteSpace(recipientBCC)) {
                sendBCC.AddRange(SplitList(recipientBCC));
            }
            if (!String.IsNullOrWhiteSpace(attachmentList)) {
                attachments.AddRange(SplitList(attachmentList));
            }
            if (SendEmail(contentModel, templateId, sendTo, sendCC, sendBCC, NotifyReadEmail, fromEmail, replyTo, attachments,queued,priority))

                yield return T("Sent");
            else
                yield return T("Not Sent");
        }

        private static IEnumerable<string> SplitList(string commaSeparated) {
            if (commaSeparated == null) return null;
            return commaSeparated.Split(new[] { ',', ';' });
        }

        private bool SendEmail(dynamic contentModel, int templateId, IEnumerable<string> sendTo, IEnumerable<string> cc, IEnumerable<string> bcc, bool NotifyReadEmail, string fromEmail = null, string replyTo = null, IEnumerable<string> attachments = null,bool queued=false,int priority=0) {
            var template = _templateServices.GetTemplate(templateId);
            var body = _templateServices.RitornaParsingTemplate(contentModel, templateId);
            if (body.StartsWith("Error On Template")) {
                _notifier.Add(NotifyType.Error, T("Error on template, mail not sent"));
                return false;
            }
            var data = new Dictionary<string, object>();
            var smtp = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            var recipient = sendTo != null ? sendTo : new List<string> { smtp.Address };

            data.Add("Subject", template.Subject);
            data.Add("Body", body);
            data.Add("Recipients", String.Join(",", recipient));
            if (cc != null) {
                data.Add("CC", String.Join(",", cc));
            }
            if (bcc != null) {
                data.Add("Bcc", String.Join(",", bcc));
            }
            if (fromEmail != null) {
                data.Add("FromEmail", fromEmail);
            }
            if(replyTo != null) {
                data.Add("ReplyTo", replyTo);
            }
            data.Add("NotifyReadEmail", NotifyReadEmail);
            if (attachments != null) {
                if (attachments.Count() > 0) {
                    data.Add("Attachments", attachments);
                }
            }
           

            if (!queued) {
                _messageService.Send(SmtpMessageChannel.MessageType, data);
            }
            else {
                _jobsQueueService.Enqueue("IMessageService.Send", new { type = SmtpMessageChannel.MessageType, parameters = data }, priority);
            }

            return true;
        }
    }
}