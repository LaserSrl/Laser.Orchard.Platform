using System;
using System.Collections.Generic;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.MailCommunication.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Tasks.Scheduling;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard.Environment.Extensions;
using System.Web.Mvc;
using Laser.Orchard.MailCommunication.Extensions;

namespace Laser.Orchard.MailCommunication.Handlers {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MailCommunicationPartHandler : ContentHandler {
        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly ITemplateService _templateService;
        private readonly IOrchardServices _orchardServices;
        private readonly IMailCommunicationService _mailCommunicationService;
        private readonly IRepository<CommunicationEmailRecord> _repoMail;
        private readonly IRepository<TitlePartRecord> _repoTitle;
        private readonly ITransactionManager _transactionManager;
        private readonly ICommunicationService _communicationService;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly ShellSettings _shellSettings;
        private readonly IScheduledTaskManager _taskManager;

        public MailCommunicationPartHandler(IControllerContextAccessor controllerContextAccessor, INotifier notifier, ITemplateService templateService, IOrchardServices orchardServices, IMailCommunicationService mailCommunicationService,
            IRepository<CommunicationEmailRecord> repoMail, IRepository<TitlePartRecord> repoTitle, ITransactionManager transactionManager, ICommunicationService communicationService, ShellSettings shellSetting, IScheduledTaskManager taskManager) {
            _repoMail = repoMail;
            _repoTitle = repoTitle;
            _controllerContextAccessor = controllerContextAccessor;
            _transactionManager = transactionManager;
            _notifier = notifier;
            _templateService = templateService;
            _orchardServices = orchardServices;

            _mailCommunicationService = mailCommunicationService;
            _communicationService = communicationService;
            _shellSettings = shellSetting;
            _taskManager = taskManager;

            T = NullLocalizer.Instance;
            OnUpdated<MailCommunicationPart>((context, part) => {
                if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.MailTest") {
                    if (part.SendToTestEmail && part.EmailForTest != string.Empty) {
                        dynamic content = context.ContentItem;

                        string host = string.Format("{0}://{1}{2}",
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80 ? string.Empty : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);

                        var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

                        Dictionary<string, object> similViewBag = new Dictionary<string, object>();
                        similViewBag.Add("CampaignLink", _communicationService.GetCampaignLink("Email", part));

                        // Place Holder
                        List<TemplatePlaceHolderViewModel> listaPH = new List<TemplatePlaceHolderViewModel>();

                        string unsubscribe = T("Click here to stop receiving email for commercial use").Text;
                        string linkUnsubscribe = "<a href='" + string.Format("{0}{1}", host, urlHelper.UnsubscribeMailCommunication()) + "'>" + unsubscribe + "</a>";

                        TemplatePlaceHolderViewModel ph = new TemplatePlaceHolderViewModel();
                        ph.Name = "[UNSUBSCRIBE]";
                        ph.Value = linkUnsubscribe;
                        ph.ShowForce = true;

                        listaPH.Add(ph);

                        if (((Laser.Orchard.TemplateManagement.Models.CustomTemplatePickerPart)content.CustomTemplatePickerPart).SelectedTemplate != null) {
                            _templateService.SendTemplatedEmail(content,
                                ((Laser.Orchard.TemplateManagement.Models.CustomTemplatePickerPart)content.CustomTemplatePickerPart).SelectedTemplate.Id,
                                null,
                                new List<string> { part.EmailForTest },
                                similViewBag,
                                false, listaPH);

                            _notifier.Information(T("Sent e-mail test!"));
                        } else {
                            _notifier.Error(T("Select or create a template for e-mail"));
                        }
                    }
                }
            });
            OnPublished<MailCommunicationPart>((context, part) => {
                if (part.SendOnNextPublish && !part.MailMessageSent) {
                    ContentItem ci = _orchardServices.ContentManager.Get(part.ContentItem.Id);
                    dynamic content = ci;
                    if (((Laser.Orchard.TemplateManagement.Models.CustomTemplatePickerPart)content.CustomTemplatePickerPart).SelectedTemplate != null) {
                        _taskManager.CreateTask("Laser.Orchard.MailCommunication.Task", DateTime.UtcNow.AddMinutes(1), ci);
                        part.MailMessageSent = true;
                    } else {
                        _notifier.Error(T("Select or create a template for e-mail"));
                    }
                }
            });
        }
    }
}