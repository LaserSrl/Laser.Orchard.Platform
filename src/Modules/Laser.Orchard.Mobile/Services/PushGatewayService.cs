using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Xml.Linq;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Settings;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.Queries.Models;
using Laser.Orchard.Queries.Services;
using Newtonsoft.Json.Linq;
using NHibernate.Transform;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using OrchardLogging = Orchard.Logging;
using Orchard.Tokens;
using Orchard.UI.Notify;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Google;
using PushSharp.Windows;
using Orchard.Core.Common.Fields;
using Laser.Orchard.ShortLinks.Services;
using NHibernate;
using NHibernate.Criterion;

namespace Laser.Orchard.Mobile.Services {

    public interface IPushGatewayService : IDependency {

        IList GetPushQueryResult(Int32[] ids, bool countOnly = false, int contentId = 0);

        IList GetPushQueryResult(Int32[] ids, TipoDispositivo? tipodisp, bool produzione, string language, bool countOnly = false, ContentItem advItem = null);

        List<PushNotificationRecord> GetPushQueryResultByUserNames(string[] userNames, TipoDispositivo? tipodisp, bool produzione, string language);

        IList CountPushQueryResultByUserNames(string[] userNames, TipoDispositivo? tipodisp, bool produzione, string language);

        void PublishedPushEventTest(ContentItem ci);

        PushState PublishedPushEvent(ContentItem ci);

        IList<IDictionary> GetContactsWithDevice(string nameFilter);

        void SendPushToContact(ContentItem ci, string contactTitle);

        NotificationsCounters GetNotificationsCounters(ContentItem ci);
        void ResetNotificationFailures(ContentItem ci);
    }

    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushGatewayService : IPushGatewayService {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IQueryPickerService _queryPickerServices;
        private readonly IOrchardServices _orchardServices;
        private readonly ITransactionManager _transactionManager;
        private readonly IRepository<SentRecord> _sentRepository;
        private readonly IRepository<PushNotificationRecord> _pushNotificationRepository;
        private readonly INotifier _notifier;
        private readonly ICommunicationService _communicationService;
        private readonly ITokenizer _tokenizer;
        private readonly ShellSettings _shellSetting;
        private readonly IShortLinksService _shortLinksService;
        public Localizer T { get; set; }
        private const string outcomeToTryAgain = "ko";

        public OrchardLogging.ILogger Logger { get; set; }

        private int _messageSent; // numero di push con esito positivo
        private int _pushNumber; // numero di push aggiunte alla coda di invio
        private const int maxPushtextLength = 160;
        private PushState _result;

        private ContentItem senderContentItemContainer;
        private ConcurrentDictionary<string, SentRecord> _sentRecords;
        private ConcurrentBag<DeviceChange> _deviceChanges;
        private ConcurrentBag<DeviceChange> _deviceExpired;

        public PushGatewayService(IPushNotificationService pushNotificationService, IQueryPickerService queryPickerServices, IOrchardServices orchardServices, ITransactionManager transactionManager, IRepository<SentRecord> sentRepository, IRepository<PushNotificationRecord> pushNotificationRepository, INotifier notifier, ICommunicationService communicationService, ITokenizer tokenizer, ShellSettings shellSetting, IShortLinksService shortLinksService) {
            _pushNotificationService = pushNotificationService;
            _queryPickerServices = queryPickerServices;
            _orchardServices = orchardServices;
            _transactionManager = transactionManager;
            _sentRepository = sentRepository;
            _pushNotificationRepository = pushNotificationRepository;
            _notifier = notifier;
            _communicationService = communicationService;
            _tokenizer = tokenizer;
            _shellSetting = shellSetting;
            _shortLinksService = shortLinksService;
            _messageSent = 0;
            _pushNumber = 0;

            Logger = OrchardLogging.NullLogger.Instance;
        }
        public NotificationsCounters GetNotificationsCounters(ContentItem ci) {
            var result = new NotificationsCounters();
            // esclude gli invii ripetuti dal conteggio
            result.TotSent = _sentRepository.Count(x => x.PushedItem == ci.Id && x.Repeatable == false);
            result.TotOk = _sentRepository.Count(x => x.PushedItem == ci.Id && x.Outcome == "ok" && x.Repeatable == false);
            result.TotToTryAgain = _sentRepository.Count(x => x.PushedItem == ci.Id && x.Outcome == outcomeToTryAgain && x.Repeatable == false);
            result.TotPending = _sentRepository.Count(x => x.PushedItem == ci.Id && x.Outcome == "" && x.Repeatable == false);
            return result;
        }
        private void NotifySentNumber(int sentNumber) {
            // notifica solo se la richiesta arriva direttamente da web, non se si tratta di un task
            if (System.Web.HttpContext.Current != null) {
                _notifier.Information(T("Notifications sent: {0}", sentNumber));
            }
        }
        private void NotifyNoPush() {
            // notifica solo se la richiesta arriva direttamente da web, non se si tratta di un task
            if (System.Web.HttpContext.Current != null) {
                _notifier.Information(T("No push will be sent, related content must be published"));
            }
        }
        private void NotifyApplePayloadExceed() {
            // notifica solo se la richiesta arriva direttamente da web, non se si tratta di un task
            if (System.Web.HttpContext.Current != null) {
                _notifier.Information(T("Apple message payload exceeds the limit"));
            }
        }
        public IList GetPushQueryResult(Int32[] ids, bool countOnly = false, int contentId = 0) {
            ContentItem contentItem = null;
            if (contentId > 0) {
                contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);
            }
            return GetPushQueryResult(ids, null, true, "All", countOnly, contentItem);
        }

