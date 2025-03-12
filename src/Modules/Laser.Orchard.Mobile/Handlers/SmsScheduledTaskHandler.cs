using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Laser.Orchard.Mobile.Handlers {

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsScheduledTaskHandler : IScheduledTaskHandler {

        private readonly IOrchardServices _orchardServices;
        private readonly ISmsServices _smsServices;
        private readonly ISmsCommunicationService _smsCommunicationService;
        private const string TaskType = "Laser.Orchard.SmsGateway.Task";
        public ILogger Logger { get; set; }
        public SmsScheduledTaskHandler(IOrchardServices orchardServices, ISmsServices smsServices,
                                       ISmsCommunicationService smsCommunicationService) {
            _orchardServices = orchardServices;
            _smsServices = smsServices;
            _smsCommunicationService = smsCommunicationService;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType != TaskType) {
                return;
            }
            try {
                //dynamic content = _orchardServices.ContentManager.Get(part.ContentItem.Id);
                dynamic content = context.Task.ContentItem;
                SmsGatewayPart part = context.Task.ContentItem.As<SmsGatewayPart>();
                Int32[] ids = null;
                Int32? idLocalization = null;

                if (content.QueryPickerPart != null && content.QueryPickerPart.Ids.Length > 0) {
                    ids = content.QueryPickerPart.Ids;
                }

                var localizedPart = content.LocalizationPart;
                if (localizedPart != null && localizedPart.Culture != null) {
                    idLocalization = localizedPart.Culture.Id;
                }

                IList listaDestinatari = new List<Hashtable>();

                if (part.RecipientList != null && part.RecipientList != "") {

                    string[] lstDest = part.RecipientList.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    foreach (string tel in lstDest) {
                        Hashtable hs = new Hashtable();
                        hs.Add("SmsContactNumber", tel);

                        listaDestinatari.Add(hs);
                    }
                } 
                else {
                    //var listaNumeri = _smsCommunicationService.GetSmsNumbersQueryResult(ids, idLocalization);
                    listaDestinatari = _smsCommunicationService.GetSmsQueryResult(ids, idLocalization, false, context.Task.ContentItem);
                }

                if (listaDestinatari.Count > 0) {
                    string linktosend = "";
                    ICommunicationService _communicationService;

                    bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
                    if (tryed) {
                        if (_communicationService.CampaignLinkExist(part)) {
                            linktosend = _communicationService.GetCampaignLink("Sms", part);
                        }
                    }
                    string messageToSms = part.Message + " " + linktosend;

                    // Invio SMS
                    //_smsServices.SendSms(listaDestinatari.Select(x => Convert.ToInt64(x.SmsPrefix + x.SmsNumber)).ToArray(),
                    //                     messageToSms, part.Alias, "Orchard_" + part.Id.ToString(), part.HaveAlias);
                    _smsServices.SendSms(listaDestinatari, messageToSms, part.Alias, "Orchard_" + part.Id.ToString(), part.HaveAlias);

                    part.SmsRecipientsNumber = listaDestinatari.Count;
                    part.SmsMessageSent = true;
                }
            } catch (Exception ex) {
                string idcontenuto = "nessun id ";
                try {
                    idcontenuto = context.Task.ContentItem.Id.ToString();
                } catch (Exception ex2) {Logger.Error(ex2,ex2.Message); }
                Logger.Error(ex, "Error on " + TaskType + " for ContentItem id = " + idcontenuto + " : " + ex.Message);
            }
        }
    }
}