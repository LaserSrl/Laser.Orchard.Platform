using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.StartupConfig.Models;
using NHibernate.Transform;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Users.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.MailCommunication.Services {

    public interface IMailCommunicationService : IDependency {

        IList GetMailQueryResult(Int32[] ids, Int32? idlingua, bool countOnly = false, int contentId = 0);
        IList GetMailQueryResult(Int32[] ids, Int32? idlingua, bool countOnly = false, ContentItem advItem = null);
        IList GetMailQueryResult(string[] userNames, Int32? idlingua, bool countOnly = false, ContentItem advItem = null);
    }

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class DefaultMailCommunicationService : IMailCommunicationService {
        private readonly IOrchardServices _orchardServices;
        private readonly ICultureManager _cultureManager;
        private readonly IQueryPickerService _queryPickerServices;
        private readonly ITransactionManager _transactionManager;

        public DefaultMailCommunicationService(ITransactionManager transactionManager, IQueryPickerService queryPickerServices, IOrchardServices orchardServices, ICultureManager cultureManager) {
            _orchardServices = orchardServices;
            _cultureManager = cultureManager;
            _queryPickerServices = queryPickerServices;
            _transactionManager = transactionManager;
        }
        public IList GetMailQueryResult(Int32[] ids, Int32? idlingua, bool countOnly = false, int contentId = 0) {
            ContentItem contentItem = null;
            if (contentId > 0) {
                contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);
            }
            return GetMailQueryResult(ids, idlingua, countOnly, contentItem);
        }

        public IList GetMailQueryResult(Int32[] ids, Int32? idlingua, bool countOnly = false, ContentItem advItem = null) {// idcontent) {
            // dynamic content = _orchardServices.ContentManager.Get(idcontent);
            //  content = _orchardServices.ContentManager.Get(idcontent, VersionOptions.DraftRequired);
            IHqlQuery query;
            if (ids != null && ids.Count() > 0) {
                //if (content.QueryPickerPart != null && content.QueryPickerPart.Ids.Length > 0) {
                Dictionary<string, object> tokens = new Dictionary<string, object>();
                if (advItem != null) {
                    tokens.Add("Content", advItem);
                }
                query = IntegrateAdditionalConditions(_queryPickerServices.GetCombinedContentQuery(ids, tokens, new string[] { "CommunicationContact" }), idlingua);
            } else {
                query = IntegrateAdditionalConditions(null, idlingua);
            }

            // Trasformo in stringa HQL
            var stringHQL = ((DefaultHqlQuery)query).ToHql(false);

            // Rimuovo la Order by per poter fare la query annidata
            // TODO: trovare un modo migliore per rimuovere la order by
            stringHQL = stringHQL.ToString().Replace("order by civ.Id", "");

            string queryForEmail = "";
            if (countOnly) {
                queryForEmail = "SELECT count(EmailRecord) as Tot";
            } else {
                queryForEmail = "SELECT cir.Id as Id, TitlePart.Title as Title, EmailRecord.Email as EmailAddress";
            }
            queryForEmail += " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr join " +
                "civr.ContentItemRecord as cir join " +
                "civr.TitlePartRecord as TitlePart join " +
                "cir.EmailContactPartRecord as EmailPart join " +
                    "EmailPart.EmailRecord as EmailRecord " +
                "WHERE civr.Published=1 AND EmailRecord.Validated AND EmailRecord.AccettatoUsoCommerciale AND civr.Id in (" + stringHQL + ")";

            var fullStatement = _transactionManager.GetSession()
                .CreateQuery(queryForEmail)
                .SetCacheable(false);
            IList lista = fullStatement
                    .SetResultTransformer(Transformers.AliasToEntityMap)
                    .List();
            return lista;
        }

        public IList GetMailQueryResult(string[] userNames, Int32? idlingua, bool countOnly = false, ContentItem advItem = null) {
            if (userNames.Length <= 0) return null;
            var userNamesCSV = String.Join(",", userNames.Select(x => "'" + x.ToLower().Replace("'", "''") + "'"));
            string queryForEmail = "";
            if (countOnly) {
                queryForEmail = "SELECT count(EmailRecord) as Tot";
            } else {
                queryForEmail = "SELECT cir.Id as Id, TitlePart.Title as Title, EmailRecord.Email as EmailAddress";
            }
            queryForEmail += " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr join " +
                "civr.ContentItemRecord as cir join " +
                "cir.EmailContactPartRecord as EmailPart join " +
                "EmailPart.EmailRecord as EmailRecord join " +
                "civr.TitlePartRecord as TitlePart " + 
                ", Laser.Orchard.CommunicationGateway.Models.CommunicationContactPartRecord contact " + 
                "WHERE civr.Published=1 AND EmailRecord.Validated AND EmailRecord.AccettatoUsoCommerciale " +
                "AND EmailRecord.EmailContactPartRecord_Id=contact.Id " + // join condition per il contact
                "AND (EmailRecord.Email IN (" + userNamesCSV + ") " +
                "    OR exists (select upr.Id from Orchard.Users.Models.UserPartRecord upr " + 
                "        WHERE upr.Id=contact.UserPartRecord_Id AND upr.UserName IN (" + userNamesCSV + ") ))";
            
            var fullStatement = _transactionManager.GetSession()
                .CreateQuery(queryForEmail)
                .SetCacheable(false);
            IList lista = fullStatement
                    .SetResultTransformer(Transformers.AliasToEntityMap)
                    .List();
            return lista;
        }

        private IHqlQuery IntegrateAdditionalConditions(IHqlQuery query, Int32? idlocalization) {
            if (query == null) {
                query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "CommunicationContact" });
            }
            // Query in base alla localizzazione del contenuto
            //  var localizedPart = content.As<LocalizationPart>();
            if (idlocalization != null) {
                //var langId = _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id;  //default site lang
                //if (localizedPart.Culture != null) {
                //    langId = localizedPart.Culture.Id;
                //}
                if (idlocalization == _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id) {
                    // la lingua è quella di default del sito, quindi prendo tutti quelli che hanno espresso la preferenza sulla lingua e quelli che non l'hanno espressa
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Disjunction(a => a.Eq("Culture_Id", idlocalization), b => b.Eq("Culture_Id", 0))); // lingua prescelta uguale a lingua contenuto oppure nessuna lingua prescelta e allora
                } else {
                    // la lingua NON è quella di default del sito, quindi prendo SOLO quelli che hanno espresso la preferenza sulla lingua
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Eq("Culture_Id", idlocalization));
                }
            }

            query = query
                .Where(x => x.ContentPartRecord<EmailContactPartRecord>(), x => x.IsNotEmpty("EmailRecord"));

            return query;
        }
    }
}