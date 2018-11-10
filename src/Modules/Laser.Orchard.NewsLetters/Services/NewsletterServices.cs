using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using Laser.Orchard.Commons.Extensions;
using Laser.Orchard.NewsLetters.Extensions;
using Laser.Orchard.NewsLetters.Models;
using Laser.Orchard.NewsLetters.ViewModels;
using Laser.Orchard.StartupConfig.Extensions;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Email.Models;
using Orchard.Email.Services;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Mvc.Html;
using Orchard.UI.Notify;
using Orchard.Events;
using System.Diagnostics;
using Laser.Orchard.TemplateManagement.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Environment.Configuration;

namespace Laser.Orchard.NewsLetters.Services {
    public interface IJobsQueueService : IEventHandler {
        void Enqueue(string message, object parameters, int priority);
    }
    public class NewsletterServices : Laser.Orchard.NewsLetters.Services.INewsletterServices {
        private readonly IJobsQueueService _jobsQueueService;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IRepository<SubscriberRecord> _repositorySubscribers;
        private readonly IRepository<NewsletterDefinitionPartRecord> _repositoryNewsletterDefinition;
        private readonly IRepository<NewsletterEditionPartRecord> _repositoryNewsletterEdition;
        private readonly ITemplateService _templateService;
        private readonly IMessageService _messageService;
        private readonly INotifier _notifier;
        private readonly ICommonsServices _commonServices;
        private readonly ShellSettings _shellSettings;

        public NewsletterServices(
            IJobsQueueService jobsQueueService,
            IContentManager contentManager,
            IOrchardServices orchardServices,
            ITemplateService templateService,
            IMessageService messageService,
            IRepository<SubscriberRecord> repositorySubscribers,
            IRepository<NewsletterDefinitionPartRecord> repositoryNewsletterDefinition,
            IRepository<NewsletterEditionPartRecord> repositoryNewsletterEdition,
            ICommonsServices commonServices,
            INotifier notifier,
            ShellSettings shellSettings) {

            _notifier = notifier;
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _messageService = messageService;
            _repositorySubscribers = repositorySubscribers;
            _repositoryNewsletterDefinition = repositoryNewsletterDefinition;
            T = NullLocalizer.Instance;
            _templateService = templateService;
            _repositoryNewsletterEdition = repositoryNewsletterEdition;
            _jobsQueueService = jobsQueueService;
            _commonServices = commonServices;
            _shellSettings = shellSettings;
        }

        public Localizer T { get; set; }


        #region [ NewsletterDefinition ]
        public ContentItem GetNewsletterDefinition(int id, VersionOptions versionOptions) {
            var NewsletterDefinitionPart = _contentManager.Get<NewsletterDefinitionPart>(id, versionOptions);
            return NewsletterDefinitionPart == null ? null : NewsletterDefinitionPart.ContentItem;
        }

        public IEnumerable<NewsletterDefinitionPart> GetNewsletterDefinition() {
            return GetNewsletterDefinition(VersionOptions.Published);
        }

