using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using Orchard.ContentManagement;
using Orchard;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.Core.Title.Models;
using Laser.Orchard.CommunicationGateway.Models;
using Orchard.Data;
using Orchard.Taxonomies.Fields;
using Orchard.ContentPicker.Fields;
using Orchard.MediaLibrary.Fields;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Models;

namespace Laser.Orchard.CommunicationGateway.Utils {
    public class ImportUtil {
        private class PartialRecord {
            public int Id { get; set; }
            public string Name { get; set; }
            public Dictionary<string, string> Profile { get; set; }
            public Dictionary<string, string> CommunicationContact { get; set; }
            public PartialRecord() {
                Id = 0;
                Name = "";
                Profile = new Dictionary<string, string>();
                CommunicationContact = new Dictionary<string, string>();
            }
        }

        public List<string> Errors { get; set; }
        public int TotMail { get; set; }
        public int TotSms { get; set; }

        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<CommunicationEmailRecord> _repositoryCommunicationEmailRecord;
        private readonly IRepository<CommunicationSmsRecord> _repositoryCommunicationSmsRecord;
        private readonly ITaxonomyService _taxonomyService;

        private const char fieldSeparator = ';';
        private const char smsSeparator = '/';
        private const char taxoPathSeparator = '/';
        private const char taxoValuesSeparator = ';';

        private int idxContactId = 0;
        private int idxContactEmail = 0;
        private int idxContactSms = 0;
        private int idxContactName = 0;

        public ImportUtil(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            _contentManager = _orchardServices.ContentManager;
            _communicationService = _orchardServices.WorkContext.Resolve<ICommunicationService>();
            _repositoryCommunicationEmailRecord = _orchardServices.WorkContext.Resolve<IRepository<CommunicationEmailRecord>>();
            _repositoryCommunicationSmsRecord = _orchardServices.WorkContext.Resolve<IRepository<CommunicationSmsRecord>>();
            _taxonomyService = _orchardServices.WorkContext.Resolve<ITaxonomyService>();
        }

        public void ImportCsv(string fileContent) {
            Errors = new List<string>();
            string[] lines = null;
            List<string> intestazione = null;
            List<string> record = null;

            try {
                // suddivide il contenuto del file nelle varie righe
                string[] lineSeparator = { Environment.NewLine };
                lines = fileContent.Split(lineSeparator, StringSplitOptions.None);
            }
            catch (Exception ex) {
                Errors.Add("File format not valid. " + ex.Message);
            }

            if (Errors.Count == 0) {
                if (lines.Count() > 1) {
                    // legge l'intestazione
                    intestazione = GetFields(lines[0]);
                    // normalizza l'intestazione nel caso di ProfilePart
                    string header = null;
                    for (int i = 0; i < intestazione.Count; i++) {
                        header = intestazione[i];
                        if (header.StartsWith("ProfilePart.")) {
                            intestazione[i] = header.Substring(12); //12: lunghezza di "ProfilePart."
                        }
                        //if (header.StartsWith("CommunicationContactPart.")) {
                        //    intestazione[i] = header.Substring(25); //12: lunghezza di "CommunicationContactPart."
                        //}
                    }

                    // controlla l'esistenza e la posizione dei campi cardine (ID, Name, mail, sms)
                    bool check = CheckForMainFields(intestazione);
                    if (check) {
                        //scandisce le righe del file
                        for (int i = 1; i < lines.Length; i++) {
                            if (string.IsNullOrWhiteSpace(lines[i]) == false) {
                                record = GetFields(lines[i]);
                                if (record.Count > 0) {
                                    ImportRecord(intestazione, record);
                                }
                            }
                        }
                    }
                    else {
                        Errors.Add("At least a main field is missing. Main fields are: Id, TitlePart.Title, ContactPart.Email, ContactPart.Sms");
                    }
                }
                else {
                    Errors.Add("Empty file: no data found after headers.");
                }
            }
        }

