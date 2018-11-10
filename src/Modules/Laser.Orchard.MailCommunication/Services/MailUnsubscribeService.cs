using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.MailCommunication.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.MailCommunication.Extensions;

namespace Laser.Orchard.MailCommunication.Services {

    public interface IMailUnsubscribeService : IDependency {
        bool SendMailConfirmUnsubscribe(string email);
        bool UnsubscribeMail(string keyUnsubscribe);
    }

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MailUnsubscribeService : IMailUnsubscribeService {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<CommunicationEmailRecord> _emailrepository;
        private readonly INotifier _notifier;
        private readonly ICommonsServices _commonServices;
        private readonly ITemplateService _templateServices;
        private readonly ShellSettings _shellSettings;

        public Localizer T { get; set; }

        public MailUnsubscribeService(IOrchardServices orchardServices, IRepository<CommunicationEmailRecord> Emailrepository, INotifier notifier,
                                      ICommonsServices commonServices, ITemplateService templateServices, ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _emailrepository = Emailrepository;
            _notifier = notifier;
            _commonServices = commonServices;
            _templateServices = templateServices;
            _shellSettings = shellSettings;

            T = NullLocalizer.Instance;
        }


        public bool SendMailConfirmUnsubscribe(string email) {
            // Controllo che la mail inserita sia presente tra i contatti
            List<CommunicationEmailRecord> listaCommunicationEmail = (from m in _emailrepository.Table
                                                                      where m.Email.Equals(email)
                                                                      select m).ToList();

            if (listaCommunicationEmail == null) {
                _orchardServices.Notifier.Error(T("Email not found!"));
                return false;
            }

            // Create Nonce
            string parametri = "Email=" + email + "&Guid=" + Guid.NewGuid().ToString();
            TimeSpan delay = new TimeSpan(1, 0, 0);
            string Nonce = _commonServices.CreateNonce(parametri, delay);

            // Update CommunicationEmailRecord - Save Nonce
            foreach (CommunicationEmailRecord recordMail in listaCommunicationEmail) {
                recordMail.KeyUnsubscribe = Nonce;
                _emailrepository.Update(recordMail);
            }

            // Encode Nonce
            string parametriEncode = System.Web.HttpUtility.HtmlEncode(Nonce.Replace('+', '_').Replace('/', '-'));

            string host = string.Format("{0}://{1}{2}",
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80 ? string.Empty : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);

            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            
            // Model template Mail
            dynamic viewModel = new UnsubscribeVM {
                Email = email,
                LinkUnsubscription = string.Format("{0}{1}?key={2}", host, urlHelper.ConfirmUnsubscribeMailCommunication(), parametriEncode),
                KeyUnsubscription = Nonce,
                UnsubscriptionDate = DateTime.Now
            };

            var settings = _orchardServices.WorkContext.CurrentSite.As<MailCommunicationSettingsPart>();

            if (settings.IdTemplateUnsubscribe == null) {
                _orchardServices.Notifier.Error(T("Select in settings or create a template for e-mail unsubscribe"));
                return false;
            }

            // Send Mail
            _templateServices.SendTemplatedEmail(viewModel, (int)settings.IdTemplateUnsubscribe, null, new List<string> { email }, null, false, null);

            return true;
        }

        public bool UnsubscribeMail(string keyUnsubscribe) {
            string parametri = "";

            // Ritorno al Nonce originale
            keyUnsubscribe = System.Web.HttpUtility.HtmlDecode(keyUnsubscribe.Replace('_', '+').Replace('-', '/'));

            // Verifico che non sia passata più di 1 ora dalla richiesta di Unsubscribe
            bool decryptOk = _commonServices.DecryptNonce(keyUnsubscribe, out parametri);

            if (!decryptOk) {
                _orchardServices.Notifier.Error(T("Unsubscribe impossible. Please try again."));
                return false;
            }

            string[] infoKey = parametri.Split('&');
            string[] parEmail = infoKey[0].Split('=');
            string email = parEmail[1];

            // Verifico tra i contatti che sia presente la mail e la key (nonce)
            List<CommunicationEmailRecord> listaCommunicationEmail = (from m in _emailrepository.Table
                                                                      where m.Email.Equals(email) && m.KeyUnsubscribe.Equals(keyUnsubscribe)
                                                                      select m).ToList();

            if (listaCommunicationEmail == null) {
                _orchardServices.Notifier.Error(T("Email not found!"));
                return false;
            }

            // Update CommunicationEmailRecord - Delete Nonce, Save DataUnsubscribe, set AccettatoUsoCommerciale false 
            foreach (CommunicationEmailRecord recordMail in listaCommunicationEmail) {
                recordMail.KeyUnsubscribe = null;
                recordMail.DataUnsubscribe = DateTime.Now;
                recordMail.AccettatoUsoCommerciale = false;

                _emailrepository.Update(recordMail);
            }

            return true;
        }




    }
}