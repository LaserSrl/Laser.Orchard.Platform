using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Laser.Orchard.ShortLinks.Services;
using Laser.Orchard.StartupConfig.Fields;
using Laser.Orchard.StartupConfig.Handlers;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.MediaLibrary.Fields;
using Orchard.Modules.Services;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.Projections.Services;
using Orchard.Security;
using Orchard.Tags.Models;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.Services {

    public interface ICommunicationService : IDependency {

        bool AdvertisingIsAvailable(Int32 id);

        string GetCampaignLink(string CampaignSource, ContentPart part);

        bool CampaignLinkExist(ContentPart part);

        void UserToContact(IUser UserContent);

        void ContactToUser(ContentItem contact);

        CommunicationContactPart GetContactFromUser(int iduser);

        List<ContentItem> GetContactsFromMail(string mail);

        List<ContentItem> GetContactsFromSms(string prefix, string sms);

        ContentItem GetContactFromName(string name);

        ContentItem GetContactFromId(int id);

        void Synchronize();

        void UnboundFromUser(UserPart userPart);

        /// <summary>
        /// Elimina le mail e gli SMS (phone number) associati a un contatto.
        /// </summary>
        /// <param name="contactId"></param>
        void RemoveMailsAndSms(int contactId);

        CommunicationContactPart EnsureMasterContact();

        CommunicationContactPart TryEnsureContact(int userId);

        /// <summary>
        /// Returns false if no further run is needed, true if further run is needed.
        /// </summary>
        bool GetRunAgainNeeded(int contentId, string context, string data, bool completedIteration, int maxNumRetry);

        void AddEmailToContact(string email, ContentItem contact);

        void AddSmsToContact(string pref, string num, ContentItem contact, bool overridexisting);
    }

    public class CommunicationService : ICommunicationService {
        private readonly IOrchardServices _orchardServices;
        private readonly IShortLinksService _shortLinksService;
        private readonly IContentExtensionsServices _contentExtensionsServices;
        private readonly IModuleService _moduleService;
        private readonly INotifier _notifier;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ICultureManager _cultureManager;
        private readonly IContactRelatedEventHandler _contactRelatedEventHandler;
        private readonly ITransactionManager _transactionManager;
        private readonly IFieldIndexService _fieldIndexService;
        private readonly IAutorouteService _autorouteService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IUtilsServices _utilsServices;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private readonly IRepository<CommunicationEmailRecord> _repositoryCommunicationEmailRecord;
        private readonly IRepository<CommunicationSmsRecord> _repositoryCommunicationSmsRecord;
        private readonly IRepository<CommunicationRetryRecord> _repositoryCommunicationRetryRecord;

        public CommunicationService(
            ITaxonomyService taxonomyService,
            IRepository<CommunicationEmailRecord> repositoryCommunicationEmailRecord,
            INotifier notifier,
            IModuleService moduleService,
            IOrchardServices orchardServices,
            IShortLinksService shortLinksService,
            IContentExtensionsServices contentExtensionsServices,
            ISessionLocator session,
            ICultureManager cultureManager,
            IRepository<CommunicationSmsRecord> repositoryCommunicationSmsRecord,
            IContactRelatedEventHandler contactRelatedEventHandler,
            ITransactionManager transactionManager,
            IFieldIndexService fieldIndexService,
            IAutorouteService autorouteService,
            IRepository<CommunicationRetryRecord> repositoryCommunicationRetryRecord,
            IContentDefinitionManager contentDefinitionManager,
            IUtilsServices utilsServices) {

            _orchardServices = orchardServices;
            _shortLinksService = shortLinksService;
            _contentExtensionsServices = contentExtensionsServices;
            _moduleService = moduleService;
            _notifier = notifier;
            _repositoryCommunicationEmailRecord = repositoryCommunicationEmailRecord;
            _repositoryCommunicationSmsRecord = repositoryCommunicationSmsRecord;
            _repositoryCommunicationRetryRecord = repositoryCommunicationRetryRecord;
            _taxonomyService = taxonomyService;
            _cultureManager = cultureManager;
            _contactRelatedEventHandler = contactRelatedEventHandler;
            _transactionManager = transactionManager;
            _fieldIndexService = fieldIndexService;
            _autorouteService = autorouteService;
            _contentDefinitionManager = contentDefinitionManager;
            _utilsServices = utilsServices;

            T = NullLocalizer.Instance;
        }

        public bool AdvertisingIsAvailable(Int32 id) {
            ContentItem ci = _orchardServices.ContentManager.Get(id, VersionOptions.DraftRequired);
            if (ci.ContentType != "CommunicationAdvertising") {
                return false;
            }
            if (ci.As<CommunicationAdvertisingPart>().CampaignId > 0) { // è legato ad una campagna
                ContentItem campaign = _orchardServices.ContentManager.Get(ci.As<CommunicationAdvertisingPart>().CampaignId, VersionOptions.Latest);
                DateTime from = ((DateTimeField)(((dynamic)campaign).CommunicationCampaignPart.FromDate)).DateTime;
                DateTime to = ((DateTimeField)(((dynamic)campaign).CommunicationCampaignPart.ToDate)).DateTime;
                if (from > DateTime.UtcNow || (to < DateTime.UtcNow && to != DateTime.MinValue))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Elimina le mail e gli SMS (phone number) associati a un contatto.
        /// </summary>
        /// <param name="contactId"></param>
        public void RemoveMailsAndSms(int contactId) {
            // elimina le mail associate
            var elencoCer = _repositoryCommunicationEmailRecord.Fetch(x => x.EmailContactPartRecord_Id == contactId).ToList();
            if (elencoCer != null) {
                foreach (var cer in elencoCer) {
                    _repositoryCommunicationEmailRecord.Delete(cer);
                }
            }

            // elimina gli sms associati
            var elencoCsr = _repositoryCommunicationSmsRecord.Fetch(x => x.SmsContactPartRecord_Id == contactId).ToList();
            if (elencoCsr != null) {
                foreach (var csr in elencoCsr) {
                    _repositoryCommunicationSmsRecord.Delete(csr);
                }
            }
        }

        public CommunicationContactPart EnsureMasterContact() {
            CommunicationContactPart master = null;
            List<ContentItem> mastersToBeDeleted = new List<ContentItem>();
            // cerca tutti i master contact attivi presenti nel sistema e sceglie il più recente
            var activeMasters = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).OrderByDescending(y => y.Id).List();
            foreach (var contact in activeMasters) {
                if (master == null) {
                    master = contact;
                }
                else {
                    mastersToBeDeleted.Add(contact.ContentItem);
                }
            }

            // se non c'è nessun master contact lo crea
            if (master == null) {
                var Contact = _orchardServices.ContentManager.Create("CommunicationContact");
                Contact.As<TitlePart>().Title = "Master Contact";
                Contact.As<CommunicationContactPart>().Master = true;
                master = Contact.As<CommunicationContactPart>();
                _notifier.Add(NotifyType.Information, T("Master Contact Created"));
            }

            // elimina i master contact in eccesso
            foreach (var contact in mastersToBeDeleted) {
                _orchardServices.ContentManager.Remove(contact);
            }
            return master;
        }

        public CommunicationContactPart TryEnsureContact(int userId) {
            CommunicationContactPart contact = null;
            var user = _orchardServices.ContentManager.Get(userId).As<IUser>();
            if (user != null) {
                UserToContact(user);
                contact = GetContactFromUser(userId);
            }
            return contact;
        }

        public void Synchronize() {
            EnsureMasterContact();
            _transactionManager.RequireNew();

            // allinea i contatti
            var users = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().List();
            foreach (var user in users) {
                UserToContact(user);
                _transactionManager.RequireNew();
            }
            _notifier.Add(NotifyType.Information, T("Syncronized {0} user's profiles", users.Count().ToString()));

            // triggera l'allineamento dei device
            _contactRelatedEventHandler.Synchronize();

            // aggiungo 200.000 record
            //for (int i = 0; i < 100000; i++) {
            //    var email = Guid.NewGuid() + "@fake.it";
            //    ContentItem Contact;
            //    Contact = _orchardServices.ContentManager.New("CommunicationContact");
            //    _orchardServices.ContentManager.Create(Contact);
            //    CommunicationEmailRecord newrec = new CommunicationEmailRecord();
            //    newrec.Email = email;
            //    newrec.CommunicationContactPartRecord_Id = Contact.Id;
            //    _repositoryCommunicationEmailRecord.Create(newrec);
            //    _repositoryCommunicationEmailRecord.Flush();
            //    Contact.As<TitlePart>().Title = email + " progr:" + i.ToString();
            //    _orchardServices.TransactionManager.RequireNew();
            //}
        }

        public CommunicationContactPart GetContactFromUser(int iduser) {
            return _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == iduser).List().FirstOrDefault();
        }

        public List<ContentItem> GetContactsFromMail(string mail) {
            string hql = @"SELECT cir.Id as Id
                FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                join civr.ContentItemRecord as cir
                join cir.EmailContactPartRecord as EmailPart
                join EmailPart.EmailRecord as EmailRecord 
                WHERE civr.Published=1 AND EmailRecord.Validated AND EmailRecord.Email = :mail";

            var elencoId = _transactionManager.GetSession()
                .CreateQuery(hql)
                .SetParameter("mail", mail)
                .List();
            var contentQuery = _orchardServices.ContentManager.Query(VersionOptions.Latest)
                .ForType("CommunicationContact")
                .Where<CommunicationContactPartRecord>(x => elencoId.Contains(x.Id)).List();
            return contentQuery.ToList();
        }

        public List<ContentItem> GetContactsFromSms(string prefix, string sms) {
            string hql = @"SELECT cir.Id as Id
                FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                join civr.ContentItemRecord as cir
                join cir.SmsContactPartRecord as SmsPart
                join SmsPart.SmsRecord as SmsRecord 
                WHERE civr.Published=1 AND SmsRecord.Prefix = :prefix AND SmsRecord.Sms = :sms";
            var elencoId = _transactionManager.GetSession()
                .CreateQuery(hql)
                .SetParameter("prefix", prefix)
                .SetParameter("sms", sms)
                .List();
            var contentQuery = _orchardServices.ContentManager.Query(VersionOptions.Latest)
                .ForType("CommunicationContact")
                .Where<CommunicationContactPartRecord>(x => elencoId.Contains(x.Id)).List();
            return contentQuery.ToList();
        }

        public ContentItem GetContactFromName(string name) {
            var query = _orchardServices.ContentManager.Query(new string[] { "CommunicationContact" })
                .Where<TitlePartRecord>(x => x.Title == name);
            return query.List().FirstOrDefault();
        }

        public ContentItem GetContactFromId(int id) {
            var query = _orchardServices.ContentManager.Query(new string[] { "CommunicationContact" })
                .Where<CommunicationContactPartRecord>(x => x.Id == id);
            return query.List().FirstOrDefault();
        }

        /// <summary>
        ///La parte sarebbe CommunicationAdvertisingPart ma non l'ho definita quindi passo una cosa generica (ContentPart)
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public string GetCampaignLink(string CampaignSource, ContentPart generalpart) {
            string shortlink = "";
            ContentPart part = (ContentPart)(((dynamic)generalpart).ContentItem.CommunicationAdvertisingPart);
            string CampaignTerm = "";
            var tagPart = part.ContentItem.As<TagsPart>();
            if (tagPart != null) {
                CampaignTerm = string.Join("+", tagPart.CurrentTags.ToArray()).ToLower();
            }
            string CampaignMedium = CampaignSource;
            string CampaignContent = part.ContentItem.As<TitlePart>().Title.ToLower();
            string CampaignName = "Flash";
            try {
                int idCampagna = ((int)((dynamic)part).CampaignId);
                CampaignName = _orchardServices.ContentManager.Get(idCampagna).As<TitlePart>().Title;
            }
            catch {
                // comunicato non legato a campagna
            }
            string link = "";
            if (!string.IsNullOrEmpty(((dynamic)part).UrlLinked.Value)) {
                link = (string)(((dynamic)part).UrlLinked.Value);
            }
            else {
                var pickerField = ((dynamic)part).ContentLinked as ContentPickerField;
                if (pickerField != null && pickerField.ContentItems != null) {
                    var firstItem = pickerField.ContentItems.FirstOrDefault();
                    if (firstItem != null) {
                        var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                        link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(firstItem));
                    }
                    else {
                        return "";
                    }
                }
                else {
                    return "";
                }
            }

            string linkelaborated = ElaborateLink(link, CampaignSource, CampaignMedium, CampaignTerm, CampaignContent, CampaignName);
            if (!string.IsNullOrEmpty(linkelaborated)) {
                shortlink = _shortLinksService.GetShortLink(linkelaborated);
                if (string.IsNullOrEmpty(shortlink)) {
                    throw new Exception("Url Creation Failed");
                }
            }
            return shortlink;
        }

        public bool CampaignLinkExist(ContentPart generalpart) {
            bool linkExist = false;

            ContentPart part = (ContentPart)(((dynamic)generalpart).ContentItem.CommunicationAdvertisingPart);

            if (!string.IsNullOrEmpty(((dynamic)part).UrlLinked.Value)) {
                linkExist = true;
            }
            else {
                var pickerField = ((dynamic)part).ContentLinked as ContentPickerField;

                if (pickerField != null) {
                    try {
                        var firstItem = pickerField.ContentItems.FirstOrDefault();
                        if (firstItem != null) {
                            linkExist = true;
                        }
                    }
                    catch { }
                }
            }

            return linkExist;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="CampaignSource"></param>
        /// <param name="CampaignMedium"></param>
        /// <param name="CampaignTerm">Tassonomia legata al contenuto cliccato</param>
        /// <param name="CampaignContent">Used for A/B testing</param>
        /// <param name="CampaignName"></param>
        /// <returns></returns>
        private string ElaborateLink(string link, string CampaignSource = "newsletter", string CampaignMedium = "email", string CampaignTerm = "", string CampaignContent = "", string CampaignName = "") {
            var uriBuilder = new UriBuilder(link);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["utm_source"] = "Krake";
            query["utm_medium"] = CampaignMedium;
            query["utm_term"] = CampaignTerm;
            query["utm_content"] = CampaignContent;
            query["utm_campaign"] = CampaignName;
            uriBuilder.Query = query.ToString();
            link = uriBuilder.ToString();
            return link;
        }

        public void UnboundFromUser(UserPart userPart) {
            var contacts = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == userPart.Id).List();
            foreach (var contact in contacts) {
                contact.UserIdentifier = 0;
                contact.As<CommonPart>().Owner = _orchardServices.WorkContext.CurrentUser;
                contact.As<CommonPart>().ModifiedUtc = DateTime.UtcNow;
            }
        }

        private string TruncateFromStart(string text, int max) {
            string result = text;
            int len = text.Length;
            if (len > max) {
                result = result.Substring(len - max);
            }
            return result;
        }
        public bool GetRunAgainNeeded(int contentId, string context, string data, bool completedIteration, int maxNumRetry) {
            bool result = false; // "non è necessario eseguire un altro run"
            CommunicationRetryRecord retry = _repositoryCommunicationRetryRecord.Get(x => x.ContentItemRecord_Id == contentId && x.Context == context);
            if (retry == null) {
                // inizializza un nuovo oggetto
                retry = new CommunicationRetryRecord {
                    ContentItemRecord_Id = contentId,
                    Context = context,
                    NoOfFailures = 0,
                    Data = "",
                    PendingErrors = false
                };
            }
            // aggiorna gli errori solo se ce ne sono, per non perdere l'informazione sulla presenza di errori all'interno dell'iterazione corrente
            if (string.IsNullOrWhiteSpace(data) == false) {
                retry.Data = data;
                retry.PendingErrors = true;
            }

            if (completedIteration) {
                if (retry.PendingErrors) {
                    retry.NoOfFailures++;
                    retry.PendingErrors = false; // resetta il flag degli errori all'inizio di una nuova iterazione
                    if (retry.NoOfFailures <= maxNumRetry) {
                        // maxNumRetry non ancora raggiunto quindi è necessario un nuovo retry
                        result = true;
                    }
                }
            }
            else {
                // è necessario almeno un altro run per completare l'iterazione
                result = true;
            }
            // salva su db il record aggiornato
            if (retry.Id == 0) {
                _repositoryCommunicationRetryRecord.Create(retry);
            }
            else {
                _repositoryCommunicationRetryRecord.Update(retry);
            }
            return result;
        }

        public void AddEmailToContact(string email, ContentItem contact) {
            if (!string.IsNullOrEmpty(email)) {
                CommunicationEmailRecord cmr = null;
                if (contact != null) {
                    cmr = _repositoryCommunicationEmailRecord.Fetch(x => x.Email == email && x.EmailContactPartRecord_Id == contact.Id).FirstOrDefault();
                }
                if (cmr != null) {
                    if (cmr.EmailContactPartRecord_Id != contact.Id) {
                        cmr.EmailContactPartRecord_Id = contact.Id;
                        cmr.DataModifica = DateTime.Now;
                        _repositoryCommunicationEmailRecord.Update(cmr);
                        _repositoryCommunicationEmailRecord.Flush();
                    }
                }
                else {
                    CommunicationEmailRecord newrec = new CommunicationEmailRecord();
                    newrec.Email = email;
                    newrec.EmailContactPartRecord_Id = contact.Id;
                    newrec.Id = 0;
                    newrec.Validated = true;
                    newrec.DataInserimento = DateTime.Now;
                    newrec.DataModifica = DateTime.Now;
                    newrec.Produzione = true;
                    _repositoryCommunicationEmailRecord.Create(newrec);
                    _repositoryCommunicationEmailRecord.Flush();
                }
            }
        }
        public void AddSmsToContact(string pref, string num, ContentItem contact,bool overridexisting=true) {
            if (!string.IsNullOrEmpty(num)) {
                CommunicationContactPart ciCommunication = contact.As<CommunicationContactPart>();
                if (ciCommunication != null) {
                    CommunicationSmsRecord csr = _repositoryCommunicationSmsRecord.Fetch(x => x.SmsContactPartRecord_Id == ciCommunication.ContentItem.Id).FirstOrDefault();
                    if (csr == null) {
                        CommunicationSmsRecord newsms = new CommunicationSmsRecord();
                        newsms.Prefix = pref;
                        newsms.Sms = num;
                        newsms.SmsContactPartRecord_Id = ciCommunication.ContentItem.Id;
                        newsms.Id = 0;
                        newsms.Validated = true;
                        newsms.DataInserimento = DateTime.Now;
                        newsms.DataModifica = DateTime.Now;
                        newsms.Produzione = true;
                        _repositoryCommunicationSmsRecord.Create(newsms);
                        _repositoryCommunicationSmsRecord.Flush();
                    }
                    else {
                        if (overridexisting) {
                            csr.Prefix = pref;
                            csr.Sms = num;
                            csr.SmsContactPartRecord_Id = ciCommunication.ContentItem.Id;
                            csr.DataModifica = DateTime.Now;
                            _repositoryCommunicationSmsRecord.Update(csr);
                            _repositoryCommunicationSmsRecord.Flush();
                        }
                    }
                }
            }
        }
        private void CopyProfilePart(ContentItem src, ContentItem dest) {
            if((((dynamic)src).ProfilePart != null) && (((dynamic)dest).ProfilePart != null)) {
                List<ContentPart> Lcp = new List<ContentPart>();
                ContentPart destProfile = (ContentPart)((dynamic)dest).ProfilePart;
                Lcp.Add(destProfile);
                foreach (dynamic cf in ((dynamic)src).ProfilePart.Fields) {
                    if (cf is ICustomField) {
                        // laser custom field type
                        var valueList = ((ICustomField)cf).GetFieldValueList();
                        if (valueList != null) {
                            var destField = (ICustomField)(destProfile.Fields.FirstOrDefault(x => x.Name == cf.Name));
                            foreach (var val in valueList) {
                                destField.SetFieldValue(val.ValueName, val.Value);
                            }
                        }
                    } else { // orchard base field type
                        object myval;
                        if (cf.FieldDefinition.Name == typeof(DateTimeField).Name)
                            myval = ((object)(((dynamic)cf).DateTime));
                        else if (cf.FieldDefinition.Name == typeof(MediaLibraryPickerField).Name || cf.FieldDefinition.Name == typeof(ContentPickerField).Name)
                            myval = ((Int32[])cf.Ids).ToList().Select(x => (object)x).ToList();
                        else if (cf.FieldDefinition.Name == typeof(TaxonomyField).Name) {
                            List<TaxoVM> second = new List<TaxoVM>();
                            var selectedTerms = _taxonomyService.GetTermsForContentItem(src.Id, ((ContentField)cf).Name);
                            foreach (TermPart tp in selectedTerms) {
                                TaxoVM tv = new TaxoVM();
                                tv.Id = tp.Id;
                                tv.flag = true;
                                second.Add(tv);
                            }
                            myval = second;
                        } else if (cf.FieldDefinition.Name == typeof(LinkField).Name) {
                            LinkField linkField = (LinkField)cf;
                            var second = new LinkVM();
                            second.Url = linkField.Value;
                            second.Text = linkField.Text;
                            myval = second;
                        } else if (cf.FieldDefinition.Name == "SecureFileField") {
                            // Creating a dynamic variable to avoid adding reference to CloudConstruct.
                            dynamic secureField = new ExpandoObject();
                            secureField.Url = cf.Url;
                            secureField.Upload = cf.Upload;
                            myval = (object)secureField;
                        } else {
                            myval = ((object)(((dynamic)cf).Value));
                        }
                        _utilsServices.StoreInspectExpandoFields(Lcp, ((string)(cf.Name)), myval);
                    }
                }
            }
        }
        private void CopyPolicyAnswers(ContentItem src, ContentItem dest) {
            var srcPolicy = src.As<UserPolicyPart>();
            var destPolicy = dest.As<UserPolicyPart>();
            if(srcPolicy != null && destPolicy != null) {
                IPolicyServices policyServices = null;
                _orchardServices.WorkContext.TryResolve<IPolicyServices>(out policyServices);
                if(policyServices != null) {
                    var policyList = new List<PolicyForUserViewModel>();
                    var srcAnswers = policyServices.GetPolicyAnswersForContent(srcPolicy.Id);
                    foreach (var policy in srcAnswers) {
                        policyList.Add(new PolicyForUserViewModel {
                            PolicyTextId = policy.PolicyTextInfoPartRecord.Id,
                            Accepted = policy.Accepted,
                            AnswerDate = policy.AnswerDate,
                            UserId = (policy.UserPartRecord == null) ? null : (int?)policy.UserPartRecord.Id
                        });
                    }
                    policyServices.PolicyForItemMassiveUpdate(policyList, dest);
                }
            }
        }
        public void UserToContact(IUser userContent) {
            // verifiche preliminari
            if (userContent.Id == 0) {
                // non crea il contatto se lo user non è ancora stato salvato
                return;
            }
            var user = userContent.ContentItem;

            if (user.As<UserPart>().RegistrationStatus == UserStatus.Pending) { 
                return;
            }
            // identifica il Contact relativo a UserContent
            var contactsUsers = _orchardServices
                .ContentManager
                .Query<CommunicationContactPart, CommunicationContactPartRecord>()
                .Where(x => x.UserPartRecord_Id == userContent.Id)
                .List().FirstOrDefault();

            var typeIsDraftable = _contentDefinitionManager.GetTypeDefinition("CommunicationContact").Settings.GetModel<ContentTypeSettings>().Draftable;

            ContentItem contact = null;
            if (contactsUsers == null) {
                // cerca un eventuale contatto con la stessa mail ma non ancora legato a uno user
                var contactEmailList = GetContactsFromMail(userContent.Email);
                foreach (var contactEmail in contactEmailList) {
                    if ((contactEmail != null) && (contactEmail.ContentType == "CommunicationContact")) {
                        if ((contactEmail.As<CommunicationContactPart>().Record.UserPartRecord_Id == 0) && (contactEmail.As<CommunicationContactPart>().Master == false)) {
                            //contact = contactEmail;
                            contact = _orchardServices.ContentManager.Get(contactEmail.Id, typeIsDraftable ? VersionOptions.DraftRequired : VersionOptions.Latest);
                            contact.As<CommunicationContactPart>().Logs =
                                TruncateFromStart(contact.As<CommunicationContactPart>().Logs +
                                    string.Format(T("This contact has been bound to its user on {0:yyyy-MM-dd HH:mm} by contact synchronize function. ").Text,
                                    DateTime.Now), 4000); //4000 sembra essere la lunghezza massima gestita da NHibernate per gli nvarchar(max)
                            contact.As<CommunicationContactPart>().UserIdentifier = userContent.Id;
                            break; // associa solo il primo contatto che trova
                        }
                    }
                }

                if (contact == null) {
                    //even if typeIsDraftable == false, it's fine to create as Draft, because we are going to publish later in this method
                    //and creating as draft does the same things as not as draft, but sets Published = false already.
                    contact = _orchardServices.ContentManager.Create("CommunicationContact", VersionOptions.Draft);
                    contact.As<CommunicationContactPart>().Master = false;
                    contact.As<CommunicationContactPart>().UserIdentifier = userContent.Id;
                }
            }
            else {
                contact = _orchardServices.ContentManager.Get(contactsUsers.Id, typeIsDraftable ? VersionOptions.DraftRequired : VersionOptions.Latest);
            }

            #region aggiorna Pushcategories
            try {
                if (((dynamic)user).User.Pushcategories != null && (((dynamic)contact).CommunicationContactPart).Pushcategories != null) {
                    List<TermPart> ListTermPartToAdd = _taxonomyService.GetTermsForContentItem(userContent.Id, "Pushcategories").ToList();
                    _taxonomyService.UpdateTerms(contact, ListTermPartToAdd, "Pushcategories");
                }
            }
            catch { // non ci sono le Pushcategories
            }
            #endregion

            #region aggiorna FavoriteCulture
            try {
                if ((user.As<FavoriteCulturePart>() != null) && (contact.As<FavoriteCulturePart>() != null)) {
                    if (user.As<FavoriteCulturePart>().Culture_Id != 0) {
                        if (user.As<FavoriteCulturePart>().Culture_Id != contact.As<FavoriteCulturePart>().Culture_Id) {
                            contact.As<FavoriteCulturePart>().Culture_Id = user.As<FavoriteCulturePart>().Culture_Id;
                        }
                    }
                    else {
                        // imposta la culture di default
                        var defaultCultureId = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture()).Id;
                        contact.As<FavoriteCulturePart>().Culture_Id = defaultCultureId;
                        user.As<FavoriteCulturePart>().Culture_Id = defaultCultureId;
                    }
                }
            }
            catch { // non si ha l'estensione per favorite culture
            }
            #endregion

            #region aggiorna email
            if (!string.IsNullOrEmpty(userContent.Email) && user.As<UserPart>().RegistrationStatus == UserStatus.Approved) {
                AddEmailToContact(userContent.Email, contact);
            }
            #endregion

            #region aggiorna sms
            try {
                dynamic userPwdRecoveryPart = ((dynamic)user).UserPwdRecoveryPart;
                if (userPwdRecoveryPart != null) {
                    AddSmsToContact(userPwdRecoveryPart.InternationalPrefix, userPwdRecoveryPart.PhoneNumber, contact, true);
                }
            }
            catch {
                // non è abilitato il modulo Laser.Mobile.SMS, quindi non allineo il telefono
            }
            #endregion

            #region aggiorna Title
            if (string.IsNullOrWhiteSpace(userContent.UserName) == false) {
                contact.As<TitlePart>().Title = userContent.UserName;
            }
            else if (string.IsNullOrWhiteSpace(userContent.Email) == false) {
                contact.As<TitlePart>().Title = userContent.Email;
            }
            else {
                contact.As<TitlePart>().Title = string.Format("User with ID {0}", userContent.Id);
            }
            #endregion

            #region aggiorna CommonPart
            if (contact.Has<CommonPart>()) {
                contact.As<CommonPart>().ModifiedUtc = DateTime.Now;
                contact.As<CommonPart>().Owner = userContent;
            }
            #endregion

            CopyProfilePart(user, contact);

            CopyPolicyAnswers(user, contact);

            if (contact != null) {
                //Whether the type is draftable or not, we still want to publish it, so at worst setting Published = false does nothing
                contact.VersionRecord.Published = false;
                _orchardServices.ContentManager.Publish(contact);
            }
        }

        public void ContactToUser(ContentItem contact) {
            // verifiche preliminari
            if (contact.Id == 0) {
                // non aggiorna lo user se il contact non è ancora stato salvato
                return;
            }
            var contactPart = contact.As<CommunicationContactPart>();
            // identifica lo user corrispondente
            var userContent = _orchardServices
                .ContentManager
                .Query<UserPart, UserPartRecord>()
                .Where(x => x.Id == contactPart.UserIdentifier)
                .List().FirstOrDefault();
            // se non esiste l'utente corrispondente, esce
            if(userContent == null) {
                return;
            }
            var user = userContent.ContentItem;

            #region aggiorna Pushcategories
            try {
                if (((dynamic)user).User.Pushcategories != null && (((dynamic)contact).CommunicationContactPart).Pushcategories != null) {
                    List<TermPart> listTermPartToAdd = _taxonomyService.GetTermsForContentItem(contact.Id, "Pushcategories").ToList();
                    _taxonomyService.UpdateTerms(user, listTermPartToAdd, "Pushcategories");
                }
            } catch { // non ci sono le Pushcategories
            }
            #endregion

            #region aggiorna FavoriteCulture
            try {
                if ((user.As<FavoriteCulturePart>() != null) && (contact.As<FavoriteCulturePart>() != null)) {
                    if (contact.As<FavoriteCulturePart>().Culture_Id != 0) {
                        if (contact.As<FavoriteCulturePart>().Culture_Id != user.As<FavoriteCulturePart>().Culture_Id) {
                            user.As<FavoriteCulturePart>().Culture_Id = contact.As<FavoriteCulturePart>().Culture_Id;
                        }
                    } else {
                        // imposta la culture di default
                        var defaultCultureId = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture()).Id;
                        user.As<FavoriteCulturePart>().Culture_Id = defaultCultureId;
                        contact.As<FavoriteCulturePart>().Culture_Id = defaultCultureId;
                    }
                }
            } catch { // non si ha l'estensione per favorite culture
            }
            #endregion

            #region aggiorna email
            if (string.IsNullOrEmpty(userContent.Email)) {
                var cmr = _repositoryCommunicationEmailRecord.Fetch(x => x.EmailContactPartRecord_Id == contact.Id).FirstOrDefault();
                if(cmr != null) {
                    userContent.Email = cmr.Email;
                }
            }
            #endregion

            #region aggiorna sms
            try {
                dynamic userPwdRecoveryPart = ((dynamic)user).UserPwdRecoveryPart;
                if (userPwdRecoveryPart != null) {
                    if(string.IsNullOrWhiteSpace(userPwdRecoveryPart.PhoneNumber)) {
                        var csr = _repositoryCommunicationSmsRecord.Fetch(x => x.SmsContactPartRecord_Id == contact.Id).FirstOrDefault();
                        if (csr != null) {
                            userPwdRecoveryPart.InternationalPrefix = csr.Prefix;
                            userPwdRecoveryPart.PhoneNumber = csr.Sms;
                        }
                    }
                }
            } catch {
                // non è abilitato il modulo Laser.Mobile.SMS, quindi non allineo il telefono
            }
            #endregion

            CopyProfilePart(contact, user);

            CopyPolicyAnswers(contact, user);

            // force publish to update FieldIndexRecords
            user.VersionRecord.Published = false;
            _orchardServices.ContentManager.Publish(user);
        }
    }
}