        private void ImportRecord(List<string> intestazioni, List<string> campi) {
            string[] elencoMail = null;
            string[] elencoSms = null;
            PartialRecord partialRecord = new PartialRecord();

            // popola le strutture dati di appoggio
            LoadData(intestazioni, campi, ref partialRecord, ref elencoMail, ref elencoSms);

            // gestisce le mail
            foreach (var mail in elencoMail) {
                ImportMail(mail, partialRecord);
            }

            // gestisce gli sms
            string[] smsComponents = null; // prefix and sms
            foreach (var prefixAndSms in elencoSms) {
                smsComponents = prefixAndSms.Split(smsSeparator);
                if ((smsComponents.Count() > 1) && (string.IsNullOrWhiteSpace(smsComponents[1]) == false)) {
                    ImportSms(smsComponents[0], smsComponents[1], partialRecord);
                }
            }
        }

        private void LoadData(List<string> intestazioni, List<string> campi, ref PartialRecord partialRecord, ref string[] elencoMail, ref string[] elencoSms) {
            string campo = null;
            for (int i = 0; i < campi.Count; i++) {
                campo = campi[i];
                if (i == idxContactId) {
                    if (string.IsNullOrWhiteSpace(campo) == false) {
                        partialRecord.Id = Convert.ToInt32(campo);
                    }
                }
                else if (i == idxContactName) {
                    partialRecord.Name = campo;
                }
                else if (i == idxContactEmail) {
                    if (string.IsNullOrWhiteSpace(campo)) {
                        elencoMail = new string[0];
                    }
                    else {
                        elencoMail = campo.Split(fieldSeparator);
                    }
                }
                else if (i == idxContactSms) {
                    if (string.IsNullOrWhiteSpace(campo)) {
                        elencoSms = new string[0];
                    }
                    else {
                        elencoSms = campo.Split(fieldSeparator);
                    }
                }
                else if (string.IsNullOrWhiteSpace(intestazioni[i]) == false) {
                    if (intestazioni[i].StartsWith("CommunicationContactPart."))
                        partialRecord.CommunicationContact.Add(intestazioni[i].Substring(25), campo);
                    else
                        partialRecord.Profile.Add(intestazioni[i], campo);
                }
            }
        }

        private void ImportMail(string mail, PartialRecord partialRecord) {
            ContentItem contact = null;
            var elencoItems = _communicationService.GetContactsFromMail(mail);
            if (elencoItems.Count == 0) {
                contact = FindContact(partialRecord.Name);
                if (contact == null) {
                    contact = CreateContact(partialRecord);
                }
                CreateMailForContact(mail, contact.Id);
                UpdateContactInfo(contact, partialRecord);
                TotMail++;
            }
            else {
                foreach (var item in elencoItems) {
                    if (IsSameContact(item, partialRecord)) {
                        UpdateContactInfo(item, partialRecord);
                        TotMail++;
                    }
                    else {
                        if (IsMasterContact(item)) {
                            contact = FindContact(partialRecord.Name);
                            if (contact == null) {
                                contact = CreateContact(partialRecord);
                            }
                            // sgancia la mail dal master e la associa al contatto
                            var cer = _repositoryCommunicationEmailRecord.Get(x => x.Email == mail);
                            cer.EmailContactPartRecord_Id = contact.Id;
                            _repositoryCommunicationEmailRecord.Update(cer);

                            UpdateContactInfo(contact, partialRecord);
                            TotMail++;
                        }
                        else {
                            var title = item.As<TitlePart>().Title;
                            Errors.Add(string.Format("Mail {0} already assigned to contact \"{1}\" (id: {2}).", mail, title, item.Id));
                        }
                    }
                }
            }
        }

