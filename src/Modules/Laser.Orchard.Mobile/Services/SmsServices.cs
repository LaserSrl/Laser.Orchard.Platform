using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.SmsServiceReference;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Tokens;
using Orchard.Users.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;

namespace Laser.Orchard.Mobile.Services {

    public interface ISmsServices : IDependency {
        //string SendSms(long[] TelDestArr, string TestoSMS, string alias = null, string IdSMS = null, bool InviaConAlias = false);
        //string SendSms(IList<SmsHQL> TelDestArr, string TestoSMS, string alias = null, string IdSMS = null, bool InviaConAlias = false);
        string SendSms(IList TelDestArr, string TestoSMS, string alias = null, string IdSMS = null, bool InviaConAlias = false);
        Config GetConfig();
        int GetStatus();
        SmsDeliveryReportResultVM GetReportSmsStatus(string IdSMS);
        void Synchronize();
    }

    [OrchardFeature("Laser.Orchard.Sms")]
    public class SmsServices : ISmsServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<CommunicationSmsRecord> _repositoryCommunicationSmsRecord;
        private readonly ITokenizer _tokenizer;

        public const int MSG_MAX_CHAR_NUMBER_SINGOLO = 160;
        public const int MSG_MAX_CHAR_NUMBER_CONCATENATI = 1530;

        private const string PREFISSO_PLACE_HOLDER = "[PH_";