        public IList<IDictionary> GetContactsWithDevice(string nameFilter) {
            string query;
            string groupby;
            IList<IDictionary> lista;
            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            if (nameFilter.StartsWith("token:")) {
                string[] machineNameCheck = GetMachineNames();
                var session = _transactionManager.GetSession();
                ICriteria criteria = session.CreateCriteria<Laser.Orchard.Mobile.Models.PushNotificationRecord>();
                criteria.SetMaxResults(10);
                criteria.SetProjection(
                    NHibernate.Criterion.Projections.ProjectionList()
                        .Add(NHibernate.Criterion.Projections.Alias(
                            NHibernate.Criterion.Projections.SqlFunction(
                            "concat",
                            NHibernateUtil.String,
                                NHibernate.Criterion.Projections.Constant("token:"),
                                NHibernate.Criterion.Projections.GroupProperty("Token"))
                            , "Title"))
                        .Add(NHibernate.Criterion.Projections.Alias(NHibernate.Criterion.Projections.Count("Token"), "NumDevice"))
                );
                var conjunction = Restrictions.Conjunction();
                conjunction.Add(Restrictions.Eq("Validated", true));
                conjunction.Add(Restrictions.Eq("Token", nameFilter.Replace("token:", "").Trim()));
                conjunction.Add(Restrictions.Eq("RegistrationUrlHost", hostCheck));
                conjunction.Add(Restrictions.Eq("RegistrationUrlPrefix", prefixCheck));
                conjunction.Add(Restrictions.In("RegistrationMachineName", machineNameCheck));
                criteria.Add(conjunction);
                lista = criteria
                    .SetResultTransformer(Transformers.AliasToEntityMap)
                    .List<IDictionary>();
            } else {
                string machineNameCheck = GetMachineNamesForSql();
                query = "SELECT tp.Title as Title, count(MobileRecord.Id) as NumDevice" +
                    " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr " +
                    " join civr.ContentItemRecord as cir " +
                    " join cir.CommunicationContactPartRecord as CommunicationContact " +
                    " join cir.MobileContactPartRecord as MobileContact " +
                    " join civr.TitlePartRecord as tp" +
                    " join MobileContact.MobileRecord as MobileRecord " +
                    " WHERE civr.Published=1 AND MobileRecord.Validated" +
                    " AND tp.Title like '%" + nameFilter.Replace("'", "''") + "%'";
                groupby = " GROUP BY tp.Title";
                query += string.Format(" AND MobileRecord.RegistrationUrlHost='{0}' AND MobileRecord.RegistrationUrlPrefix='{1}' AND MobileRecord.RegistrationMachineName IN ({2})", hostCheck.Replace("'", "''"), prefixCheck.Replace("'", "''"), machineNameCheck);
                query += groupby;
                var fullStatement = _transactionManager.GetSession()
                    .CreateQuery(query)
                    .SetCacheable(false);
                lista = fullStatement
                    .SetResultTransformer(Transformers.AliasToEntityMap)
                     .List<IDictionary>();

            }
            return lista;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids">Query Ids array</param>
        /// <param name="tipodisp">DeviceType Android|Apple|WindowsMobile</param>
        /// <param name="produzione">Device registration context</param>
        /// <param name="language"></param>
        /// <param name="countOnly"></param>
        /// <param name="advItem">ContentItem representing the Container object</param>
        /// <returns></returns>
        public IList GetPushQueryResult(Int32[] ids, TipoDispositivo? tipodisp, bool produzione, string language, bool countOnly = false, ContentItem advItem = null) {
            IHqlQuery query;
            if (ids != null && ids.Count() > 0) {
                Dictionary<string, object> tokens = new Dictionary<string, object>();
                if (advItem != null) {
                    tokens.Add("Content", advItem);
                } else if (senderContentItemContainer != null) {
                    tokens.Add("Content", senderContentItemContainer);
                }
                query = IntegrateAdditionalConditions(_queryPickerServices.GetCombinedContentQuery(ids, tokens, new string[] { "CommunicationContact" }));
            } else {
                query = IntegrateAdditionalConditions(null);
            }

            // Trasformo in stringa HQL
            var stringHQL = ((DefaultHqlQuery)query).ToHql(false);

            // Rimuovo la Order by per poter fare la query annidata
            // TODO: trovare un modo migliore per rimuovere la order by
            // provare a usare: query.OrderBy(null, null);
            stringHQL = stringHQL.ToString().Replace("order by civ.Id", "");

            string queryForPush = "";
            if (countOnly) {
                queryForPush = "SELECT count(MobileRecord) as Tot, sum(case MobileRecord.Device when 'Android' then 1 else 0 end) as Android, sum(case MobileRecord.Device when 'Apple' then 1 else 0 end) as Apple, sum(case MobileRecord.Device when 'WindowsMobile' then 1 else 0 end) as WindowsMobile";
            } else {
                queryForPush = "SELECT MobileRecord.Id as Id, MobileRecord.Device as Device, MobileRecord.Produzione as Produzione, MobileRecord.Validated as Validated, MobileRecord.Language as Language, MobileRecord.UUIdentifier as UUIdentifier, MobileRecord.Token as Token, MobileRecord.RegistrationUrlHost as RegistrationUrlHost, MobileRecord.RegistrationUrlPrefix as RegistrationUrlPrefix, MobileRecord.RegistrationMachineName as RegistrationMachineName";
            }
            queryForPush += " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr " +
                "join civr.ContentItemRecord as cir " +
                "join cir.CommunicationContactPartRecord as CommunicationContact " +
                "join cir.MobileContactPartRecord as MobileContact " +
                "join MobileContact.MobileRecord as MobileRecord " +
                "WHERE civr.Published=1 AND MobileRecord.Validated";
            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string machineNameCheck = GetMachineNamesForSql();

            queryForPush += string.Format(" AND MobileRecord.RegistrationUrlHost='{0}' AND MobileRecord.RegistrationUrlPrefix='{1}' AND MobileRecord.RegistrationMachineName in ({2})", hostCheck.Replace("'", "''"), prefixCheck.Replace("'", "''"), machineNameCheck);
            if (tipodisp.HasValue) {
                queryForPush += " AND MobileRecord.Device='" + tipodisp.Value + "'";
            }
            if (language != "All") {
                queryForPush += " AND MobileRecord.Language='" + language.Replace("'", "''") + "'"; // sostituzione anti sql-injection
            }
            queryForPush += " AND MobileRecord.Produzione=" + ((produzione) ? "1" : "0");
            if ((ids != null) && (ids.Count() > 0)) {
                // tiene conto degli id selezionati ma aggiunge comunque i device del master contact
                queryForPush += " AND (civr.Id in (" + stringHQL + ") "
                    // + "OR CommunicationContact.Master"
                    + ")";
            }

            // Creo query ottimizzata per le performance
            var fullStatement = _transactionManager.GetSession()
                .CreateQuery(queryForPush)
                .SetCacheable(false);
            var lista = fullStatement
                .SetResultTransformer(Transformers.AliasToEntityMap)
                 .List();
            return lista;
        }
        private string GetPartialPushQueryByUserNames(string[] userNames, TipoDispositivo? tipodisp, bool produzione, string language) {
            var userNamesCSV = String.Join(",", userNames.Select(x => "'" + x.ToLower().Replace("'", "''") + "'"));
            var query = " FROM Laser.Orchard.Mobile.Models.PushNotificationRecord as pnr, " +
            " Laser.Orchard.Mobile.Models.UserDeviceRecord as udr " +
            " join udr.UserPartRecord upr " +
            " WHERE pnr.Validated AND upr.RegistrationStatus = 'Approved' " +
            " AND pnr.UUIdentifier=udr.UUIdentifier " +
            " AND (upr.UserName IN (" + userNamesCSV + ") OR upr.Email IN (" + userNamesCSV + ") )";
            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string machineNameCheck = GetMachineNamesForSql();
            query += string.Format(" AND pnr.RegistrationUrlHost='{0}' AND pnr.RegistrationUrlPrefix='{1}' AND pnr.RegistrationMachineName in ({2})", hostCheck.Replace("'", "''"), prefixCheck.Replace("'", "''"), machineNameCheck);
            if (tipodisp.HasValue) {
                query += " AND pnr.Device='" + tipodisp.Value + "'";
            }
            if (language != "All") {
                query += " AND pnr.Language='" + language.Replace("'", "''") + "'"; // sostituzione anti sql-injection
            }
            query += " AND pnr.Produzione=" + ((produzione) ? "1" : "0");
            return query;
        }
        public IList CountPushQueryResultByUserNames(string[] userNames, TipoDispositivo? tipodisp, bool produzione, string language) {
            if (userNames.Length <= 0) return null;
            var query = "SELECT count(pnr) as Tot, sum(case pnr.Device when 'Android' then 1 else 0 end) as Android, sum(case pnr.Device when 'Apple' then 1 else 0 end) as Apple, sum(case pnr.Device when 'WindowsMobile' then 1 else 0 end) as WindowsMobile"
                     + GetPartialPushQueryByUserNames(userNames, tipodisp, produzione, language);
            var fullStatement = _transactionManager.GetSession()
                .CreateQuery(query)
                .SetCacheable(false);
            var elenco = fullStatement
                .SetResultTransformer(Transformers.AliasToEntityMap)
                 .List();
            return elenco;
        }
        /// <summary>
        /// from a  given list of UserNames returns list of associated devices or a list af one single row having devices count organised by DeviceType
        /// </summary>
        /// <param name="userNames">string array containing User names or email of the contacts to push</param>
        /// <param name="countOnly">if true a single line list will be returned having device count by DeviceType</param>
        /// <returns></returns>
        public List<PushNotificationRecord> GetPushQueryResultByUserNames(string[] userNames, TipoDispositivo? tipodisp, bool produzione, string language) {
            if (userNames.Length <= 0) return null;
            var query = "SELECT pnr " + GetPartialPushQueryByUserNames(userNames, tipodisp, produzione, language);
            var fullStatement = _transactionManager.GetSession()
                .CreateQuery(query)
                .SetCacheable(false);
            return fullStatement.List<PushNotificationRecord>().ToList();
        }

        private List<PushNotificationRecord> GetDevicesByContact(string contactTitle) {
            string query = "SELECT MobileRecord" +
                " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr " +
                " join civr.ContentItemRecord as cir " +
                " join cir.CommunicationContactPartRecord as CommunicationContact " +
                " join cir.MobileContactPartRecord as MobileContact " +
                " join civr.TitlePartRecord as tp" +
                " join MobileContact.MobileRecord as MobileRecord " +
                " WHERE civr.Published=1 AND MobileRecord.Validated" +
                " AND tp.Title='" + contactTitle.Replace("'", "''") + "'";
            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string machineNameCheck = GetMachineNamesForSql();
            query += string.Format(" AND MobileRecord.RegistrationUrlHost='{0}' AND MobileRecord.RegistrationUrlPrefix='{1}' AND MobileRecord.RegistrationMachineName in ({2})", hostCheck.Replace("'", "''"), prefixCheck.Replace("'", "''"), machineNameCheck);
            var fullStatement = _transactionManager.GetSession()
                .CreateQuery(query)
                .SetCacheable(false);
            var elenco = fullStatement
                 .List<PushNotificationRecord>();
            return elenco.ToList();
        }
        private List<PushNotificationRecord> GetListMobileDeviceByUserNames(string[] userNames) {
            return GetPushQueryResultByUserNames(userNames, null, true, "All");
        }
        private List<PushNotificationRecord> GetDevicesByToken(string token) {
            var query = "SELECT MobileRecord " +
                " FROM Laser.Orchard.Mobile.Models.PushNotificationRecord as MobileRecord " +
                " WHERE MobileRecord.Validated" +
                " AND MobileRecord.Token='" + token.Replace("'", "''").Replace("token:", "").Trim() + "'";
            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string machineNameCheck = GetMachineNamesForSql();
            query += string.Format(" AND MobileRecord.RegistrationUrlHost='{0}' AND MobileRecord.RegistrationUrlPrefix='{1}' AND MobileRecord.RegistrationMachineName in ({2})", hostCheck.Replace("'", "''"), prefixCheck.Replace("'", "''"), machineNameCheck);
            var fullStatement = _transactionManager.GetSession()
                .CreateQuery(query)
                .SetCacheable(false);
            var elenco = fullStatement
                 .List<PushNotificationRecord>();
            return elenco.ToList();
        }

        private IHqlQuery IntegrateAdditionalConditions(IHqlQuery query) {
            if (query == null) {
                query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "MobileContact" });
            }
            query = query
                .Where(x => x.ContentPartRecord<MobileContactPartRecord>(), x => x.IsNotEmpty("MobileRecord"));

