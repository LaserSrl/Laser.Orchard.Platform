using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.MailCommunication.Services;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Newtonsoft.Json;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Notify;
using Orchard.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Email.Models;
using System.Web.Mvc;
using Laser.Orchard.Commons.Extensions;
using Laser.Orchard.StartupConfig.Extensions;
using Orchard.Services;
using Orchard.Logging;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard.Environment.Extensions;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.MailCommunication.Extensions;

namespace Laser.Orchard.MailCommunication.Handlers {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MailerScheduledTaskHandler : IScheduledTaskHandler {
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardServices _orchardServices;
        private readonly IMailCommunicationService _mailCommunicationService;
        private readonly ICommunicationService _communicationService;
        private readonly ITemplateService _templateService;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IClock _clock;
        private readonly ICommonsServices _commonServices;

        private MailerSiteSettingsPart _mailerConfig;
        private const string TaskType = "Laser.Orchard.MailCommunication.Task";

        public MailerScheduledTaskHandler(IOrchardServices orchardServices, IMailCommunicationService mailCommunicationService,
                                          ShellSettings shellSettings, ICommunicationService communicationService, ITemplateService templateService,
                                          IScheduledTaskManager taskManager, IClock clock, ICommonsServices commonServices) {
            _orchardServices = orchardServices;
            _mailCommunicationService = mailCommunicationService;
            _shellSettings = shellSettings;
            _communicationService = communicationService;
            _templateService = templateService;
            _taskManager = taskManager;
            _commonServices = commonServices;
            _clock = clock;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType != TaskType) {
                return;
            }
            try {
                dynamic content = context.Task.ContentItem;
                MailCommunicationPart part = context.Task.ContentItem.As<MailCommunicationPart>();
                Int32[] ids = null;
                Int32? idLocalization = null;

                _mailerConfig = _orchardServices.WorkContext.CurrentSite.As<MailerSiteSettingsPart>();
                if (content.QueryPickerPart != null && content.QueryPickerPart.Ids.Length > 0)
                    ids = content.QueryPickerPart.Ids;

                var localizedPart = content.LocalizationPart;
                if (localizedPart != null && localizedPart.Culture != null)
                    idLocalization = localizedPart.Culture.Id;

                IList lista;
                if (part.UseRecipientList) {
                    if (string.IsNullOrWhiteSpace(part.RecipientList)) {
                        lista = new List<object>();
                    } else {
                        lista = _mailCommunicationService
                            .GetMailQueryResult(
                                part.RecipientList.Split(new string[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries),
                                idLocalization, 
                                false, 
                                context.Task.ContentItem);
                    }
                    
                } else {
                    lista = _mailCommunicationService.GetMailQueryResult(ids, idLocalization, false, context.Task.ContentItem);
                }

                // ricava i settings e li invia tramite FTP
                var templateId = ((Laser.Orchard.TemplateManagement.Models.CustomTemplatePickerPart)content.CustomTemplatePickerPart).SelectedTemplate.Id;
                Dictionary<string, object> settings = GetSettings(content, templateId, part);
                
                if (lista == null || lista.Count == 0) {
                    Logger.Error(T("No recipients selected.").ToString());
                } else if ((settings.Count > 0) && (lista.Count > 0)) {
                    SendSettings(settings, part.Id);

                    // impagina e invia i recipiens tramite FTP
                    int pageNum = 0;
                    List<object> pagina = new List<object>();
                    int pageSize = _mailerConfig.RecipientsPerJsonFile;
                    for (int i = 0; i < lista.Count; i++) {
                        if (((i + 1) % pageSize) == 0) {
                            SendRecipients(pagina, part.Id, pageNum);
                            pageNum++;
                            pagina = new List<object>();
                        }
                        pagina.Add(lista[i]);
                    }
                    // invia l'ultima pagina se non è vuota
                    if (pagina.Count > 0) {
                        SendRecipients(pagina, part.Id, pageNum);
                    }
                    part.RecipientsNumber = lista.Count;
                    part.SentMailsNumber = 0;
                    part.MailMessageSent = true;
                } else {
                    Logger.Error(T("Error parsing mail template.").ToString());
                }
                
            } catch (Exception ex) {
                string idcontenuto = "nessun id ";
                try {
                    idcontenuto = context.Task.ContentItem.Id.ToString();
                } catch (Exception ex2) { Logger.Error(ex2, ex2.Message); }
                Logger.Error(ex, "Error on " + TaskType + " for ContentItem id = " + idcontenuto + " : " + ex.Message);
            }
        }