        private void ImportSms(string prefix, string sms, PartialRecord partialRecord) {
            ContentItem contact = null;
            var elencoItems = _communicationService.GetContactsFromSms(prefix, sms);
            if (elencoItems.Count == 0) {
                contact = FindContact(partialRecord.Name);
                if (contact == null) {
                    contact = CreateContact(partialRecord);
                }
                CreateSmsForContact(prefix, sms, contact.Id);
                UpdateContactInfo(contact, partialRecord);
                TotSms++;
            }
            else {
                foreach (var item in elencoItems) {
                    if (IsSameContact(item, partialRecord)) {
                        UpdateContactInfo(item, partialRecord);
                        TotSms++;
                    }
                    else {
                        if (IsMasterContact(item)) {
                            contact = FindContact(partialRecord.Name);
                            if (contact == null) {
                                contact = CreateContact(partialRecord);
                            }
                            // sgancia la mail dal master e la associa al contatto
                            var csr = _repositoryCommunicationSmsRecord.Get(x => x.Prefix == prefix && x.Sms == sms);
                            csr.SmsContactPartRecord_Id = contact.Id;
                            _repositoryCommunicationSmsRecord.Update(csr);

                            UpdateContactInfo(contact, partialRecord);
                            TotSms++;
                        }
                        else {
                            var title = item.As<TitlePart>().Title;
                            Errors.Add(string.Format("Sms phone number {0}/{1} already assigned to contact \"{2}\" (id: {3}).", prefix, sms, title, item.Id));
                        }
                    }
                }
            }
        }

