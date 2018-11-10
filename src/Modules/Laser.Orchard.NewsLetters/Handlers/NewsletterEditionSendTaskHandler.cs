using Laser.Orchard.NewsLetters.Models;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.NewsLetters.Services;
using Laser.Orchard.NewsLetters.Extensions;
using Laser.Orchard.StartupConfig.Extensions;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.Commons.Extensions;
using Laser.Orchard.TemplateManagement.Services;
using Laser.Orchard.MailCommunication;
using Newtonsoft.Json;
using Orchard;
using Orchard.Mvc.Html;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;
using Orchard.Environment.Configuration;
using Orchard.Email.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;
using Orchard.Logging;
using System.Dynamic;



namespace Laser.Orchard.NewsLetters.Handlers {
    public class NewsletterEditionSendTaskHandler : IScheduledTaskHandler {

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly INewsletterServices _newslServices;
        private readonly ITemplateService _templateService;
        private readonly IRepository<NewsletterEditionPartRecord> _repositoryNewsletterEdition;
        private readonly ShellSettings _shellSettings;
        private readonly ICommonsServices _commonServices;
        
        private MailerSiteSettingsPart _mailerConfig;
        
        private const string TaskType = "Laser.Orchard.NewsLetters.SendEdition.Task";

        public NewsletterEditionSendTaskHandler(IContentManager contentManager, IOrchardServices orchardServices,
                                                INewsletterServices newslServices, ITemplateService templateService,
                                                ShellSettings shellSettings, ICommonsServices commonServices,
                                                IRepository<NewsletterEditionPartRecord> repositoryNewsletterEdition) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _newslServices = newslServices;
            _templateService = templateService;
            _shellSettings = shellSettings;
            _commonServices = commonServices;
            _repositoryNewsletterEdition = repositoryNewsletterEdition;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType != TaskType) {
                return;
            }
            try {
                dynamic content = context.Task.ContentItem;
                NewsletterEditionPart part = context.Task.ContentItem.As<NewsletterEditionPart>();
                _mailerConfig = _orchardServices.WorkContext.CurrentSite.As<MailerSiteSettingsPart>();
                var urlHelper = _commonServices.GetUrlHelper();
                int[] selectedAnnIds;
                IList<AnnouncementPart> items = null;
                IEnumerable<ExpandoObject> fullyItems;
                if (!String.IsNullOrWhiteSpace(part.AnnouncementIds)) {
                    selectedAnnIds = !String.IsNullOrWhiteSpace(part.AnnouncementIds) ? part.AnnouncementIds.Split(',').Select(s => Convert.ToInt32(s)).ToArray() : null;
                    items = GetAnnouncements(selectedAnnIds);
                    fullyItems = items.Select(
                        s => new {
                            AnnouncementPart = s,
                            DisplayUrl = urlHelper.ItemDisplayUrl(s)
                        }.ToExpando());
                } else {
                    fullyItems = null;
                }
                var subscribers = _newslServices.GetSubscribers(part.NewsletterDefinitionPartRecord_Id).Where(w => w.Confirmed);
                var subscribersEmails = subscribers.Select(s => new { s.Id, s.Name, EmailAddress = s.Email });
                List<object> listaSubscribers = new List<object>(subscribersEmails);

                // ricava i settings e li invia tramite FTP
                var templateId = _newslServices.GetNewsletterDefinition(part.NewsletterDefinitionPartRecord_Id,
                    VersionOptions.Published).As<NewsletterDefinitionPart>().TemplateRecord_Id;
                var model = new {
                    NewsletterEdition = content,
                    ContentItems = fullyItems
                }.ToExpando();
                Dictionary<string, object> settings = GetSettings(model, templateId, part);

                if ((settings.Count > 0) && (listaSubscribers.Count > 0)) {
                    SendSettings(settings, part.Id);

                    // impagina e invia i recipiens tramite FTP
                    int pageNum = 0;
                    List<object> pagina = new List<object>();
                    int pageSize = _mailerConfig.RecipientsPerJsonFile;

                    for (int i = 0; i < listaSubscribers.Count; i++) {
                        if (((i + 1) % pageSize) == 0) {
                            SendRecipients(pagina, part.Id, pageNum);
                            pageNum++;
                            pagina = new List<object>();
                        }
                        pagina.Add(listaSubscribers[i]);
                    }
                    // invia l'ultima pagina se non è vuota
                    if (pagina.Count > 0) {
                        SendRecipients(pagina, part.Id, pageNum);
                    }

                    // Aggiorno la newsletter edition, e rimuovo la relazione tra Newletter e Announcement 
                    part.Dispatched = true;
                    part.DispatchDate = DateTime.Now;
                    part.Number = GetNextNumber(part.NewsletterDefinitionPartRecord_Id); ;

                    if (items != null) {
                        foreach (var item in items) {
                            var ids = ("," + item.AttachToNextNewsletterIds + ",").Replace("," + part.NewsletterDefinitionPartRecord_Id + ",", "");
                            item.AttachToNextNewsletterIds = ids;
                        }
                    }

                    _contentManager.Publish(context.Task.ContentItem);
                } else {
                    Logger.Error(T("Error parsing mail template.").Text);
                }
            } catch (Exception ex) {
                string idcontenuto = "nessun id ";
                try {
                    idcontenuto = context.Task.ContentItem.Id.ToString();
                } catch (Exception ex2) { Logger.Error(ex2, ex2.Message); }
                Logger.Error(ex, "Error on " + TaskType + " for ContentItem id = " + idcontenuto + " : " + ex.Message);
            }

        }

        private IList<AnnouncementPart> GetAnnouncements(int[] selectedIds) {
            var list = _contentManager.Query<AnnouncementPart, AnnouncementPartRecord>(VersionOptions.Published)
                .Where(w => selectedIds.Contains(w.Id))
                .OrderBy(br => br.AnnouncementTitle)
                .List();
            return list.ToList();
        }

        /// <summary>
        /// [{"Id":value,"EmailAddress":"value","Title":"value"}]
        /// </summary>
        private void SendRecipients(List<object> recipients, int communicationId, int pageNum) {
            string pathFtp = _mailerConfig.FtpPath;
            string jsonDestinatari = JsonConvert.SerializeObject(recipients);
            SendFtp(jsonDestinatari, _mailerConfig.FtpHost, _mailerConfig.FtpUser, _mailerConfig.FtpPassword, string.Format("{0}nws{1}.{2}-{3}-recipients.json", pathFtp, _shellSettings.Name, communicationId, pageNum));
        }

        /// <summary>
        /// {"Subject":"value","Body":"value","Sender":"value","Priority":"value"}
        /// </summary>
        private void SendSettings(object settings, int communicationId) {
            string pathFtp = _mailerConfig.FtpPath;
            string jsonSettings = JsonConvert.SerializeObject(settings);
            SendFtp(jsonSettings, _mailerConfig.FtpHost, _mailerConfig.FtpUser, _mailerConfig.FtpPassword, string.Format("{0}nws{1}.{2}-settings.json", pathFtp, _shellSettings.Name, communicationId));
        }

        private void SendFtp(string contenuto, string host, string usr, string pwd, string fileName) {
            // upload di un file tramite ftp
            using (System.Net.FtpClient.FtpClient client = new System.Net.FtpClient.FtpClient()) {
                client.Host = host;
                client.Credentials = new System.Net.NetworkCredential(usr, pwd);
                client.Connect();
                using (var ftpStream = client.OpenWrite(fileName)) {
                    byte[] buffer = System.Text.ASCIIEncoding.Unicode.GetBytes(contenuto);
                    ftpStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private Dictionary<string, object> GetSettings(dynamic contentModel, int templateId, NewsletterEditionPart part) {
            var data = new Dictionary<string, object>();

            var baseUri = new Uri(_orchardServices.WorkContext.CurrentSite.BaseUrl);

            var template = _templateService.GetTemplate(templateId);
            string body = _templateService.RitornaParsingTemplate(contentModel, templateId);

            if (!body.StartsWith("Error On Template")) {

                string host = string.Format("{0}://{1}{2}",
                                       baseUri.Scheme,
                                       baseUri.Host,
                                       baseUri.Port == 80 ? string.Empty : ":" + baseUri.Port);

                var urlHelper = _commonServices.GetUrlHelper();

                // Add Link [UNSUBSCRIBE]
                string ph_Unsubscribe = "[UNSUBSCRIBE]";
                string unsubscribe = T("Click here to unsubscribe").Text;
                string linkUnsubscribe = "<a href='" + string.Format("{0}{1}?newsletterId={2}", host, urlHelper.SubscriptionUnsubscribe(), part.NewsletterDefinitionPartRecord_Id) + "'>" + unsubscribe + "</a>";

                if (body.Contains(ph_Unsubscribe))
                    body = body.Replace(ph_Unsubscribe, linkUnsubscribe);
                else
                    body += "<br /><br />" + linkUnsubscribe;

                var subject = template.Subject;
                var smtp = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
                string priority = "L";
                switch (_mailerConfig.MailPriority) {
                    case MailPriorityValues.High:
                        priority = "H";
                        break;

                    case MailPriorityValues.Normal:
                        priority = "N";
                        break;

                    default:
                        priority = "L";
                        break;
                }

                data.Add("Subject", subject);
                data.Add("Body", body);
                data.Add("Sender", smtp.Address);
                data.Add("Priority", priority);
            }
            return data;
        }

        public int GetNextNumber(int newsltterId) {
            var maxNumber = _repositoryNewsletterEdition.Table
                 .Where(w => w.NewsletterDefinitionPartRecord_Id == newsltterId)
                 .Select(s => s.Number)
                 .Max();
            return (maxNumber.HasValue ? maxNumber.Value + 1 : 1);
        }

    }
}