        /// <summary>
        /// [{"Id":value,"EmailAddress":"value","Title":"value"}]
        /// </summary>
        private void SendRecipients(List<object> recipients, int communicationId, int pageNum) {
            string pathFtp = _mailerConfig.FtpPath;
            string jsonDestinatari = JsonConvert.SerializeObject(recipients);
            SendFtp(jsonDestinatari, _mailerConfig.FtpHost, _mailerConfig.FtpUser, _mailerConfig.FtpPassword, string.Format("{0}adv{1}.{2}-{3}-recipients.json", pathFtp, _shellSettings.Name, communicationId, pageNum));
        }

        /// <summary>
        /// {"Subject":"value","Body":"value","Sender":"value","Priority":"value","Url":"value","Attachments":"value"}
        /// </summary>
        private void SendSettings(object settings, int communicationId) {
            string pathFtp = _mailerConfig.FtpPath;
            string jsonSettings = JsonConvert.SerializeObject(settings);
               SendFtp(jsonSettings, _mailerConfig.FtpHost, _mailerConfig.FtpUser, _mailerConfig.FtpPassword, string.Format("{0}adv{1}.{2}-settings.json", pathFtp, _shellSettings.Name, communicationId));
        }

        /// <summary>
        /// Invia un testo (con codifica Unicode) tramite FTP.
        /// </summary>
        /// <param name="contenuto">Il testo da inviare.</param>
        /// <param name="host"></param>
        /// <param name="usr"></param>
        /// <param name="pwd"></param>
        /// <param name="fileName">Path e nome del file di destinazione.</param>
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
        private Dictionary<string, object> GetSettings(dynamic contentModel, int templateId, MailCommunicationPart part) {
            var data = new Dictionary<string, object>();

            var baseUri = new Uri(_orchardServices.WorkContext.CurrentSite.BaseUrl);

            Dictionary<string, object> similViewBag = new Dictionary<string, object>();
            similViewBag.Add("CampaignLink", _communicationService.GetCampaignLink("Email", part));

            string body = _templateService.RitornaParsingTemplate(contentModel, templateId, similViewBag);

            if (!body.StartsWith("Error On Template")) {

                string host = string.Format("{0}://{1}{2}",
                                       baseUri.Scheme,
                                       baseUri.Host,
                                       baseUri.Port == 80 ? string.Empty : ":" + baseUri.Port);

                var urlHelper = _commonServices.GetUrlHelper();


                // Add Link [UNSUBSCRIBE]
                string ph_Unsubscribe = "[UNSUBSCRIBE]";
                string unsubscribe = T("Click here to stop receiving email for commercial use").Text;
                string linkUnsubscribe = "<a href='" + string.Format("{0}{1}", host, urlHelper.UnsubscribeMailCommunication()) + "'>" + unsubscribe + "</a>";

                if (body.Contains(ph_Unsubscribe))
                    body = body.Replace(ph_Unsubscribe, linkUnsubscribe);
                else
                    body += "<br /><br />" + linkUnsubscribe;

                var subject = contentModel.TitlePart.Title;
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

                var baseUrl = baseUri.ToString();
                // token di sicurezza: contiene data e ora (senza minuti e secondi) e id del content item
                var token = string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHH"), (contentModel as ContentItem).Id);
                token = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(token));
                //var url = string.Format("{0}/Laser.Orchard.MailCommunication/MailerResult?tk={1}", baseUrl, token);  // versione per il GET
                var tenantPrefix = GetTenantUrlPrexix(_shellSettings);
                var url = string.Format("{0}/{1}api/Laser.Orchard.MailCommunication/MailerResultAPI?tk={2}", baseUrl, tenantPrefix, token);  // versione per il POST

                data.Add("Subject", subject);
                data.Add("Body", body);
                data.Add("Sender", smtp.Address);
                data.Add("Priority", priority);
                data.Add("Url", url);  // url di ritorno per comunicare a Orchard il numero di mail inviate con successo
                data.Add("Attachments", ""); // TODO esempio: "[\"prova.pdf\",\"prova.docx\"]" 2016-01-14: per ora non li gestiamo
            }

            return data;
        }

        /// <summary>
        /// Get the URL prefix of the current tenant.
        /// If prefix is not empty, it ends with slash (/).
        /// </summary>
        /// <param name="shellSettings"></param>
        /// <returns></returns>
        private string GetTenantUrlPrexix(ShellSettings shellSettings) {
            // calcola il prefix del tenant corrente
            string tenantPath = shellSettings.RequestUrlPrefix ?? "";

            if (tenantPath != "") {
                tenantPath = tenantPath + "/";
            }
            return tenantPath;
        }
    }
}