        public IEnumerable<NewsletterDefinitionPart> GetNewsletterDefinition(VersionOptions versionOptions) {
            return _contentManager.Query<NewsletterDefinitionPart, NewsletterDefinitionPartRecord>(versionOptions)
                .Join<TitlePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public IEnumerable<NewsletterDefinitionPart> GetNewsletterDefinition(string ids, VersionOptions versionOptions) {
            var newsIds = ids.Split(',').Select(s => Convert.ToInt32(s)).ToList();
            return _contentManager.Query<NewsletterDefinitionPart, NewsletterDefinitionPartRecord>(versionOptions)
                .Where(w => newsIds.Contains(w.Id))
                .Join<TitlePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public void DeleteNewsletterDefinition(ContentItem newsletterDefinition) {
            _contentManager.Remove(newsletterDefinition);
        }
        #endregion


        #region [ NewsletterEdition | Item]

        public ContentItem GetNewsletterEdition(int id, VersionOptions versionOptions) {
            var newsletterEditionPart = _contentManager.Get<NewsletterEditionPart>(id, versionOptions);
            return newsletterEditionPart == null ? null : newsletterEditionPart.ContentItem;
        }

        public IEnumerable<NewsletterEditionPart> GetNewsletterEditions(int newsletterId) {
            return GetNewsletterEditions(newsletterId, VersionOptions.Published);
        }

        public IEnumerable<NewsletterEditionPart> GetNewsletterEditions(int newsletterId, VersionOptions versionOptions) {
            return _contentManager.Query<NewsletterEditionPart, NewsletterEditionPartRecord>(versionOptions)
                .Where(w => w.NewsletterDefinitionPartRecord_Id == newsletterId)
                .Join<TitlePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public void DeleteNewsletterEdition(ContentItem newsletterDefinition) {
            _contentManager.Remove(newsletterDefinition);
        }

        public void SendNewsletterEdition(ref NewsletterEditionPart newsletterEdition, string testEmail) {
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var subscribers = GetSubscribers(newsletterEdition.NewsletterDefinitionPartRecord_Id).Where(w => w.Confirmed);
            int[] selectedAnnIds;
            IList<AnnouncementPart> items = null;
            IEnumerable<ExpandoObject> fullyItems;
            if (!String.IsNullOrWhiteSpace(newsletterEdition.AnnouncementIds)) {
                selectedAnnIds = !String.IsNullOrWhiteSpace(newsletterEdition.AnnouncementIds) ? newsletterEdition.AnnouncementIds.Split(',').Select(s => Convert.ToInt32(s)).ToArray() : null;
                items = GetAnnouncements(selectedAnnIds);
                fullyItems = items.Select(
                    s => new {
                        AnnouncementPart = s,
                        DisplayUrl = urlHelper.ItemDisplayUrl(s)
                    }.ToExpando());
            } else {
                fullyItems = null;
            }
            var model = new {
                NewsletterEdition = newsletterEdition,
                ContentItems = fullyItems
            }.ToExpando();
            //if (!isTest) {
            //  if (_templateService.SendTemplatedEmail((dynamic)model,
            //                                        GetNewsletterDefinition(newsletterEdition.NewsletterDefinitionPartRecord_Id,VersionOptions.Published).As<NewsletterDefinitionPart>().TemplateRecord_Id,
            //                                        new List<string> { testEmail }, null, null, false, null)) {

            //        // Aggiorno la newsletter edition, e rimuovo la relazione tra Newletter e Announcement 
            //        newsletterEdition.Dispatched = true;
            //        newsletterEdition.DispatchDate = DateTime.Now;
            //        newsletterEdition.Number = GetNextNumber(newsletterEdition.NewsletterDefinitionPartRecord_Id); ;
            //        foreach (var item in items) {
            //            var ids = ("," + item.AttachToNextNewsletterIds + ",").Replace("," + newsletterEdition.NewsletterDefinitionPartRecord_Id + ",", "");
            //            item.AttachToNextNewsletterIds = ids;
            //        }
            //    }
            //}
            if (!String.IsNullOrWhiteSpace(testEmail)) {

                string host = string.Format("{0}://{1}{2}",
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80 ? string.Empty : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);

                // Place Holder
                List<TemplatePlaceHolderViewModel> listaPH = new List<TemplatePlaceHolderViewModel>();

                string unsubscribe = T("Click here to unsubscribe").Text;
                string linkUnsubscribe = "<a href='" + string.Format("{0}{1}?newsletterId={2}", host, urlHelper.SubscriptionUnsubscribe(), newsletterEdition.NewsletterDefinitionPartRecord_Id) + "'>" + unsubscribe + "</a>";

                TemplatePlaceHolderViewModel ph = new TemplatePlaceHolderViewModel();
                ph.Name = "[UNSUBSCRIBE]";
                ph.Value = linkUnsubscribe;
                ph.ShowForce = true;

                listaPH.Add(ph);

                if (_templateService.SendTemplatedEmail((dynamic)model,
                                                        GetNewsletterDefinition(newsletterEdition.NewsletterDefinitionPartRecord_Id, VersionOptions.Published).As<NewsletterDefinitionPart>().TemplateRecord_Id,
                                                        new List<string> { testEmail }, null, null, false, listaPH))
                
                    _orchardServices.Notifier.Information(T("Newsletter edition sent to a test email!"));
            }
            else if (String.IsNullOrWhiteSpace(testEmail)) {
                _orchardServices.Notifier.Error(T("Enter a test email!"));
            }
        }

        public IList<AnnouncementLookupViewModel> GetNextAnnouncements(int newsltterId, int[] selectedIds) {
            var list = _contentManager.Query<AnnouncementPart, AnnouncementPartRecord>(VersionOptions.Published)
                .Where(w => w.AttachToNextNewsletterIds.Contains(newsltterId.ToString()))
                .OrderBy(br => br.AnnouncementTitle)
                .List()
                .Select(s => new AnnouncementLookupViewModel {
                    Id = s.Id,
                    Title = String.IsNullOrWhiteSpace(s.AnnouncementTitle) ? s.ContentItem.As<TitlePart>().Title : s.AnnouncementTitle,
                    Selected = selectedIds != null && selectedIds.Contains(s.Id)
                });
            return list.ToList();
        }

        public IList<AnnouncementPart> GetAnnouncements(int[] selectedIds) {
            var list = _contentManager.Query<AnnouncementPart, AnnouncementPartRecord>(VersionOptions.Published)
                .Where(w => selectedIds.Contains(w.Id))
                .OrderBy(br => br.AnnouncementTitle)
                .List();
            return list.ToList();

        }

        public int GetNextNumber(int newsltterId) {
            var maxNumber = _repositoryNewsletterEdition.Table
                 .Where(w => w.NewsletterDefinitionPartRecord_Id == newsltterId)
                 .Select(s => s.Number)
                 .Max();
            return (maxNumber.HasValue ? maxNumber.Value + 1 : 1);
        }
        #endregion


        #region [ Subscribers ]

        public SubscriberRecord GetSubscriber(int id) {
            var subscriber = _repositorySubscribers
                .Get(id);
            return subscriber;
        }

        public IEnumerable<SubscriberRecord> GetSubscribers(int newsletterId) {
            var subscriber = _repositorySubscribers
                .Fetch(w => w.NewsletterDefinition.Id == newsletterId);
            return subscriber;
        }

        public SubscriberRecord TryRegisterSubscriber(SubscriberViewModel subscriber) {
            SubscriberRecord returnValue = null;
            try {
                var subs = _repositorySubscribers.Table.Where(w => w.Email == subscriber.Email &&
                                                                   w.NewsletterDefinition.Id == subscriber.NewsletterDefinition_Id).SingleOrDefault();

                string guid = Guid.NewGuid().ToString();

                // Create Nonce
                string parametri = "Email=" + subscriber.Email + "&Guid=" + guid;
                TimeSpan delay = new TimeSpan(1, 0, 0);
                string Nonce = _commonServices.CreateNonce(parametri, delay);

                if (subs == null) {
                    subs = new SubscriberRecord {
                        Email = subscriber.Email,
                        Guid = guid,
                        Confirmed = false,
                        SubscriptionDate = DateTime.Now,
                        SubscriptionKey = Nonce,
                        Name = subscriber.Name,
                        NewsletterDefinition = _repositoryNewsletterDefinition.Get(subscriber.NewsletterDefinition_Id)
                    };

                    try {
                        _repositorySubscribers.Create(subs);
                        returnValue = subs;
                    }
                    catch (Exception ex) {
                        _orchardServices.Notifier.Error(T(ex.Message));
                        returnValue = null;
                    }
                }
                else if (!subs.Confirmed) {
                    subs.Name = subscriber.Name;
                    subs.SubscriptionDate = DateTime.Now;
                    subs.SubscriptionKey = Nonce;
                    subs.UnsubscriptionDate = null;

                    try {
                        _repositorySubscribers.Update(subs);
                        returnValue = subs;
                    }
                    catch (Exception ex) {
                        _orchardServices.Notifier.Error(T(ex.Message));
                        returnValue = null;
                    }
                }
                else {
                    _orchardServices.Notifier.Information(T("Email already registered!"));
                }
                if (returnValue != null) {
                    // Encode Nonce
                    string parametriEncode = System.Web.HttpUtility.HtmlEncode(Nonce.Replace('+', '_').Replace('/', '-'));

                    string host = string.Format("{0}://{1}{2}",
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80 ? string.Empty : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);

                    var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

                    // Model template Mail
                    dynamic viewModel = new SubscriberViewModel {
                        Email = subs.Email,
                        Name = subs.Name,
                        Guid = subs.Guid,
                        LinkSubscription = string.Format("{0}{1}?key={2}", host, urlHelper.SubscriptionConfirmSubscribe(), parametriEncode),
                        KeySubscription = Nonce,
                        NewsletterDefinition_Id = subs.NewsletterDefinition.Id,
                        NewsletterDefinition = _contentManager.Get(subs.NewsletterDefinition.Id)
                    };

                    _templateService.SendTemplatedEmail(viewModel,
                                                        subs.NewsletterDefinition.ConfirmSubscrptionTemplateRecord_Id,
                                                        new List<string> { subs.Email }, null, null, false, null);
                }
            }
            catch {
                returnValue = null;
            }

            return returnValue;
        }

        public SubscriberRecord TryRegisterConfirmSubscriber(string keySubscribe) {
            string parametri = "";

            // Ritorno al Nonce originale
            keySubscribe = System.Web.HttpUtility.HtmlDecode(keySubscribe.Replace('_', '+').Replace('-', '/'));

            // Verifico che non sia passata più di 1 ora dalla richiesta di Unsubscribe
            bool decryptOk = _commonServices.DecryptNonce(keySubscribe, out parametri);

            if (!decryptOk) {
                _orchardServices.Notifier.Error(T("Subscribe impossible. Please try again."));
                return null;
            }

            string[] infoKey = parametri.Split('&');
            string[] parEmail = infoKey[0].Split('=');
            string email = parEmail[1];

            var subs = _repositorySubscribers.Table.Where(w => w.Email == email && w.SubscriptionKey == keySubscribe).SingleOrDefault();

            if (subs == null) {
                _orchardServices.Notifier.Error(T("Subscriber not found!"));
                return null;
            }

            if (subs.Confirmed) {
                _orchardServices.Notifier.Error(T("Email already registered!"));
                    return null;
            }
            
            subs.SubscriptionKey = null;
            subs.ConfirmationDate = DateTime.Now;
            subs.Confirmed = true;

            try {
                _repositorySubscribers.Update(subs);
                return subs;
            }
            catch (Exception ex) {
                _orchardServices.Notifier.Error(T(ex.Message));
                return null;
            }
        }

        public SubscriberRecord TryUnregisterSubscriber(SubscriberViewModel subscriber) {
            SubscriberRecord returnValue = null;
            try {
                var subs = _repositorySubscribers.Table.Where(w => w.Email == subscriber.Email &&
                    w.NewsletterDefinition.Id == subscriber.NewsletterDefinition_Id).SingleOrDefault();
                if (subs == null) {
                    _orchardServices.Notifier.Error(T("Email not found!"));
                    return null;
                }
                else if (subs.Confirmed) {
                    returnValue = subs;
                }
                else {
                    _orchardServices.Notifier.Error(T("Email not found!"));
                    return null;
                }
                if (returnValue != null) {

                    // Create Nonce
                    string parametri = "Email=" + subs.Email + "&Guid=" + subs.Guid;
                    TimeSpan delay = new TimeSpan(1, 0, 0);
                    string Nonce = _commonServices.CreateNonce(parametri, delay);

                    // Update SubscriberRecord - Save Nonce
                    returnValue.UnsubscriptionKey = Nonce;
                    _repositorySubscribers.Update(returnValue);

                    // Encode Nonce
                    string parametriEncode = System.Web.HttpUtility.HtmlEncode(Nonce.Replace('+', '_').Replace('/', '-'));

                    string host = string.Format("{0}://{1}{2}",
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                     _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80 ? string.Empty : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);

                    var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

                    // Model template Mail
                    dynamic viewModel = new SubscriberViewModel {
                        Email = subs.Email,
                        Name = subs.Name,
                        LinkUnsubscription = string.Format("{0}{1}?key={2}", host, urlHelper.SubscriptionConfirmUnsubscribe(), parametriEncode),
                        KeyUnsubscription = Nonce,
                        NewsletterDefinition_Id = subs.NewsletterDefinition.Id,
                        NewsletterDefinition = _contentManager.Get(subs.NewsletterDefinition.Id)
                    };

                    _templateService.SendTemplatedEmail(viewModel,
                                                        subs.NewsletterDefinition.DeleteSubscrptionTemplateRecord_Id,
                                                        new List<string> { subs.Email }, null, null, false, null);
                }
            }
            catch {
                returnValue = null;
            }

            return returnValue;
        }

        public SubscriberRecord TryUnregisterConfirmSubscriber(string keyUnsubscribe) {
            string parametri = "";

            // Ritorno al Nonce originale
            keyUnsubscribe = System.Web.HttpUtility.HtmlDecode(keyUnsubscribe.Replace('_', '+').Replace('-', '/'));

            // Verifico che non sia passata più di 1 ora dalla richiesta di Unsubscribe
            bool decryptOk = _commonServices.DecryptNonce(keyUnsubscribe, out parametri);

            if (!decryptOk) {
                _orchardServices.Notifier.Error(T("Unsubscribe impossible. Please try again."));
                return null;
            }

            string[] infoKey = parametri.Split('&');
            string[] parEmail = infoKey[0].Split('=');
            string email = parEmail[1];

            // Verifico tra gli iscritti sia presente la mail e la key (nonce)
            var subs = _repositorySubscribers.Table.Where(w => w.Email == email && w.UnsubscriptionKey.Equals(keyUnsubscribe)).SingleOrDefault();

            if (subs == null) {
                _orchardServices.Notifier.Error(T("Subscriber not found!"));
                return null;
            }

            if (!subs.Confirmed) {
                _orchardServices.Notifier.Error(T("Email not yet registered!"));
                return null;
            }

            subs.UnsubscriptionKey = null;
            subs.UnsubscriptionDate = DateTime.Now;
            subs.Confirmed = false;

            try 
            {
                _repositorySubscribers.Update(subs);
                return subs;
            } 
            catch (Exception ex) {
                _orchardServices.Notifier.Error(T(ex.Message));
                return null;
            }
        }
        
        #endregion
    }
}