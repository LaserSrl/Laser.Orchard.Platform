using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.StartupConfig.Models;
using NHibernate.Transform;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Laser.Orchard.Mobile.Services {

    public interface ISmsCommunicationService : IDependency {
        IList GetSmsQueryResult(Int32[] ids, Int32? idlingua, bool countOnly = false, int contentId = 0); 
        IList GetSmsQueryResult(Int32[] ids, Int32? idlingua, bool countOnly = false, ContentItem advItem = null);
    }

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsCommunicationService : ISmsCommunicationService {
        private readonly IOrchardServices _orchardServices;
        private readonly ICultureManager _cultureManager;
        private readonly IQueryPickerService _queryPickerServices;
        private readonly ITransactionManager _transactionManager;

        public SmsCommunicationService(IOrchardServices orchardServices, IQueryPickerService queryPickerServices, ICultureManager cultureManager, ITransactionManager transactionManager)
        {
            _orchardServices = orchardServices;
            _cultureManager = cultureManager;
            _queryPickerServices = queryPickerServices;
            _transactionManager = transactionManager;
        }

        public IList GetSmsQueryResult(Int32[] ids, Int32? idlingua, bool countOnly = false, int contentId = 0) {
            ContentItem contentItem = null;
            if (contentId > 0) {
                contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);
            }
            return GetSmsQueryResult(ids, idlingua, countOnly, contentItem);

        }
                
        public IList GetSmsQueryResult(Int32[] ids, Int32? idlingua, bool countOnly = false, ContentItem advItem = null)
        {
            IHqlQuery query;
            if (ids != null && ids.Length > 0)
            {
                Dictionary<string, object> tokens = new Dictionary<string, object>();
                if (advItem != null) {
                    tokens.Add("Content", advItem);
                }
                query = IntegrateAdditionalConditions(_queryPickerServices.GetCombinedContentQuery(ids, tokens, new string[] { "CommunicationContact" }), idlingua);
            }
            else
            {
                query = IntegrateAdditionalConditions(null, idlingua);
            }

            //// prove #GM
            //var ris1 = _orchardServices.ContentManager.HqlQuery()
            //    .ForType(new string[] { "CommunicationContact" })
            //    .Join(alias => alias.ContentPartRecord<SmsContactPartRecord>().Property("SmsRecord", "smsrecord")) 
            //    .Join(alias => alias.ContentPartRecord<TitlePartRecord>())
            //    ;
            
            //var ris2 = ris1.List();
            //return ris2.ToList();

            // Trasformo in stringa HQL
            var stringHQL = ((DefaultHqlQuery)query).ToHql(false);

            // Rimuovo la Order by per poter fare la query annidata
            // TODO: trovare un modo migliore per rimuovere la order by
            stringHQL = stringHQL.ToString().Replace("order by civ.Id", "");

            //var queryForSms = "SELECT distinct cir.Id as Id, TitlePart.Title as Title, SmsRecord.Prefix as SmsPrefix, SmsRecord.Sms as SmsNumber FROM " +
            //    "Orchard.ContentManagement.Records.ContentItemVersionRecord as civr join " +
            //    "civr.ContentItemRecord as cir join " +
            //    "civr.TitlePartRecord as TitlePart join " +
            //    "cir.SmsContactPartRecord as SmsPart join " +
            //        "SmsPart.SmsRecord as SmsRecord " +
            //    "WHERE civr.Published=1 AND civr.Id in (" + stringHQL + ")";
            string queryForSms = "";
            if (countOnly) {
                queryForSms = "SELECT count(SmsRecord) as Tot";
            }
            else {
                queryForSms = "SELECT civr as ContentItemVersionRecord, cir as ContentItemRecord, TitlePart as TitlePartRecord, SmsPart as SmsContactPartRecord, SmsRecord as SmsRecord";
            }
            queryForSms += " FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr join " +
                "civr.ContentItemRecord as cir join " +
                "civr.TitlePartRecord as TitlePart join " +
                "cir.SmsContactPartRecord as SmsPart join " +
                    "SmsPart.SmsRecord as SmsRecord " +
                "WHERE civr.Published=1 AND SmsRecord.Validated AND SmsRecord.AccettatoUsoCommerciale AND civr.Id in (" + stringHQL + ")";

            // Creo query ottimizzata per le performance
            var fullStatement = _transactionManager.GetSession()
                .CreateQuery(queryForSms)
                .SetCacheable(false);

            var lista = fullStatement
                .SetResultTransformer(Transformers.AliasToEntityMap)  //(Transformers.AliasToEntityMap) //(Transformers.AliasToBean<SmsHQL>())
                .List(); // .List<SmsHQL>();
            return lista;
        }

        private IHqlQuery IntegrateAdditionalConditions(IHqlQuery query, Int32? idlocalization)
        {
            if (query == null)
            {
                query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "CommunicationContact" });
            }

            // Query in base alla localizzazione del contenuto
            if (idlocalization != null)
            {
                if (idlocalization == _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id)
                {
                    // la lingua è quella di default del sito, quindi prendo tutti quelli che hanno espresso la preferenza sulla lingua e quelli che non l'hanno espressa
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Disjunction(a => a.Eq("Culture_Id", idlocalization), b => b.Eq("Culture_Id", 0))); // lingua prescelta uguale a lingua contenuto oppure nessuna lingua prescelta e allora
                }
                else
                {
                    // la lingua NON è quella di default del sito, quindi prendo SOLO quelli che hanno espresso la preferenza sulla lingua
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Eq("Culture_Id", idlocalization));
                }
            }

            query = query
                .Where(x => x.ContentPartRecord<SmsContactPartRecord>(), x => x.IsNotEmpty("SmsRecord"));

            return query;
        }

        //public IHqlQuery IntegrateAdditionalConditions(IHqlQuery query = null, IContent content = null) {
        //    if (query == null) {
        //        query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "CommunicationContact" });
        //    }

        //    // Query in base alla localizzazione del contenuto
        //    var localizedPart = content.As<LocalizationPart>();
        //    if (localizedPart != null) {
        //        var langId = _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id;  //default site lang
        //        if (localizedPart.Culture != null) {
        //            langId = localizedPart.Culture.Id;
        //        }
        //        if (langId == _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id) {
        //            // la lingua è quella di default del sito, quindi prendo tutti quelli che hanno espresso la preferenza sulla lingua e quelli che non l'hanno espressa
        //            query = query
        //                .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Disjunction(a => a.Eq("Culture_Id", langId), b => b.Eq("Culture_Id", 0))); // lingua prescelta uguale a lingua contenuto oppure nessuna lingua prescelta e allora
        //        } else {
        //            // la lingua NON è quella di default del sito, quindi prendo SOLO quelli che hanno espresso la preferenza sulla lingua 
        //            query = query
        //                .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Eq("Culture_Id", langId));
        //        }
        //    }

        //    query = query
        //        .Where(x => x.ContentPartRecord<SmsContactPartRecord>(), x => x.IsNotEmpty("SmsRecord"));

        //    return query;
        //}
    }
}