        private bool IsMasterContact(ContentItem contactItem) {
            bool result = false;
            var contactPart = contactItem.As<CommunicationContactPart>();
            if ((contactPart != null) && (contactPart.Master)) {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Cerca un contatto nel DB in base a ID e nome.
        /// </summary>
        /// <param name="partialRecord"></param>
        /// <returns></returns>
        private ContentItem FindContact(string name) {
            ContentItem result = _communicationService.GetContactFromName(name);
            return result;
        }

        private void UpdateSinglePart(ContentItem ci,KeyValuePair<string,string>prop,ContentField cf) {
           // var cf = profile.Fields.Where(x => x.Name == prop.Key).FirstOrDefault();
            if (cf != null) {
                if (cf.GetType() == typeof(DateTime)) {
                    ((dynamic)cf).DateTime = prop.Value;
                }
                else if (cf.GetType() == typeof(TaxonomyField)) {
                    UpdateTaxonomyField(ci, cf as TaxonomyField, prop.Value);
                }
                else if ((cf.GetType() == typeof(MediaLibraryPickerField)) || (cf.GetType() == typeof(ContentPickerField))) {
                    Errors.Add(string.Format("Property {0} of type {1} cannot be updated by CSV import.", prop.Key, cf.GetType().Name));
                }
                else {
                    ((dynamic)cf).Value = prop.Value;
                }
            }
            else {
                Errors.Add(string.Format("Field \"{0}\" not found in Contact.", prop.Key));
            }
        }

        /// <summary>
        /// Aggiorna le altre info del contatto: nome e profile part.
        /// </summary>
        private void UpdateContactInfo(ContentItem ci, PartialRecord partialRecord) {
            ci.As<TitlePart>().Title = partialRecord.Name;
            
            ContentPart profile = ((dynamic)ci).ProfilePart;
            if (profile != null) {
                foreach (var prop in partialRecord.Profile) {
                           var cf = profile.Fields.Where(x => x.Name == prop.Key).FirstOrDefault();
                    UpdateSinglePart(ci,prop,cf);
                }
            }

            ContentPart CommunicationContact = ((dynamic)ci).CommunicationContactPart;
            if (CommunicationContact != null) {
                foreach (var prop in partialRecord.CommunicationContact) {
                    var cf = CommunicationContact.Fields.Where(x => x.Name == prop.Key).FirstOrDefault();
                    UpdateSinglePart(ci,prop,cf);
                }
            }  
                    //if (cf != null) {
                    //    if (cf.GetType() == typeof(DateTime)) {
                    //        ((dynamic)cf).DateTime = prop.Value;
                    //    }
                    //    else if (cf.GetType() == typeof(TaxonomyField)) {
                    //        UpdateTaxonomyField(ci, cf as TaxonomyField, prop.Value);
                    //    }
                    //    else if ((cf.GetType() == typeof(MediaLibraryPickerField)) || (cf.GetType() == typeof(ContentPickerField))) {
                    //        Errors.Add(string.Format("Property {0} of type {1} cannot be updated by CSV import.", prop.Key, cf.GetType().Name));
                    //    }
                    //    else {
                    //        ((dynamic)cf).Value = prop.Value;
                    //    }
                    //}
                    //else {
                    //    Errors.Add(string.Format("Field \"{0}\" not found in Contact.", prop.Key));
                    //}
           
        }

        private void UpdateTaxonomyField(ContentItem ci, TaxonomyField tf, string valuesList) {
            List<TermPart> termlist = new List<TermPart>();
            TermPart term = null;
            if (string.IsNullOrWhiteSpace(valuesList) == false) {
                var taxoTerms = GetTaxonomyTerms(tf);
                if (taxoTerms != null) {
                    foreach (string locValue in valuesList.Split(taxoValuesSeparator)) {
                        if (string.IsNullOrWhiteSpace(locValue) == false) {
                            string path = "" + taxoPathSeparator;
                            string termName = null;
                            foreach (string section in locValue.Split(taxoPathSeparator)) {
                                termName = section.Replace('\\', '/').Replace(".,", ";");
                                term = taxoTerms.FirstOrDefault(x => x.Path == path && x.Name == termName);
                                if (term != null) {
                                    path = term.FullPath + taxoPathSeparator;
                                }
                                else {
                                    term = null;
                                    break;
                                }
                            }
                            if (term != null) {
                                termlist.Add(term);
                            }
                            else {
                                Errors.Add(string.Format("Taxonomy term \"{0}\" not found on field \"{1}\".", locValue, tf.Name));
                            }
                        }
                    }
                }
                else {
                    Errors.Add(string.Format("Taxonomy not found for field \"{0}\".", tf.DisplayName));
                }
                var oldTerms = _taxonomyService.GetTermsForContentItem(ci.Id, tf.Name);

                // verifica se è necessario aggiornare il campo
                bool updateNeeded = false;
                if (oldTerms.Count() == termlist.Count) {
                    foreach (var term0 in termlist) {
                        if (oldTerms.Contains(term0) == false) {
                            updateNeeded = true;
                            break;
                        }
                    }
                }
                else {
                    updateNeeded = true;
                }

                if (updateNeeded) {
                    _taxonomyService.UpdateTerms(ci, termlist, tf.Name);
                }
            }
        }

        private IEnumerable<TermPart> GetTaxonomyTerms(TaxonomyField tf) {
            IEnumerable<TermPart> result = null;
            var taxoSettings = tf.PartFieldDefinition.Settings;
            string taxoName = taxoSettings.Where(x => x.Key == "TaxonomyFieldSettings.Taxonomy").Select(x => x.Value).FirstOrDefault();
            TaxonomyPart taxoPart = _taxonomyService.GetTaxonomyByName(taxoName);
            if (taxoPart != null) {
                result = taxoPart.Terms;
            }
            return result;
        }

        private void CreateMailForContact(string mail, int contactId) {
            CommunicationEmailRecord cer = new CommunicationEmailRecord();
            cer.Email = mail;
            cer.DataInserimento = DateTime.Now;
            cer.DataModifica = DateTime.Now;
            cer.Produzione = true;
            cer.Validated = true;
            cer.EmailContactPartRecord_Id = contactId;
            _repositoryCommunicationEmailRecord.Create(cer);
        }

        private void CreateSmsForContact(string prefix, string sms, int contactId) {
            CommunicationSmsRecord csr = new CommunicationSmsRecord();
            csr.Prefix = prefix;
            csr.Sms = sms;
            csr.DataInserimento = DateTime.Now;
            csr.DataModifica = DateTime.Now;
            csr.Produzione = true;
            csr.Validated = true;
            csr.SmsContactPartRecord_Id = contactId;
            _repositoryCommunicationSmsRecord.Create(csr);
        }
        //private void CreateContactForMail(PartialRecord partialRecord, string mail) {
        //    var ci = CreateContact(partialRecord);
        //    CommunicationEmailRecord cer = new CommunicationEmailRecord();
        //    cer.Email = mail;
        //    cer.DataInserimento = DateTime.Now;
        //    cer.DataModifica = DateTime.Now;
        //    cer.Produzione = true;
        //    cer.Validated = true;
        //    cer.EmailContactPartRecord_Id = ci.Id;
        //    _repositoryCommunicationEmailRecord.Create(cer);
        //}

        //private void CreateContactForSms(PartialRecord partialRecord, string prefix, string sms) {
        //    var ci = CreateContact(partialRecord);
        //    CommunicationSmsRecord csr = new CommunicationSmsRecord();
        //    csr.Prefix = prefix;
        //    csr.Sms = sms;
        //    csr.DataInserimento = DateTime.Now;
        //    csr.DataModifica = DateTime.Now;
        //    csr.Produzione = true;
        //    csr.Validated = true;
        //    csr.SmsContactPartRecord_Id = ci.Id;
        //    _repositoryCommunicationSmsRecord.Create(csr);
        //}

        private ContentItem CreateContact(PartialRecord partialRecord) {
            ContentItem ci = _contentManager.Create("CommunicationContact");
            ci.As<TitlePart>().Title = partialRecord.Name;
            return ci;
        }

        /// <summary>
        /// Controlla se il content item appartiene allo stesso contatto: verifica prima per nome e poi per id.
        /// La verifica viene fattaprima per nome per non considerare diversi i casi in cui un contatto era stato esportato,
        /// poi eliminato e poi reimportato. In questo caso il sistema gli ha assegnato un id diverso.
        /// </summary>
        /// <param name="ci"></param>
        /// <param name="partialRecord"></param>
        /// <returns></returns>
        private bool IsSameContact(ContentItem ci, PartialRecord partialRecord) {
            bool result = false;
            if (partialRecord.Name == ci.As<TitlePart>().Title) {
                result = true;
            }
            else if (partialRecord.Id != 0) {
                if (partialRecord.Id == ci.Id) {
                    result = true;
                }
            }
            return result;
        }

        private bool CheckForMainFields(List<string> intestazioni) {
            bool result = false;
            idxContactId = -1;
            idxContactName = -1;
            idxContactEmail = -1;
            idxContactSms = -1;

            for (var i = 0; i < intestazioni.Count; i++) {
                if (intestazioni[i].CompareTo("Id") == 0) {
                    idxContactId = i;
                }
                else if (intestazioni[i].CompareTo("TitlePart.Title") == 0) {
                    idxContactName = i;
                }

                else if (intestazioni[i].CompareTo("ContactPart.Email") == 0) {
                    idxContactEmail = i;
                }
                else if (intestazioni[i].CompareTo("ContactPart.Sms") == 0) {
                    idxContactSms = i;
                }
            }
            if ((idxContactId != -1) && (idxContactName != -1) && (idxContactEmail != -1) && (idxContactSms != -1)) {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Suddivide una string adi testo in campi separati da semicolon (;).
        /// Se in un campo sono presenti i caratteri semicolon o doppio apice, esso sarà delimitato da doppi apici (es. "Libro/Giallo;Film/Comico").
        /// Se in un campo è prensete un doppio apice, questo viene raddoppiato (es. "John disse: ""Geronimo!"" e si tuffò.").
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private List<string> GetFields(string line) {
            List<string> result = new List<string>();
            const char stringDelimiter = '"';
            const char doppiApici = '\0';
            bool insideAString = false;
            StringBuilder sb = new StringBuilder();

            // sostituisce eventuali coppie di doppi apici consecutivi con un carattere di comodo
            string row = line.Replace("\"\"", "" + doppiApici);
            for (int i = 0; i < row.Length; i++) {
                switch (row[i]) {
                    case fieldSeparator:
                        if (insideAString) {
                            sb.Append(row[i]);
                        }
                        else {
                            result.Add(sb.ToString());
                            sb.Clear();
                        }
                        break;
                    case stringDelimiter:
                        if (insideAString) {
                            // fine della stringa
                            insideAString = false;
                        }
                        else {
                            // inizio della stringa
                            insideAString = true;
                        }
                        break;
                    case doppiApici:
                        sb.Append("\"");
                        break;
                    default:
                        sb.Append(row[i]);
                        break;
                }
            }
            // aggiunge l'ultimo campo
            result.Add(sb.ToString());
            return result;
        }
    }
}