        public SmsServices(IOrchardServices orchardServices, IRepository<CommunicationSmsRecord> repositoryCommunicationSmsRecord, ITokenizer tokenizer)
        {
            _repositoryCommunicationSmsRecord = repositoryCommunicationSmsRecord;
            _orchardServices = orchardServices;
            _tokenizer = tokenizer;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        public void Synchronize() {
            #region lego tutti gli sms
            var alluser = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(x => x.RegistrationStatus == UserStatus.Approved);
            if (alluser.List().FirstOrDefault().As<UserPwdRecoveryPart>() != null) {
                var allusercol = alluser.List().Where(x => !string.IsNullOrEmpty(x.ContentItem.As<UserPwdRecoveryPart>().PhoneNumber)).ToList();
                foreach (IContent user in allusercol) {
                    string pref = user.ContentItem.As<UserPwdRecoveryPart>().InternationalPrefix;
                    string num = user.ContentItem.As<UserPwdRecoveryPart>().PhoneNumber;
                    CommunicationSmsRecord csr = _repositoryCommunicationSmsRecord.Fetch(x => x.Sms == num && x.Prefix == pref).FirstOrDefault();
                    CommunicationContactPart ciCommunication = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == user.Id).List().FirstOrDefault();
                    if (ciCommunication == null) {
                        // Una contact part dovrebbe esserci in quanto questo codice viene eseguito dopo la sincronizzazione utenti
                        // Se non vi è una contartpart deduco che il dato sia sporco (es: UUid di un utente che è stato cancellato quindi non sincronizzo il dato con contactpart, verrà legato come se fosse scollegato al contentitem che raggruppa tutti i scollegati)
                        //throw new Exception("Utente senza associazione alla profilazione");
                    } else {
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
                        } else {
                            if (csr.SmsContactPartRecord_Id != ciCommunication.ContentItem.Id) {
                                csr.SmsContactPartRecord_Id = ciCommunication.ContentItem.Id;
                                csr.DataModifica = DateTime.Now;
                                _repositoryCommunicationSmsRecord.Update(csr);
                                _repositoryCommunicationSmsRecord.Flush();
                            }
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Data una Hashtable frutto di una query HQL e di un AliasToEntityMap, cerca la componenete SmsContactPartRecord 
        /// e ne estrae il numero di telefono (prefisso + numero).
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string GetPhoneNumber(Hashtable content)
        {
            string risultato = "";
            SmsContactPartRecord contactRecord = null;
            CommunicationSmsRecord smsRecord = null;

            // Send for Contact Part
            if (content.ContainsKey("SmsContactPartRecord"))
            {
                contactRecord = (content["SmsContactPartRecord"] as SmsContactPartRecord);
                if ((contactRecord.SmsRecord != null) && (contactRecord.SmsRecord.Count > 0))
                {
                    smsRecord = contactRecord.SmsRecord[0];
                    risultato = smsRecord.Prefix + smsRecord.Sms;
                }
            } 
            // Send for Lista Destinatari
            else if (content.ContainsKey("SmsContactNumber")) {
                risultato = content["SmsContactNumber"].ToString();
            }

            ////versione senza l'uso degli alias nella query HQL
            //foreach (DictionaryEntry obj in content)
            //{
            //    if (obj.Value.GetType() == typeof(SmsContactPartRecord))
            //    {
            //        contactRecord = (obj.Value as SmsContactPartRecord);
            //        if ((contactRecord.SmsRecord != null) && (contactRecord.SmsRecord.Count > 0))
            //        {
            //            smsRecord = contactRecord.SmsRecord[0];
            //            risultato = smsRecord.Prefix + smsRecord.Sms;
            //            break;
            //        }
            //    }
            //}
            return risultato;
        }

        //public string SendSms(long[] telDestArr, string testoSMS, string alias = null, string IdSMS = null, bool InviaConAlias = false) {
        //public string SendSms(IList<SmsHQL> telDestArr, string testoSMS, string alias = null, string IdSMS = null, bool InviaConAlias = false) {
        public string SendSms(IList telDestArr, string testoSMS, string alias = null, string IdSMS = null, bool InviaConAlias = false) {
            var bRet = "FALSE";

            ArrayOfLong numbers = new ArrayOfLong();
            foreach (Hashtable tel in telDestArr)
            {
                numbers.Add(Convert.ToInt64(GetPhoneNumber(tel)));
            }

            //numbers.AddRange(telDestArr.Select(x => Convert.ToInt64(x.SmsPrefix + x.SmsNumber)).ToArray());

            try {
                var smsSettings = _orchardServices.WorkContext.CurrentSite.As<SmsSettingsPart>();

                // Imposto Guid univoco se la richiesta non arriva da SmsGateway
                if (String.IsNullOrEmpty(IdSMS)) {
                    IdSMS = new Guid().ToString();
                }

                if (InviaConAlias) {
                    if (String.IsNullOrEmpty(alias)) {
                        alias = smsSettings.SmsFrom;
                    }
                } else {
                    alias = null;
                }

                SmsServiceReference.Sms sms = new SmsServiceReference.Sms {
                    DriverId = smsSettings.MamDriverIdentifier,
                    SmsFrom = (_orchardServices.WorkContext.CurrentUser!=null?_orchardServices.WorkContext.CurrentUser.UserName:"system"),
                    MamHaveAlias = InviaConAlias,
                    Alias = alias,
                    SmsPrority = smsSettings.SmsPrority ?? 0,
                    SmsValidityPeriod = smsSettings.SmsValidityPeriod ?? 3600,
                    ExternalId = IdSMS,
                    SmsBody = testoSMS,
                    SmsTipoCodifica = 0,
                    SmsNumber = numbers,
                };

                SmsServiceReference.SmsWebServiceSoapClient _service = CreateSmsService(smsSettings);

                // Place Holder
                List<SmsServiceReference.PlaceHolderMessaggio> listPH = GetPlaceHolder(telDestArr, testoSMS);
                SmsServiceReference.PlaceHolderMessaggio[] SmsPlaceHolder = null;
                if (listPH != null && listPH.Count > 0) {
                    SmsPlaceHolder = listPH.ToArray();
                }

                // Login
                SmsServiceReference.Login login = new SmsServiceReference.Login();
                login.User = smsSettings.WsUsername;
                login.Password = smsSettings.WsPassword;
                login.DriverId = smsSettings.MamDriverIdentifier;

                var result = _service.SendSMS(login, sms, SmsPlaceHolder);

                //Log.Info(Metodo + " Inviato SMS ID: " + idSmsComponent);
                bRet = result;

            } catch (Exception ex) {
                Logger.Error(ex, ex.Message + " :: " + ex.StackTrace);
            }

            return bRet;
        }

        //private List<SmsServiceReference.PlaceHolderMessaggio> GetPlaceHolder(long[] telDestArr, string testoSMS) {
        //private List<SmsServiceReference.PlaceHolderMessaggio> GetPlaceHolder(IList<SmsHQL> telDestArr, string testoSMS) {
        private List<SmsServiceReference.PlaceHolderMessaggio> GetPlaceHolder(IList telDestArr, string testoSMS) {
            List<SmsServiceReference.PlaceHolderMessaggio> listaPH = null;

            var smsPlaceholdersSettingsPart = _orchardServices.WorkContext.CurrentSite.As<SmsPlaceholdersSettingsPart>();

            if (smsPlaceholdersSettingsPart.PlaceholdersList.Placeholders.Count() > 0 && testoSMS.Contains(PREFISSO_PLACE_HOLDER)) {
                listaPH = new List<SmsServiceReference.PlaceHolderMessaggio>();

                foreach (Hashtable dest in telDestArr)
                {
                    SmsServiceReference.PlaceHolderMessaggio ph = new SmsServiceReference.PlaceHolderMessaggio();
                    ph.Telefono = GetPhoneNumber(dest);

                    List<SmsServiceReference.PHChiaveValore> listaCV = new List<SmsServiceReference.PHChiaveValore>();

                    foreach(var settingsPH in smsPlaceholdersSettingsPart.PlaceholdersList.Placeholders) 
                    {
                        SmsServiceReference.PHChiaveValore ph_SettingsCV = new SmsServiceReference.PHChiaveValore();
                        ph_SettingsCV.Chiave = "[PH_" + settingsPH.Name + "]";

                        if (settingsPH.Value.StartsWith("."))
                        {
                            ph_SettingsCV.Valore = GetCustomTokenValue(settingsPH.Value, dest);
                        }
                        else
                        {
                            ph_SettingsCV.Valore = _tokenizer.Replace(settingsPH.Value, null);
                        }
                        listaCV.Add(ph_SettingsCV);
                    }
                    ph.ListaPHChiaveValore = listaCV.ToArray();
                    listaPH.Add(ph);
                }
            }
            return listaPH;
        }

        /// <summary>
        /// Data una Hashtable di oggetti, la naviga in base al token e restituisce il valore corrispondente.
        /// Il token deve essere nella forma: chiave_hashtable.proprietà1.proprietà2...
        /// Il token può iniziare con il punto che verrà però ignorato.
        /// Sono gestiti gli indexer numerici (es. classe.proprietà1[0]).
        /// </summary>
        /// <param name="token"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetCustomTokenValue(string token, Hashtable context)
        {
            string risultato = "";
            char[] sepProprieta = { '.' };
            char[] sepIndexer = { '[', ']' };
            object obj = null;
            string[] arrAux = null;
            string indexer = "";
            object[] arrIdx = { 0 };
            string[] navigation = token.Split(sepProprieta, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                if (navigation.Length > 0)
                {
                    obj = context[navigation[0]];

                    // scende nelle properties dell'oggetto
                    for (int i = 1; i < navigation.Length; i++)
                    {
                        if (navigation[i].EndsWith("]"))
                        {
                            // gestione proprietà con indexer
                            arrAux = navigation[i].Split(sepIndexer, StringSplitOptions.RemoveEmptyEntries);
                            if (arrAux.Length == 2)
                            {
                                indexer = arrAux[0];
                                arrIdx[0] = Convert.ToInt32(arrAux[1]);
                                if (obj.GetType().GetProperty(indexer).GetIndexParameters().Length > 0)
                                {
                                    //indexer esplicito
                                    obj = obj.GetType().GetProperty(indexer).GetValue(obj, arrIdx);
                                }
                                else
                                {
                                    //indexer implicito
                                    obj = obj.GetType().GetProperty(indexer).GetValue(obj);
                                    obj = obj.GetType().GetProperty("Item").GetValue(obj, arrIdx);
                                }
                            }
                        }
                        else
                        {
                            // gestione proprietà senza indexer
                            obj = obj.GetType().GetProperty(navigation[i]).GetValue(obj);
                        }
                    }
                    risultato = obj.ToString();
                }
            }
            catch 
            {
                // ignora volutamente qualsiasi errore e restituisce una stringa vuota
            }
            return risultato;
        }

        public Config GetConfig() {
            //Specify the binding to be used for the client.
            var smsSettings = _orchardServices.WorkContext.CurrentSite.As<SmsSettingsPart>();
            SmsServiceReference.SmsWebServiceSoapClient _service = CreateSmsService(smsSettings);

            SmsServiceReference.Login login = new SmsServiceReference.Login();
            login.User = smsSettings.WsUsername;
            login.Password = smsSettings.WsPassword;
            login.DriverId = smsSettings.MamDriverIdentifier;

            var result = _service.GetConfig(login);

            return result;
        }

        public SmsDeliveryReportResultVM GetReportSmsStatus(string IdSMS) {
            SmsDeliveryReportResultVM esito = new SmsDeliveryReportResultVM();
            var smsSettings = _orchardServices.WorkContext.CurrentSite.As<SmsSettingsPart>();
            SmsServiceReference.SmsWebServiceSoapClient _service = CreateSmsService(smsSettings);

            SmsServiceReference.Login login = new SmsServiceReference.Login();
            login.User = smsSettings.WsUsername;
            login.Password = smsSettings.WsPassword;
            login.DriverId = smsSettings.MamDriverIdentifier;

            // Dettaglio Transfer Delivery Report
            SmsServiceReference.StatusByExtId[] ret = _service.GetSmsStateByExternalId(login, IdSMS);

            if (ret != null && ret.Length > 0) {
            int contACCEPTED = (from sms in ret where sms.SmsState.CompareTo("ACCEPTED") == 0 select sms).Count();
            int contDELIVERED = (from sms in ret where sms.SmsState.CompareTo("DELIVERED") == 0 select sms).Count();
            int contEXPIRED = (from sms in ret where sms.SmsState.CompareTo("EXPIRED") == 0 select sms).Count();
            int contREJECTED = (from sms in ret where sms.SmsState.CompareTo("REJECTED") == 0 select sms).Count();

            int contSmsTotali = contACCEPTED + contDELIVERED + contEXPIRED + contREJECTED;
            int contSmsInviati = contACCEPTED + contDELIVERED;
            int contSmsFalliti = contEXPIRED + contREJECTED;

                string reportStatus = "Tot Utenti: " + contSmsTotali.ToString();
            reportStatus += " - Inviati: " + contSmsInviati.ToString();
            reportStatus += " (Consegnati al terminale: " + contDELIVERED.ToString() + " - Consegnati all'operatore: " + contACCEPTED.ToString() + ")";
            reportStatus += " - Falliti: " + contSmsFalliti.ToString();
            reportStatus += " (Rejected: " + contREJECTED.ToString() + " - Expired: " + contEXPIRED.ToString() + ")";

                List<SmsDeliveryReportDetails> listDetails = new List<SmsDeliveryReportDetails>();
                foreach (SmsServiceReference.StatusByExtId report in ret) {
                    SmsDeliveryReportDetails detail = new SmsDeliveryReportDetails();

                    detail.Recipient = report.SmsNumber.ToString();
                    detail.SubmittedDate = Convert.ToDateTime(report.SmsLastUpdate, new CultureInfo("it-IT"));
                    detail.RequestDate = Convert.ToDateTime(report.SmsSentDate, new CultureInfo("it-IT"));
                    detail.Status = report.SmsState;

                    listDetails.Add(detail);
                }

                esito.ReportStatus = reportStatus;
                esito.Details = listDetails;
            } 

            else {
                esito = null;
            }

            return esito;
        }

        public int GetStatus() {
            //Specify the binding to be used for the client.
            var smsSettings = _orchardServices.WorkContext.CurrentSite.As<SmsSettingsPart>();

            EndpointAddress address = new EndpointAddress(smsSettings.SmsServiceEndPoint);
            BasicHttpBinding binding = new BasicHttpBinding();

            if (smsSettings.SmsServiceEndPoint.ToLower().StartsWith("https://")) {
                binding.Security.Mode = BasicHttpSecurityMode.Transport;
            }
            SmsServiceReference.SmsWebServiceSoapClient _service = new SmsWebServiceSoapClient(binding, address);

            SmsServiceReference.Login login = new SmsServiceReference.Login();
            login.User = smsSettings.WsUsername;
            login.Password = smsSettings.WsPassword;
            login.DriverId = smsSettings.MamDriverIdentifier;

            var result = _service.GetStatus(login);

            return result;
        }
        private SmsServiceReference.SmsWebServiceSoapClient CreateSmsService(SmsSettingsPart smsSettings) {
            //Specify the binding to be used for the client.
            EndpointAddress address = new EndpointAddress(smsSettings.SmsServiceEndPoint);
            BasicHttpBinding binding = new BasicHttpBinding();
            if (address.Uri.Scheme.ToLower().StartsWith("https")) {
                binding.Security.Mode = BasicHttpSecurityMode.Transport;
            }

            return new SmsWebServiceSoapClient(binding, address);
        }
	}}