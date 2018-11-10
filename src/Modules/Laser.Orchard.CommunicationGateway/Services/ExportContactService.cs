using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using NHibernate;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Fields.Fields;
using Orchard.MediaLibrary.Fields;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Services {

    #region Support Classes

    public class ContactExport {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Hashtable> Fields { get; set; }
        public List<string> Sms { get; set; }
        public List<string> Mail { get; set; }
    }

    #endregion

    public interface IExportContactService : IDependency {

        IEnumerable<ContentItem> GetContactList(SearchVM search);
        ContactExport GetInfoContactExport(ContentItem contenuto);
    }

    public class ExportContactService : IExportContactService {

        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly ITaxonomyService _taxonomyService;

        public ExportContactService(IContentManager contentManager, ITransactionManager transactionManager, ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _taxonomyService = taxonomyService;
        }

        public IEnumerable<ContentItem> GetContactList(SearchVM search) {
            IEnumerable<ContentItem> contentItems = null;
            List<int> arr = null;

            IContentQuery<ContentItem> contentQuery = _contentManager.Query(VersionOptions.Latest).ForType("CommunicationContact");

            if (!(string.IsNullOrEmpty(search.Expression) && !search.CommercialUseAuthorization.HasValue && !search.ThirdPartyAuthorization.HasValue)) {
                switch (search.Field) {
                    case SearchFieldEnum.Name:
                        contentItems = contentQuery.Where<TitlePartRecord>(w => w.Title.Contains(search.Expression)).List();
                        break;

                    case SearchFieldEnum.Mail:
                        string myQueryMail = @"select cir.Id
                                    from Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                                    join civr.ContentItemRecord as cir
                                    join cir.EmailContactPartRecord as EmailPart
                                    join EmailPart.EmailRecord as EmailRecord
                                    where 1 = 1 ";
                        if (!string.IsNullOrEmpty(search.Expression)) myQueryMail += "and EmailRecord.Email like '%' + :mail + '%' ";
                        if (search.CommercialUseAuthorization.HasValue) myQueryMail += "and EmailRecord.AccettatoUsoCommerciale = :commuse ";
                        if (search.ThirdPartyAuthorization.HasValue) myQueryMail += "and EmailRecord.AutorizzatoTerzeParti = :tpuse ";
                        myQueryMail += "order by cir.Id";

                        var mailQueryToExecute = _transactionManager.GetSession().CreateQuery(myQueryMail);
                        if (!string.IsNullOrEmpty(search.Expression)) mailQueryToExecute.SetParameter("mail", search.Expression);
                        if (search.CommercialUseAuthorization.HasValue) mailQueryToExecute.SetParameter("commuse", search.CommercialUseAuthorization.Value, NHibernateUtil.Boolean);
                        if (search.ThirdPartyAuthorization.HasValue) mailQueryToExecute.SetParameter("tpuse", search.ThirdPartyAuthorization.Value, NHibernateUtil.Boolean);

                        var elencoIdMail = mailQueryToExecute.List<int>();

                        arr = new List<int>(elencoIdMail);
                        contentItems = contentQuery.Where<CommunicationContactPartRecord>(x => arr.Contains(x.Id)).List();
                        break;

                    case SearchFieldEnum.Phone:
                        string myQuerySms = @"select cir.Id
                                    from Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                                    join civr.ContentItemRecord as cir
                                    join cir.SmsContactPartRecord as SmsPart
                                    join SmsPart.SmsRecord as SmsRecord
                                    where 1 = 1 ";
                        if (!string.IsNullOrEmpty(search.Expression)) myQuerySms += "and SmsRecord.Sms like '%' + :sms + '%' ";
                        if (search.CommercialUseAuthorization.HasValue) myQuerySms += "and SmsRecord.AccettatoUsoCommerciale = :commuse ";
                        if (search.ThirdPartyAuthorization.HasValue) myQuerySms += "and SmsRecord.AutorizzatoTerzeParti = :tpuse ";
                        myQuerySms += "order by cir.Id";

                        var smsQueryToExecute = _transactionManager.GetSession().CreateQuery(myQuerySms);
                        if (!string.IsNullOrEmpty(search.Expression)) smsQueryToExecute.SetParameter("sms", search.Expression);
                        if (search.CommercialUseAuthorization.HasValue) smsQueryToExecute.SetParameter("commuse", search.CommercialUseAuthorization.Value, NHibernateUtil.Boolean);
                        if (search.ThirdPartyAuthorization.HasValue) smsQueryToExecute.SetParameter("tpuse", search.ThirdPartyAuthorization.Value, NHibernateUtil.Boolean);

                        var elencoIdSms = smsQueryToExecute.List<int>();

                        arr = new List<int>(elencoIdSms);
                        contentItems = contentQuery.Where<CommunicationContactPartRecord>(x => arr.Contains(x.Id)).List();
                        break;
                }
            } else {
                contentItems = contentQuery.List();
            };

            return contentItems;
        }


        private Hashtable ConvertField(ContentField contfield,string partname) {
            dynamic cf = contfield;
            Hashtable hs = new Hashtable();
            if (cf.FieldDefinition.Name != typeof(ContentPickerField).Name && cf.FieldDefinition.Name != typeof(MediaLibraryPickerField).Name) {
                        string keyField = partname+"." + ((object)cf.Name).ToString();
                        string valueField = "";

                        if (cf.FieldDefinition.Name == typeof(DateTimeField).Name) {
                            if (cf.DateTime != null && !cf.DateTime.Equals(DateTime.MinValue))
                                valueField = ((object)cf.DateTime).ToString();
                        }
                        else if (cf.FieldDefinition.Name == typeof(TaxonomyField).Name) {
                            if (((TaxonomyField)cf).Terms != null) {
                                foreach (TermPart term in ((TaxonomyField)cf).Terms) {
                                    // Più termini selezionati
                                    if (valueField != "")
                                        valueField = ";" + valueField;

                                    if (term.Path == "/") {
                                        // Taxonomy ad un livello
                                        valueField = term.Name.Replace('/', '\\').Replace(";", ".,") + valueField;
                                    } 
                                    else {
                                        // Taxonomy su più livelli
                                        GetValueCompletoTerms(term, ref valueField);
                                    }
                                }
                            }
                        } else {
                            if (cf.Value != null)
                                valueField = ((object)cf.Value).ToString();
                        }

                       // Hashtable hs = new Hashtable();
                        hs.Add(keyField, valueField);
                      //  listaField.Add(hs);
                    }
            return hs;
        }


        public ContactExport GetInfoContactExport(ContentItem content) {
            ContactExport contact = new ContactExport();

            // Id
            contact.Id = content.Id;

            // Title
            contact.Title = content.As<TitlePart>().Title;

            // Fields
            List<Hashtable> listaField = new List<Hashtable>();

            bool ExistProfilePart = true;
            bool ExistCommunicationContactPart = true;
            dynamic fields = null;

            try {fields = ((dynamic)content).CommunicationContactPart.Fields;}
            catch { ExistCommunicationContactPart = false; }
            if (ExistCommunicationContactPart)
            foreach (dynamic cf in fields) {
                Hashtable hs = ConvertField(cf,"CommunicationContactPart");
                if (hs.Count > 0)
                    listaField.Add(hs);
            }




            try {
                fields = ((dynamic)content).ProfilePart.Fields;
            } catch {
                ExistProfilePart = false;
            }



            if (ExistProfilePart) {
                foreach (dynamic cf in fields) {
                    Hashtable hs = ConvertField(cf, "ProfilePart");
                    if (hs.Count>0)
                        listaField.Add(hs);
                    //if (cf.FieldDefinition.Name != typeof(ContentPickerField).Name && cf.FieldDefinition.Name != typeof(MediaLibraryPickerField).Name) {
                    //    string keyField = "ProfilePart." + ((object)cf.DisplayName).ToString();
                    //    string valueField = "";

                    //    if (cf.FieldDefinition.Name == typeof(DateTimeField).Name) {
                    //        if (cf.DateTime != null && !cf.DateTime.Equals(DateTime.MinValue))
                    //            valueField = ((object)cf.DateTime).ToString();
                    //    }
                    //    else if (cf.FieldDefinition.Name == typeof(TaxonomyField).Name) {
                    //        if (((TaxonomyField)cf).Terms != null) {
                    //            foreach (TermPart term in ((TaxonomyField)cf).Terms) {
                    //                // Più termini selezionati
                    //                if (valueField != "")
                    //                    valueField = ";" + valueField;

                    //                if (term.Path == "/") {
                    //                    // Taxonomy ad un livello
                    //                    valueField = term.Name.Replace('/', '\\').Replace(";", ".,") + valueField;
                    //                } 
                    //                else {
                    //                    // Taxonomy su più livelli
                    //                    GetValueCompletoTerms(term, ref valueField);
                    //                }
                    //            }
                    //        }
                    //    } else {
                    //        if (cf.Value != null)
                    //            valueField = ((object)cf.Value).ToString();
                    //    }

                    //    Hashtable hs = new Hashtable();
                    //    hs.Add(keyField, valueField);
                    //    listaField.Add(hs);
                    //}
                }
            }
            contact.Fields = listaField;

            // Sms
            List<string> listaSms = new List<string>();
            string smsPrefix = "";
            SmsContactPart smspart = content.As<SmsContactPart>();
            foreach (CommunicationSmsRecord sms in smspart.Record.SmsRecord) {
                // Rimuovo il carattere '+' perchè Excel lo considera come una formula
                smsPrefix = sms.Prefix ?? "";
                if (smsPrefix.StartsWith("+"))
                    smsPrefix = smsPrefix.Substring(1);

                listaSms.Add(smsPrefix + "/" + sms.Sms);
            }
            contact.Sms = listaSms;

            // Mail
            List<string> listaMail = new List<string>();

            EmailContactPart mailpart = content.As<EmailContactPart>();
            foreach (CommunicationEmailRecord mail in mailpart.Record.EmailRecord) {
                listaMail.Add(mail.Email);
            }
            contact.Mail = listaMail;

            return contact;
        }


        private void GetValueCompletoTerms(TermPart term, ref string valueTerm) {

            valueTerm = "/" + term.Name.Replace('/', '\\').Replace(";", ".,") + valueTerm;

            string padre = term.FullPath.Split('/')[term.FullPath.Split('/').Length - 2];

            if (padre != "") {
                // Metodo ricorsivo
                TermPart termFather = _taxonomyService.GetTerm(Convert.ToInt32(padre));
                GetValueCompletoTerms(termFather, ref valueTerm);
            } 
            else {
                // Rimuovo primo carattere '/'
                valueTerm = valueTerm.Substring(1);
            }
        }

    }
}