            return query;
        }

        public void SendPushToContact(ContentItem ci, string contactTitle) {
            _result = null;
            // ricava l'elenco dei device
            List<PushNotificationRecord> elencoDevice;
            if (contactTitle.StartsWith("token:")) {
                elencoDevice = GetDevicesByToken(contactTitle.Replace("token:", ""));
            } else {
                elencoDevice = GetDevicesByContact(contactTitle);
            }
            MobilePushPart mpp = ci.As<MobilePushPart>();
            // ricava l'id del related content
            int idContentRelated = 0;
            var relatedContent = ((dynamic)ci).MobilePushPart.RelatedContent;
            // nel caso in cui la MobilePushPart sia contenuta nel content type CommunicationAdvertising, usa il related content di quest'ultimo
            if (ci.ContentType == "CommunicationAdvertising") {
                relatedContent = ((dynamic)ci).CommunicationAdvertisingPart.ContentLinked;
            }
            if (relatedContent != null && relatedContent.Ids != null && ((int[])relatedContent.Ids).Count() > 0) {
                idContentRelated = relatedContent.Ids[0];
            }

            // compone il messaggio da inviare
            PushMessage pushMessage = GeneratePushMessage(mpp, ci.Id, idContentRelated);

            // invia la push a ogni device
            List<PushNotificationRecord> singoloDevice = null;
            foreach (var device in elencoDevice) {
                singoloDevice = new List<PushNotificationRecord>();
                singoloDevice.Add(device);
                if (device.Device == TipoDispositivo.Android) {
                    PushAndroid(singoloDevice, device.Produzione, pushMessage, true);
                }
                if (device.Device == TipoDispositivo.Apple) {
                    PushApple(singoloDevice, device.Produzione, pushMessage, true);
                }
                if (device.Device == TipoDispositivo.WindowsMobile) {
                    PushWindows(singoloDevice, device.Produzione, pushMessage, true);
                }
            }
        }

        public void PublishedPushEventTest(ContentItem ci) {
            _result = null;
            int maxIdVersionRecord = _orchardServices.ContentManager.GetAllVersions(ci.Id).Max(x => x.VersionRecord.Id);
            ContentItem savedCi = _orchardServices.ContentManager.GetAllVersions(ci.Id).Where(x => x.VersionRecord.Id == maxIdVersionRecord).FirstOrDefault();
            MobilePushPart mpp = ci.As<MobilePushPart>();
            Int32 idContent = mpp.Id;
            var relatedContent = ((dynamic)ci).MobilePushPart.RelatedContent;

            // nel caso in cui la MobilePushPart sia contenuta nel content type CommunicationAdvertising, usa il related content di quest'ultimo
            if (ci.ContentType == "CommunicationAdvertising") {
                relatedContent = ((dynamic)savedCi).CommunicationAdvertisingPart.ContentLinked;
            }

            ContentItem relatedContentItem = null;
            Int32 idContentRelated = 0;
            dynamic contentForPush;
            dynamic ciDynamic = (dynamic)ci;

            if (relatedContent != null && relatedContent.Ids != null && ((int[])relatedContent.Ids).Count() > 0) {
                idContentRelated = relatedContent.Ids[0];
                relatedContentItem = _orchardServices.ContentManager.Get(idContentRelated);
                if (relatedContentItem == null) {
                    relatedContentItem = _orchardServices.ContentManager.GetLatest(idContentRelated);
                }
                contentForPush = (dynamic)relatedContentItem;
            } else {
                contentForPush = ciDynamic;
            }

            // tipo dispositivo (Android, Apple, Windows)
            TipoDispositivo? locTipoDispositivo = null;
            if (mpp.DevicePush != "All") {
                TipoDispositivo auxTipoDispositivo;
                if (Enum.TryParse<TipoDispositivo>(mpp.DevicePush, out auxTipoDispositivo)) {
                    locTipoDispositivo = auxTipoDispositivo;
                }
            }

            // determina il language
            string language = _orchardServices.WorkContext.CurrentSite.SiteCulture;
            try {
                language = contentForPush.LocalizationPart.Culture != null ? contentForPush.LocalizationPart.Culture.Culture : language;
            } catch {
                language = "All";
            }

            // determina le query impostate
            int[] ids = null;
            var aux = ci.As<QueryPickerPart>();
            if (aux != null) {
                ids = aux.Ids;
            }

            bool produzione = false;

            var Myobject = new Dictionary<string, object> { { "Content", ci } };
            string queryDevice = GetQueryDevice(Myobject, ci.As<MobilePushPart>());

            // it is a test so all submissions are repeatable
            if (locTipoDispositivo.HasValue == false) {// tutti
                SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
                SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
                SendAllWindowsPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
            } else if (locTipoDispositivo.Value == TipoDispositivo.Android) {
                SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
            } else if (locTipoDispositivo.Value == TipoDispositivo.Apple) {
                SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
            } else if (locTipoDispositivo.Value == TipoDispositivo.WindowsMobile) {
                SendAllWindowsPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids, repeatable: true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ci"></param>
        /// <returns>An error list, if any.</returns>
        public PushState PublishedPushEvent(ContentItem ci) {
            _result = new PushState();
            _result.CompletedIteration = true;
            senderContentItemContainer = ci;
            bool SendPushToSpecificDevices = false;
            try {
                LogInfo("Iniziato invio Push del content " + ci.Id);
                ContentItem savedCi = _orchardServices.ContentManager.Get(ci.Id);
                MobilePushPart mpp = ci.As<MobilePushPart>();
                _messageSent = 0;
                SendPushToSpecificDevices = mpp.UseRecipientList;
                if (mpp.ToPush) {
                    bool stopPush = false;
                    Int32 idContent = mpp.Id;
                    var relatedContent = ((dynamic)ci).MobilePushPart.RelatedContent;

                    // nel caso in cui la MobilePushPart sia contenuta nel content type CommunicationAdvertising, usa il related content di quest'ultimo
                    if (ci.ContentType == "CommunicationAdvertising") {
                        relatedContent = ((dynamic)savedCi).CommunicationAdvertisingPart.ContentLinked;
                    }
                    ContentItem relatedContentItem = null;
                    Int32 idContentRelated = 0;
                    dynamic contentForPush;
                    dynamic ciDynamic = (dynamic)ci;
                    if (relatedContent != null && relatedContent.Ids != null && ((int[])relatedContent.Ids).Count() > 0) {
                        idContentRelated = relatedContent.Ids[0];
                        relatedContentItem = _orchardServices.ContentManager.Get(idContentRelated);
                        if (relatedContentItem == null) {
                            relatedContentItem = _orchardServices.ContentManager.GetLatest(idContentRelated);
                        }
                        if (relatedContentItem == null) {
                            contentForPush = ciDynamic;
                        } else {
                            contentForPush = (dynamic)relatedContentItem;
                            if (!relatedContentItem.IsPublished()) {
                                NotifyNoPush();
                                LogInfo("No push will be sent, related content must be published");
                                stopPush = true;
                            }
                        }
                    } else {
                        contentForPush = ciDynamic;
                    }
                    if (!stopPush) {
                        // determina le query impostate
                        int[] ids = null;
                        var aux = ci.As<QueryPickerPart>();
                        if (aux != null) {
                            ids = aux.Ids;
                        }

                        // determina il language
                        string language = _orchardServices.WorkContext.CurrentSite.SiteCulture;
                        try {
                            if (((dynamic)contentForPush).LocalizationPart != null && ((dynamic)contentForPush).LocalizationPart.Culture != null) {
                                language = ((dynamic)contentForPush).LocalizationPart.Culture.Culture;
                            } else if (((dynamic)ci).LocalizationPart != null && ((dynamic)ci).LocalizationPart.Culture != null) {
                                language = ((dynamic)ci).LocalizationPart.Culture.Culture;
                            }
                        } catch {
                            language = "All";
                        }
                        LogInfo("language: " + language);
                        LogInfo("Send to: " + mpp.DevicePush);

                        // determina se è ambiente di produzione
                        bool produzione = true;
                        if (_orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>().ShowTestOptions) {
                            produzione = !(mpp.TestPush);
                        }

                        // tipo di dispositivo (Android, Apple, Windows)
                        TipoDispositivo? locTipoDispositivo = null;
                        if (mpp.DevicePush != "All") {
                            TipoDispositivo auxTipoDispositivo;
                            if (Enum.TryParse<TipoDispositivo>(mpp.DevicePush, out auxTipoDispositivo)) {
                                locTipoDispositivo = auxTipoDispositivo;
                            }
                        }

                        var Myobject = new Dictionary<string, object> { { "Content", ci } };
                        string queryDevice = GetQueryDevice(Myobject, ci.As<MobilePushPart>());
                        if (!SendPushToSpecificDevices) {
                            if (locTipoDispositivo.HasValue == false) {// tutti
                                SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                                SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                                SendAllWindowsPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                            } else if (locTipoDispositivo.Value == TipoDispositivo.Android) {
                                SendAllAndroidPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                            } else if (locTipoDispositivo.Value == TipoDispositivo.Apple) {
                                SendAllApplePart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                            } else if (locTipoDispositivo.Value == TipoDispositivo.WindowsMobile) {
                                SendAllWindowsPart(mpp, idContent, idContentRelated, language, produzione, queryDevice, ids);
                            }
                        } else {
                            var listDevices = GetListMobileDeviceByUserNames(mpp.RecipientList.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                            var pushMessage = GeneratePushMessage(mpp, idContent, idContentRelated);
                            PushAndroid(listDevices.Where(x => x.Device == TipoDispositivo.Android).ToList(),
                                produzione,
                                pushMessage);
                            PushApple(listDevices.Where(x => x.Device == TipoDispositivo.Apple).ToList(),
                                produzione,
                                pushMessage);
                            PushWindows(listDevices.Where(x => x.Device == TipoDispositivo.WindowsMobile).ToList(),
                                produzione,
                                pushMessage);
                        }
                        // aggiorna la MobilePushPart
                        ContentItem ci2 = _orchardServices.ContentManager.Get(ci.Id);
                        MobilePushPart mpp2 = ci2.As<MobilePushPart>();
                        mpp2.PushSent = true;
                        mpp2.PushSentNumber = CountSentOnDb(ci.Id);
                        int counter = 0;
                        if (ci.ContentType == "CommunicationAdvertising") {
                            IList counterAux;
                            if (!SendPushToSpecificDevices) {
                                counterAux = GetPushQueryResult(ids, locTipoDispositivo, produzione, language, true, ci);
                                counter = Convert.ToInt32(((Hashtable)(counterAux[0]))["Tot"]);
                            } else {
                                counterAux = CountPushQueryResultByUserNames(mpp.RecipientList.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries), null, true, "All");
                                counter = Convert.ToInt32(((Hashtable)(counterAux[0]))["Tot"]);
                            }
                        } else {
                            if (queryDevice.Trim() == "") {
                                counter = _pushNotificationRepository.Fetch(x => (x.Device == locTipoDispositivo || locTipoDispositivo == null) && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All")).Count();
                            } else {
                                var estrazione = _transactionManager.GetSession()
                                    .CreateSQLQuery(string.Format("select count(1) from ( {0} ) x where (x.Device = '{1}' or '{1}' = 'All') and x.Produzione = {2} and x.Validated = 1 and (x.Language = '{3}' or '{3}' = 'All') ", queryDevice, (locTipoDispositivo == null) ? "All" : locTipoDispositivo.ToString(), (produzione) ? 1 : 0, language))
                                    .UniqueResult();
                                counter = Convert.ToInt32(estrazione);
                            }
                        }
                        mpp2.TargetDeviceNumber = counter;
                        NotifySentNumber(_messageSent);
                        LogInfo("Notifications sent: " + _messageSent.ToString());
                    }
                }
                string title = "no title";
                try {
                    title = ci.As<TitlePart>().Title;
                } catch {
                    // ignora volutamente qualsiasi errore
                }
                LogInfo("Terminato invio Push del content " + ci.Id + " " + title);
            } catch (Exception ex) {
                string title = "no title";
                try {
                    title = ci.As<TitlePart>().Title;
                } catch {
                    // ignora volutamente qualsiasi errore
                }
                LogError("Errore invio Push del content " + ci.Id + " \"" + title + "\" - Error: " + ex.Message + "\r\n" + ex.StackTrace);
                //aggiorna il result
                _result.Errors = "Error in PublishedPushEvent: " + ex.Message;
            }
            return _result;
        }

        private string[] getextrainfo(Int32 id_of_content) {
            string[] extrainfo = new string[] { "", "" };
            try {
                var thecontent = ((ContentItem)_orchardServices.ContentManager.Get(id_of_content));
                var theautoroute = thecontent.As<AutoroutePart>();
                extrainfo[0] = thecontent.ContentType;
                extrainfo[1] = theautoroute.DisplayAlias;
            } catch { }

            return extrainfo;
        }

        private PushMessage GeneratePushMessage(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated) {
            PushMessage pushMessage = new PushMessage();
            pushMessage.Text = mpp.TextPush;
            pushMessage.Title = mpp.TitlePush;
            pushMessage.ValidPayload = true;
            bool done = false;
            if (mpp.ContentItem.ContentType == "CommunicationAdvertising") {
                if (!string.IsNullOrEmpty(((dynamic)(mpp.ContentItem.As<CommunicationAdvertisingPart>())).UrlLinked.Value)) {
                    string shortlink = _communicationService.GetCampaignLink("Push", mpp);
                    pushMessage.Eu = shortlink;
                    done = true;
                }
            }
            if (mpp.ContentItem.ContentType == "BackgroundPush") {
                var link = ((dynamic)(mpp.ContentItem)).BackgroundPush.ExternalUrl.Value;
                if (!string.IsNullOrEmpty(link)) {
                    var shortlink = _shortLinksService.GetShortLink(link);
                    if (string.IsNullOrEmpty(shortlink)) {
                        throw new Exception("Url Creation Failed");
                    }
                    pushMessage.Eu = shortlink;
                    done = true;
                }
            }
            if (done == false) {
                string ctype = "";
                string displayalias = "";
                var extra = getextrainfo(idContentRelated > 0 ? idContentRelated : idcontent);
                ctype = extra[0];
                displayalias = extra[1];
                pushMessage.idContent = idcontent;
                pushMessage.idRelated = idContentRelated;
                pushMessage.Ct = ctype;
                pushMessage.Al = displayalias;
                var partSettings = mpp.Settings.GetModel<PushMobilePartSettingVM>();
                if (!(partSettings.AcceptZeroRelated) && pushMessage.idRelated == 0) {
                    pushMessage.idRelated = pushMessage.idContent;
                }
            }
            return pushMessage;
        }

        private void SendAllAndroidPart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds, bool repeatable = false) {
            PushMessage pushMessage = GeneratePushMessage(mpp, idcontent, idContentRelated);
            SendAllAndroid(mpp.ContentItem.ContentType, pushMessage, produzione, language, queryDevice, queryIds, repeatable);
        }

        private string GetQueryDevice(Dictionary<string, object> contesto, MobilePushPart mpp) {
            if (mpp.ContentItem.ContentType == "BackgroundPush") {
                var queryField = mpp.ContentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == "BackgroundPush").Fields.FirstOrDefault(x => x.Name == "QueryDevice");
                var query = (queryField as TextField).Value;
                return query ?? "";
            } else {
                string withtoken = mpp.Settings.GetModel<PushMobilePartSettingVM>().QueryDevice;
                if (string.IsNullOrEmpty(withtoken))
                    return "";
                else
                    return _tokenizer.Replace(withtoken.Replace("\r\n", " ").Replace("\t", " "), contesto);
            }
        }

        private List<PushNotificationRecord> GetListMobileDevice(string contenttype, string queryDevice, TipoDispositivo tipodisp, bool produzione, string language, int[] queryIds) {
            var lista = new List<PushNotificationRecord>();
            try {
                if (contenttype == "CommunicationAdvertising") {
                    var elenco = GetPushQueryResult(queryIds, tipodisp, produzione, language);
                    foreach (Hashtable ht in elenco) {
                        lista.Add(new PushNotificationRecord {
                            Id = Convert.ToInt32(ht["Id"]),
                            Device = (TipoDispositivo)(Enum.Parse(typeof(TipoDispositivo), Convert.ToString(ht["Device"]))),
                            Produzione = Convert.ToBoolean(ht["Produzione"], CultureInfo.InvariantCulture),
                            Validated = Convert.ToBoolean(ht["Validated"], CultureInfo.InvariantCulture),
                            Language = Convert.ToString(ht["Language"]),
                            UUIdentifier = Convert.ToString(ht["UUIdentifier"]),
                            Token = Convert.ToString(ht["Token"]),
                            RegistrationUrlHost = Convert.ToString(ht["RegistrationUrlHost"]),
                            RegistrationUrlPrefix = Convert.ToString(ht["RegistrationUrlPrefix"]),
                            RegistrationMachineName = Convert.ToString(ht["RegistrationMachineName"])
                        });
                    }
                } else {
                    if (queryDevice.Trim() == "") {
                        var elenco = _pushNotificationRepository.Fetch(x => x.Device == tipodisp && x.Produzione == produzione && x.Validated == true && (x.Language == language || language == "All"));
                        lista = elenco.ToList();
                    } else {
                        var estrazione = _transactionManager.GetSession()
                            .CreateSQLQuery(string.Format("select Id, Device, Produzione, Validated, Language, UUIdentifier, Token, RegistrationUrlHost, RegistrationUrlPrefix, RegistrationMachineName from ( {0} ) x where x.Device = '{1}' and x.Produzione = {2} and x.Validated = 1 and (x.Language = '{3}' or '{3}' = 'All') ", queryDevice, tipodisp, (produzione) ? 1 : 0, language))
                            .List();
                        object[] ht = null;
                        foreach (var arr in estrazione) {
                            ht = (object[])arr;
                            lista.Add(new PushNotificationRecord {
                                Id = Convert.ToInt32(ht[0]),
                                Device = (TipoDispositivo)(Enum.Parse(typeof(TipoDispositivo), Convert.ToString(ht[1]))),
                                Produzione = Convert.ToBoolean(ht[2], CultureInfo.InvariantCulture),
                                Validated = Convert.ToBoolean(ht[3], CultureInfo.InvariantCulture),
                                Language = Convert.ToString(ht[4]),
                                UUIdentifier = Convert.ToString(ht[5]),
                                Token = Convert.ToString(ht[6]),
                                RegistrationUrlHost = Convert.ToString(ht[7]),
                                RegistrationUrlPrefix = Convert.ToString(ht[8]),
                                RegistrationMachineName = Convert.ToString(ht[9])
                            });
                        }
                    }
                }
            } catch (Exception ex) {
                LogError(string.Format("Error in PushGatewayService.GetListMobileDevice(): {0} - {1}", ex.Message, ex.StackTrace));
                //aggiorna il result
                _result.Errors = "Error in GetListMobileDevice: " + ex.Message;
            }
            return lista;
        }

        private void SendAllAndroid(string contenttype, PushMessage pushMessage, bool produzione, string language, string queryDevice = "", int[] queryIds = null, bool repeatable = false) {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.Android, produzione, language, queryIds);
            PushAndroid(allDevice, produzione, pushMessage, repeatable);
        }

        private void SendAllApplePart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds, bool repeatable = false) {
            PushMessage newpush = GeneratePushMessage(mpp, idcontent, idContentRelated);
            if (newpush.Text.Length > maxPushtextLength) {
                NotifyApplePayloadExceed();
                LogInfo("Apple message payload exceeds the limit");
                newpush.ValidPayload = false;
            }
            SendAllApple(mpp.ContentItem.ContentType, newpush, produzione, language, queryDevice, queryIds, repeatable);
        }

        private void SendAllApple(string contenttype, PushMessage newpush, bool produzione, string language, string queryDevice = "", int[] queryIds = null, bool repeatable = false) {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.Apple, produzione, language, queryIds);
            if (newpush.ValidPayload) {
                PushApple(allDevice, produzione, newpush, repeatable);
            }
        }

        private void SendAllWindowsPart(MobilePushPart mpp, Int32 idcontent, Int32 idContentRelated, string language, bool produzione, string queryDevice, int[] queryIds, bool repeatable = false) {
            PushMessage pushMessage = GeneratePushMessage(mpp, idcontent, idContentRelated);
            SendAllWindows(mpp.ContentItem.ContentType, pushMessage, produzione, language, queryDevice, queryIds, repeatable);
        }

        private void SendAllWindows(string contenttype, PushMessage pushMessage, bool produzione, string language, string queryDevice = "", int[] queryIds = null, bool repeatable = false) {
            var allDevice = GetListMobileDevice(contenttype, queryDevice, TipoDispositivo.WindowsMobile, produzione, language, queryIds);
            PushWindows(allDevice, produzione, pushMessage, repeatable);
        }

        private List<PushNotificationRecord> RemoveSent(List<PushNotificationRecord> listdispositivo, Int32 IdContent) {
            if (IdContent > 0) {
                List<Int32> listainvii = _sentRepository.Fetch(x => x.PushedItem == IdContent && x.Repeatable == false).Select(y => y.PushNotificationRecord_Id).ToList();
                return listdispositivo.Where(x => !listainvii.Contains(x.Id)).ToList();
            } else {
                // se il contenuto è estemporaneo (non salvato su db) non fa nessun filtro sulle push già inviate
                return listdispositivo;
            }
        }

        private List<PushNotificationRecord> CleanRecipients(List<PushNotificationRecord> listdispositivo, int idContent, bool repeatable = false) {
            if (repeatable == false) {
                listdispositivo = RemoveSent(listdispositivo, idContent);
            }
            // elimina i dispositivi non registrati sulla macchina corrente
            string hostCheck = _shellSetting.RequestUrlHost ?? "";
            string prefixCheck = _shellSetting.RequestUrlPrefix ?? "";
            string[] machineNameCheck = GetMachineNames();
            return listdispositivo.Where(x => x.RegistrationUrlHost == hostCheck
                && x.RegistrationUrlPrefix == prefixCheck && machineNameCheck.Contains(x.RegistrationMachineName)).ToList();
        }

        private void InitializeRecipients(List<PushNotificationRecord> listdispositivo, int offset, int size, int idContent, bool repeatable, PushMobileSettingsPart pushSettings) {
            _sentRecords = new ConcurrentDictionary<string, SentRecord>();
            _deviceChanges = new ConcurrentBag<DeviceChange>();
            _deviceExpired = new ConcurrentBag<DeviceChange>();
            int maxPushPerIteration = pushSettings.MaxPushPerIteration == 0 ? 1000 : pushSettings.MaxPushPerIteration;
            // salva l'elenco delle push da inviare
            for (int idx = offset; idx < Math.Min(offset + size, listdispositivo.Count); idx++) {
                try {
                    if (_result != null && _pushNumber >= maxPushPerIteration) {
                        _result.CompletedIteration = false;
                        break;
                    }
                    var pnr = listdispositivo[idx];
                    SentRecord sr = new SentRecord {
                        DeviceType = pnr.Device.ToString(),
                        PushNotificationRecord_Id = pnr.Id,
                        PushedItem = idContent,
                        SentDate = DateTime.UtcNow,
                        Outcome = "",
                        Repeatable = repeatable
                    };
                    _sentRecords.AddOrUpdate(pnr.Token, sr, (key, record) => {
                        return sr;
                    });
                    _pushNumber++;
                } catch (Exception ex) {
                    LogError("PushGateway.InitializeRecipients error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
                    //aggiorna il result
                    _result.Errors = "Error in InitializeRecipients: " + ex.Message;
                }
            }
        }

        private void InitializeRecipientsOnDb() {
            foreach (var sr in _sentRecords.Values) {
                _sentRepository.Create(sr);
            }
            _transactionManager.RequireNew();
        }
        private void UpdateDevicesAndOutcomesOnDb(bool produzione) {
            // aggiorna la validità degli expired
            foreach (var device in _sentRecords.Where(x => x.Value.Outcome == "ex")) {
                try {
                    var myrepo = _pushNotificationRepository.Fetch(x => x.Token == device.Key && x.Produzione == produzione && x.Device.ToString() == device.Value.DeviceType).ToList();
                    if (myrepo.Count() == 1) {
                        PushNotificationRecord pnr = myrepo.FirstOrDefault();
                        pnr.Validated = false;
                        _pushNotificationRepository.Update(pnr);
                        LogInfo(string.Format("Device Subscription Expired Action: {0} not validated -> {1}", device.Value.DeviceType, device.Key));
                    } else {
                        LogError(string.Format("UpdateDevicesAndOutcomesOnDb - Device Subscription Expired Error: {0} -> token not found or token not unique: {1}", device.Value.DeviceType, device.Key));
                    }
                } catch (Exception ex) {
                    LogError("UpdateDevicesAndOutcomesOnDb - Update expired error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
                }
            }
            _transactionManager.RequireNew();

            // salva tutti i device per i quali è cambiato il token
            foreach (var change in _deviceChanges) {
                try {
                    List<PushNotificationRecord> pnrList = _pushNotificationRepository.Fetch(x => x.Token == change.OldToken && x.Device.ToString() == change.TipoDispositivo && x.Produzione == change.Produzione).ToList();
                    foreach (var pnr in pnrList) {
                        IEnumerable<PushNotificationRecord> nuovo = _pushNotificationRepository.Fetch(x => x.Token == change.NewToken && x.Device.ToString() == change.TipoDispositivo && x.Produzione == change.Produzione);
                        if (nuovo == null || nuovo.FirstOrDefault() == null) {
                            // aggiorna il vecchio token
                            pnr.Token = change.NewToken;
                            pnr.Validated = true;
                        }
                        // non serve settare validated a false per il vecchio token perché è già stato fatto in quanto expired
                        _pushNotificationRepository.Update(pnr);
                    }
                } catch (Exception ex) {
                    LogError("UpdateDevicesAndOutcomesOnDb - Device changed error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
                }
                _pushNotificationRepository.Flush();
            }
            _transactionManager.RequireNew();

            // aggiorna i device expired segnalati in ritardo dal feedback service
            foreach (var change in _deviceExpired) {
                try {
                    List<PushNotificationRecord> pnrList = _pushNotificationRepository.Fetch(x => x.Token == change.OldToken && x.Device.ToString() == change.TipoDispositivo).ToList();
                    foreach (var pnr in pnrList) {
                        change.OldDeviceId = pnr.Id;
                        pnr.Validated = false;
                        _pushNotificationRepository.Update(pnr);
                    }
                } catch (Exception ex) {
                    LogError("UpdateDevicesAndOutcomesOnDb - Device expired feedback error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
                }
                _pushNotificationRepository.Flush();
            }
            _transactionManager.RequireNew();

            // aggiorna gli esiti su db
            foreach (var device in _sentRecords) {
                try {
                    if (device.Value.Id == 0) {
                        _sentRepository.Create(device.Value);
                    } else {
                        _sentRepository.Update(device.Value);
                    }
                } catch (Exception ex) {
                    LogError("UpdateDevicesAndOutcomesOnDb - Outcome error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
                }
            }
            _transactionManager.RequireNew();

            // aggiorna l'ultimo esito per i device expired segnalati in ritardo dal feedback service
            foreach (var change in _deviceExpired) {
                try {
                    var record = _sentRepository.Fetch(x => x.PushNotificationRecord_Id == change.OldDeviceId).OrderByDescending(x => x.SentDate).FirstOrDefault();
                    if (record != null) {
                        record.Outcome = "ex";
                        _sentRepository.Update(record);
                        LogInfo(string.Format("UpdateDevicesAndOutcomesOnDb - Updated late feedback outcome for token {0} (Id: {1}).", change.OldToken, change.OldDeviceId));
                    } else {
                        LogInfo(string.Format("UpdateDevicesAndOutcomesOnDb - SentRecord not found for expired token {0} reported by late feedback.", change.OldToken));
                    }
                } catch (Exception ex) {
                    LogError("UpdateDevicesAndOutcomesOnDb - Late feedback outcome error: " + ex.Message + " StackTrace: " + ex.StackTrace);
                }
                _sentRepository.Flush();
            }
            _transactionManager.RequireNew();
        }
        private int CountSentOnDb(int contentId) {
            int result = 0;
            try {
                if (contentId != 0) {
                    result = _sentRepository.Count(x => x.PushedItem == contentId && x.Outcome == "ok");
                }
            } catch (Exception ex) {
                LogError("PushGateway CountSentOnDb error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
            }
            return result;
        }
        private void PushAndroid(List<PushNotificationRecord> listdispositivo, bool produzione, PushMessage pushMessage, bool repeatable = false) {
            listdispositivo = CleanRecipients(listdispositivo, pushMessage.idContent, repeatable);

            // calcola la configurazione per Android
            var pushSettings = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>();
            string setting = "";
            if (produzione)
                setting = pushSettings.AndroidApiKey;
            else
                setting = pushSettings.AndroidApiKeyDevelopment;
            if (listdispositivo.Count > 0) {
                if (string.IsNullOrWhiteSpace(setting)) {
                    LogInfo("Error PushAndroid: missing Android API Key.");
                    _result.Errors = "Error PushAndroid: missing Android API Key.";
                    return;
                }
            } else {
                // nessuna push da inviare
                return;
            }
            var config = new GcmConfiguration(setting);
            var serviceUrl = pushSettings.AndroidPushServiceUrl;
            var notificationIcon = pushSettings.AndroidPushNotificationIcon;
            if (string.IsNullOrWhiteSpace(serviceUrl)) {
                // default: FCM
                config.OverrideUrl("https://fcm.googleapis.com/fcm/send");
            } else {
                config.OverrideUrl(serviceUrl);
            }

            // compone il payload
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendFormat("{{ \"Text\": \"{0}\"", FormatJsonValue(pushMessage.Text));
            if (!string.IsNullOrEmpty(pushMessage.Eu)) {
                sb.AppendFormat(",\"Eu\":\"{0}\"", FormatJsonValue(pushMessage.Eu));
            } else {
                sb.AppendFormat(",\"Id\":{0}", pushMessage.idContent);
                sb.AppendFormat(",\"Rid\":{0}", pushMessage.idRelated);
                sb.AppendFormat(",\"Ct\":\"{0}\"", FormatJsonValue(pushMessage.Ct));
                sb.AppendFormat(",\"Al\":\"{0}\"", FormatJsonValue(pushMessage.Al));
            }
            sb.Append("}");
            var sbParsed = JObject.Parse(sb.ToString());
            // sezione notification
            StringBuilder sbNotification = new StringBuilder();
            sbNotification.Clear();
            JObject sbNotificationParsed = null;
            if (string.IsNullOrWhiteSpace(notificationIcon) == false) {
                sbNotification.AppendFormat("{{ \"body\": \"{0}\"", FormatJsonValue(pushMessage.Text));
                //sbNotification.AppendFormat(",\"title\":\"{0}\"", FormatJsonValue(pushMessage.Text));
                sbNotification.AppendFormat(",\"icon\":\"{0}\"", notificationIcon);
                sbNotification.Append("}");
                sbNotificationParsed = JObject.Parse(sbNotification.ToString());
            }

            int offset = 0;
            int size = pushSettings.PushSendBufferSize == 0 ? 50 : pushSettings.PushSendBufferSize;
            while (offset < listdispositivo.Count) {
                InitializeRecipients(listdispositivo, offset, size, pushMessage.idContent, repeatable, pushSettings);
                if (pushSettings.CommitSentOnly == false) {
                    InitializeRecipientsOnDb();
                }
                // ciclo con retry sui nuovi
                GcmNotification objNotification = null;
                for (int i = 0; i < 2; i++) { // cicla 2 volte: la prima per i device in input, la seconda per quelli changed
                    var push = new GcmServiceBroker(config);
                    push.OnNotificationSucceeded += (notification) => {
                        NotificationSent(notification);
                    };
                    push.OnNotificationFailed += (notification, aggregateEx) => {
                        aggregateEx.Handle(ex => {
                            if (ex is DeviceSubscriptionExpiredException) {
                                var expiredException = (DeviceSubscriptionExpiredException)ex;
                                var oldId = expiredException.OldSubscriptionId;
                                var newId = expiredException.NewSubscriptionId;
                                if (!string.IsNullOrWhiteSpace(newId)) {
                                    DeviceSubscriptionChanged(notification.GetType().Name, oldId, newId, expiredException.Notification, produzione, TipoDispositivo.Android, repeatable);
                                } else
                                    DeviceSubscriptionExpired(notification.GetType().Name, oldId, expiredException.ExpiredAt, produzione, TipoDispositivo.Android);
                            } else {
                                NotificationFailed(notification, aggregateEx);
                            }
                            // Mark it as handled
                            return true;
                        });
                    };
                    push.Start();
                    foreach (var device in _sentRecords.Where(x => x.Value.Outcome == "")) {
                        try {
                            objNotification = new GcmNotification {
                                RegistrationIds = new List<string> { device.Key },
                                Data = sbParsed,
                                Priority = GcmNotificationPriority.High
                                // necessario per bypassare il fatto che l'app non sia in whitelist
                                //TimeToLive = 172800 //2 giorni espressi in secondi
                            };
                            if (sbNotification.Length > 0) {
                                objNotification.Notification = sbNotificationParsed;
                            }
                            push.QueueNotification(objNotification);
                        } catch (Exception ex) {
                            LogError("PushAndroid retry error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
                        }
                    }
                    push.Stop();
                    push = null;
                } // end retry cicle

                UpdateDevicesAndOutcomesOnDb(produzione);
                offset += size;
            }
            // check se ha tentato di inviare tutto o se è uscito per limite di push per ogni run
        }
        private void PushApple(List<PushNotificationRecord> listdispositivo, bool produzione, PushMessage pushMessage, bool repeatable = false) {
            listdispositivo = CleanRecipients(listdispositivo, pushMessage.idContent, repeatable);

            // calcola la configurazione per Apple
            var pushSettings = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>();
            if (string.IsNullOrWhiteSpace(pushMessage.Sound))
                pushMessage.Sound = pushSettings.ApplePushSound;
            if (string.IsNullOrWhiteSpace(pushMessage.Sound))
                pushMessage.Sound = "sound.caf"; //default
            string setting_password = "";
            string setting_file = "";
            bool certificateexist = true;
            ApnsConfiguration.ApnsServerEnvironment environment = ApnsConfiguration.ApnsServerEnvironment.Sandbox;
            if (produzione) {
                environment = ApnsConfiguration.ApnsServerEnvironment.Production;
                setting_password = pushSettings.AppleCertificatePassword;
                setting_file = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Mobile\" + pushSettings.ApplePathCertificateFile;
                if (string.IsNullOrEmpty(pushSettings.ApplePathCertificateFile))
                    certificateexist = false;
            } else {
                setting_password = pushSettings.AppleCertificatePasswordDevelopment;
                setting_file = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Mobile\" + pushSettings.ApplePathCertificateFileDevelopment;
                if (string.IsNullOrEmpty(pushSettings.ApplePathCertificateFileDevelopment))
                    certificateexist = false;
            }
            if (listdispositivo.Count > 0) {
                if (certificateexist == false) {
                    LogInfo("Error PushApple: missing Apple certificate file.");
                    _result.Errors = "Error PushApple: missing Apple certificate file.";
                    return;
                }
            } else {
                // nessuna push da inviare
                return;
            }
            var config = new ApnsConfiguration(environment, setting_file, setting_password);

            // compone il payload
            JObject sbParsed = null;
            if (pushMessage.Text.Length > maxPushtextLength) {
                NotifyApplePayloadExceed();
                LogInfo("Apple message payload exceeds the limit");
            } else {
                StringBuilder sb = new StringBuilder();
                sb.Clear();
                sb.AppendFormat("{{ \"aps\": {{ \"alert\": \"{0}\", \"sound\":\"{1}\"}}", FormatJsonValue(pushMessage.Text), FormatJsonValue(pushMessage.Sound));
                if (!string.IsNullOrEmpty(pushMessage.Eu)) {
                    sb.AppendFormat(",\"Eu\":\"{0}\"", FormatJsonValue(pushMessage.Eu));
                } else {
                    sb.AppendFormat(",\"Id\":{0}", pushMessage.idContent);
                    sb.AppendFormat(",\"Rid\":{0}", pushMessage.idRelated);
                    sb.AppendFormat(",\"Ct\":\"{0}\"", FormatJsonValue(pushMessage.Ct));
                    sb.AppendFormat(",\"Al\":\"{0}\"", FormatJsonValue(pushMessage.Al));
                }
                sb.Append("}");
                sbParsed = JObject.Parse(sb.ToString());
            }

            int offset = 0;
            int size = pushSettings.PushSendBufferSize == 0 ? 50 : pushSettings.PushSendBufferSize;
            while (offset < listdispositivo.Count) {
                InitializeRecipients(listdispositivo, offset, size, pushMessage.idContent, repeatable, pushSettings);
                if (pushSettings.CommitSentOnly == false) {
                    InitializeRecipientsOnDb();
                }
                // ciclo con retry sui nuovi
                for (int i = 0; i < 2; i++) { // cicla 2 volte: la prima per i device in input, la seconda per quelli changed
                    var push = new ApnsServiceBroker(config);
                    push.OnNotificationSucceeded += (notification) => {
                        NotificationSent(notification);
                    };

                    push.OnNotificationFailed += (notification, aggregateEx) => {
                        aggregateEx.Handle(ex => {
                            if (ex is DeviceSubscriptionExpiredException) {
                                var expiredException = (DeviceSubscriptionExpiredException)ex;
                                var oldId = expiredException.OldSubscriptionId;
                                var newId = expiredException.NewSubscriptionId;
                                if (!string.IsNullOrWhiteSpace(newId)) {
                                    DeviceSubscriptionChanged(notification.GetType().Name, oldId, newId, expiredException.Notification, produzione, TipoDispositivo.Apple, repeatable);
                                } else
                                    DeviceSubscriptionExpired(notification.GetType().Name, oldId, expiredException.ExpiredAt, produzione, TipoDispositivo.Apple);
                            } else {
                                NotificationFailed(notification, aggregateEx);
                            }
                            // Mark it as handled
                            return true;
                        });
                    };
                    push.Start();
                    foreach (var device in _sentRecords.Where(x => x.Value.Outcome == "")) {
                        try {
                            var notification = new ApnsNotification {
                                DeviceToken = device.Key,
                                Payload = sbParsed,
                                LowPriority = false
                            };
                            if(notification.IsDeviceRegistrationIdValid() && notification.DeviceToken.Length >= ApnsNotification.DEVICE_TOKEN_STRING_MIN_SIZE) {
                                push.QueueNotification(notification);
                            }
                            else {
                                _sentRecords.AddOrUpdate(device.Key, new SentRecord(), (key, record) => {
                                    record.Outcome = "wr"; // lexical contraction for 'wrong'
                                    return record;
                                });
                                LogError(string.Format("PushApple error: token '{0}' is invalid and was discarded.", device.Key));
                            }
                        } catch (Exception ex) {
                            LogError("PushApple error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
                        }
                    }
                    push.Stop();
                    push = null;

                    // check sui device expired
                    try {
                        var feedback = new FeedbackService(config);
                        feedback.FeedbackReceived += (token, expiredTime) => {
                            DeviceSubscriptionExpiredLateFeedback(token, expiredTime, TipoDispositivo.Apple);
                        };
                        feedback.Check();
                    } catch (Exception ex) {
                        LogError("PushApple-FeedbackService error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
                    }
                } // end retry cicle

                UpdateDevicesAndOutcomesOnDb(produzione);
                offset += size;
            }
        }

        private void PushWindows(List<PushNotificationRecord> listdispositivo, bool produzione, PushMessage pushMessage, bool repeatable = false) {
            listdispositivo = CleanRecipients(listdispositivo, pushMessage.idContent, repeatable);

            // calcola la configurazione per WindowsPhone
            // TODO: gestire settaggi produzione/sviluppo?
            var pushSettings = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>();
            var setting_WindowsAppPackageName = pushSettings.WindowsAppPackageName;
            var setting_WindowsAppSecurityIdentifier = pushSettings.WindowsAppSecurityIdentifier;
            var setting_WindowsEndPoint = pushSettings.WindowsEndPoint;
            if (listdispositivo.Count > 0) {
                if (string.IsNullOrWhiteSpace(setting_WindowsAppPackageName) || string.IsNullOrWhiteSpace(setting_WindowsAppSecurityIdentifier) || string.IsNullOrWhiteSpace(setting_WindowsEndPoint)) {
                    LogInfo("Error PushWindows: missing Windows Mobile settings.");
                    _result.Errors = "Error PushWindows: missing Windows Mobile settings.";
                    return;
                }
            } else {
                // nessuna push da inviare
                return;
            }
            var config = new WnsConfiguration(setting_WindowsAppPackageName, setting_WindowsAppSecurityIdentifier, setting_WindowsEndPoint);

            // genera il payload
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendFormat("<toast><visual><binding template=\"ToastGeneric\"><text>{0}</text>", FormatJsonValue(pushMessage.Text));
            if (!string.IsNullOrEmpty(pushMessage.Eu)) {
                sb.AppendFormat("<Eu>{0}</Eu>", FormatJsonValue(pushMessage.Eu));
            } else {
                sb.AppendFormat("<Id>{0}</Id>", pushMessage.idContent);
                sb.AppendFormat("<Rid>{0}</Rid>", pushMessage.idRelated);
                sb.AppendFormat("<Ct>{0}</Ct>", FormatJsonValue(pushMessage.Ct));
                sb.AppendFormat("<Al>{0}</Al>", FormatJsonValue(pushMessage.Al));
            }
            sb.Append("</binding></visual></toast>");
            var sbXElement = XElement.Parse(sb.ToString());

            int offset = 0;
            int size = pushSettings.PushSendBufferSize == 0 ? 50 : pushSettings.PushSendBufferSize;
            while (offset < listdispositivo.Count) {
                InitializeRecipients(listdispositivo, offset, size, pushMessage.idContent, repeatable, pushSettings);
                if (pushSettings.CommitSentOnly == false) {
                    InitializeRecipientsOnDb();
                }
                // ciclo con retry sui nuovi
                for (int i = 0; i < 2; i++) { // cicla 2 volte: la prima per i device in input, la seconda per quelli changed
                    var push = new WnsServiceBroker(config);
                    push.OnNotificationSucceeded += (notification) => {
                        NotificationSent(notification);
                    };
                    push.OnNotificationFailed += (notification, aggregateEx) => {
                        aggregateEx.Handle(ex => {
                            if (ex is DeviceSubscriptionExpiredException) {
                                var expiredException = (DeviceSubscriptionExpiredException)ex;
                                var oldId = expiredException.OldSubscriptionId;
                                var newId = expiredException.NewSubscriptionId;
                                if (!string.IsNullOrWhiteSpace(newId)) {
                                    DeviceSubscriptionChanged(notification.GetType().Name, oldId, newId, expiredException.Notification, produzione, TipoDispositivo.WindowsMobile, repeatable);
                                } else
                                    DeviceSubscriptionExpired(notification.GetType().Name, oldId, expiredException.ExpiredAt, produzione, TipoDispositivo.WindowsMobile);
                            } else {
                                NotificationFailed(notification, aggregateEx);
                            }
                            // Mark it as handled
                            return true;
                        });
                    };
                    push.Start();
                    foreach (var device in _sentRecords.Where(x => x.Value.Outcome == "")) {
                        try {
                            push.QueueNotification(new WnsToastNotification {
                                ChannelUri = device.Key,
                                Payload = sbXElement
                            });
                        } catch (Exception ex) {
                            LogError("PushWindows retry error:  " + ex.Message + " StackTrace: " + ex.StackTrace);
                        }
                    }
                    push.Stop();
                    push = null;
                } // end retry cicle

                UpdateDevicesAndOutcomesOnDb(produzione);
                offset += size;
            }
        }

        private string FormatJsonValue(string text) {
            return (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private void DeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification, bool produzione, TipoDispositivo tipoDispositivo, bool repeatable) {
            try {
                SentRecord srOld = null;
                SentRecord srNew = null;
                if (string.IsNullOrWhiteSpace(oldSubscriptionId) == false) {
                    srOld = _sentRecords.AddOrUpdate(oldSubscriptionId, new SentRecord(), (key, record) => {
                        record.Outcome = "ex";
                        return record;
                    });
                }
                if (srOld != null) {
                    srNew = new SentRecord {
                        DeviceType = srOld.DeviceType,
                        Id = 0,
                        PushedItem = srOld.PushedItem,
                        PushNotificationRecord_Id = srOld.PushNotificationRecord_Id,
                        SentDate = srOld.SentDate,
                        Outcome = "",
                        Repeatable = repeatable
                    };
                    _sentRecords.AddOrUpdate(newSubscriptionId, srNew, (key, record) => {
                        return srNew;
                    });
                } else {
                    LogError("Error DeviceSubscriptionChanged: tipoDispositivo: " + tipoDispositivo + " -> oldSubscriptionId" + oldSubscriptionId + " -> newSubscriptionId" + newSubscriptionId + "Error: old token not in _sentRecords.");
                }
                _deviceChanges.Add(new DeviceChange {
                    OldToken = oldSubscriptionId,
                    NewToken = newSubscriptionId,
                    Produzione = produzione,
                    TipoDispositivo = tipoDispositivo.ToString()
                });
                LogInfo(string.Format("Device Registration Changed:  Old-> {0}  New-> {1} -> {2}", oldSubscriptionId, newSubscriptionId, notification));
            } catch (Exception ex) {
                LogError("Error DeviceSubscriptionChanged: tipoDispositivo: " + tipoDispositivo + " -> oldSubscriptionId" + oldSubscriptionId + " -> newSubscriptionId" + newSubscriptionId + "Error :" + ex.Message + " StackTRace:" + ex.StackTrace);
            }
        }

        private void NotificationSent(INotification notification) {
            try {
                string token = "";
                if (notification is ApnsNotification) {
                    token = (notification as ApnsNotification).DeviceToken;
                } else if (notification is WnsNotification) {
                    token = (notification as WnsNotification).ChannelUri;
                } else if (notification is GcmNotification) {
                    token = (notification as GcmNotification).RegistrationIds[0];
                }
                LogInfo(string.Format("Sent: {0} -> {1} -> {2}", notification.GetType().Name, token, notification));
                _sentRecords.AddOrUpdate(token, new SentRecord(), (key, record) => {
                    record.Outcome = "ok";
                    return record;
                });
                _messageSent++;
            } catch (Exception ex) {
                LogError("Error NotificationSent: notification: " + notification + " -> Error: " + ex.Message + " StackTRace: " + ex.StackTrace);
            }
        }

        private void NotificationFailed(INotification notification, AggregateException notificationFailureException) {
            string token = "";
            try {
                if (notification is ApnsNotification) {
                    token = (notification as ApnsNotification).DeviceToken;
                } else if (notification is WnsNotification) {
                    token = (notification as WnsNotification).ChannelUri;
                } else if (notification is GcmNotification) {
                    token = (notification as GcmNotification).RegistrationIds[0];
                }
                var innerEx = new StringBuilder();
                Exception inner = null;
                foreach (var ie in notificationFailureException.InnerExceptions) {
                    innerEx.AppendFormat("\r\n\t{0}", ie.Message);
                    innerEx.AppendFormat("\r\n\t\t{0}", (ie.StackTrace ?? "").Replace("\r\n", "\r\n\t\t"));
                    inner = ie.InnerException;
                    while (inner != null) {
                        innerEx.AppendFormat("\r\n\t{0}", inner.Message);
                        innerEx.AppendFormat("\r\n\t\t{0}", (inner.StackTrace ?? "").Replace("\r\n", "\r\n\t\t"));
                        inner = inner.InnerException;
                    }
                }
                LogInfo(string.Format("Failure: {0} token: {1} -> {2} - InnerExceptions: {3}\r\n\t-> {4}", notification.GetType().Name, token, notificationFailureException.Message, innerEx.ToString(), notification.ToString()));
                _sentRecords.AddOrUpdate(token, new SentRecord(), (key, record) => {
                    record.Outcome = "ko";
                    return record;
                });
            } catch (Exception ex) {
                LogError(string.Format("Error NotificationFailed: {0} token: {1} -> Error: {2} StackTrace: {3}", notification.GetType().Name, token, ex.Message, ex.StackTrace));
            }
        }

        private void DeviceSubscriptionExpired(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, bool produzione, TipoDispositivo dispositivo) {
            try {
                _sentRecords.AddOrUpdate(expiredDeviceSubscriptionId, new SentRecord(), (key, record) => {
                    record.Outcome = "ex";
                    return record;
                });
                LogInfo(string.Format("Device Subscription Expired: {0} -> {1}", sender, expiredDeviceSubscriptionId));
            } catch (Exception ex) {
                LogError(string.Format("Error DeviceSubscriptionExpired: tipoDispositivo: {0} -> expiredDeviceSubscriptionId: {1} - Error: {2} StackTrace: {3}", dispositivo, expiredDeviceSubscriptionId, ex.Message, ex.StackTrace));
            }
        }
        private void DeviceSubscriptionExpiredLateFeedback(string token, DateTime timestamp, TipoDispositivo dispositivo) {
            try {
                if (_sentRecords.ContainsKey(token)) {
                    _sentRecords.AddOrUpdate(token, new SentRecord(), (key, record) => {
                        record.Outcome = "ex";
                        return record;
                    });
                    LogInfo(string.Format("Device Subscription Expired (feedback service): {0} -> {1}", dispositivo, token));
                } else {
                    // fallback nel caso in cui il device non faccia parte dell'attuale gruppo di device appena pushati ma di un gruppo precedente
                    _deviceExpired.Add(new DeviceChange {
                        OldToken = token,
                        NewToken = null,
                        TipoDispositivo = dispositivo.ToString()
                    });
                    LogInfo(string.Format("Device Subscription Expired (late feedback service): {0} -> {1}", dispositivo, token));
                }
            } catch (Exception ex) {
                LogError(string.Format("Error DeviceSubscriptionExpiredLateFeedback: tipoDispositivo: {0} -> expiredDeviceSubscriptionId: {1} - Error: {2} StackTrace: {3}", dispositivo, token, ex.Message, ex.StackTrace));
            }
        }

        private void LogError(string message) {
            var wrapper = string.Format("PushGatewayService error on {0}: {1}",
                string.IsNullOrWhiteSpace(_shellSetting?.Name) ? "[could not identify tenant name]" : _shellSetting.Name,
                "{0}");
            Logger.Log(OrchardLogging.LogLevel.Error, null, string.Format(wrapper, message), null);
        }

        private void LogInfo(string message) {
            Logger.Log(OrchardLogging.LogLevel.Information, null, message, null);
        }

        private string[] GetMachineNames() {
            var list = _shellSetting["PushRegistrationAllowedMachineNames"] ?? System.Environment.MachineName;
            var result = list.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if(result.Contains(System.Environment.MachineName)) {
                return result;
            }
            else {
                LogError(string.Format("Current machine name \"{0}\" not correctly set in Settings.txt", System.Environment.MachineName));
                return new string[0];
            }
        }

        private string GetMachineNamesForSql() {
            var list = GetMachineNames();
            var result = new List<string>();
            foreach(var name in list) {
                result.Add(name.Replace("'", "''"));
            }
            return "'" + string.Join("','", result) + "'";
        }

        public void ResetNotificationFailures(ContentItem ci) {
            // update failed SentRecords according to PushedItem, Outcome and Repeatable
            var list = _sentRepository.Fetch(x => x.PushedItem == ci.Id && x.Outcome == outcomeToTryAgain && x.Repeatable == false);
            foreach (var item in list) {
                item.Repeatable = true;
                item.Outcome = "rp";
            }
        }

        /// <summary>
        /// Classe di utility per la gestione dei token modificati
        /// </summary>
        private class DeviceChange {
            public string OldToken { get; set; }
            public string NewToken { get; set; }
            public bool Produzione { get; set; }
            public string TipoDispositivo { get; set; }
            public int OldDeviceId { get; set; }

            public DeviceChange() {
                OldToken = "";
                NewToken = "";
                Produzione = false;
                TipoDispositivo = "";
            }
        }

    }
}