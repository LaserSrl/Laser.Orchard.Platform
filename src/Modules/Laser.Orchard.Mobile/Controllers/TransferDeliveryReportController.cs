using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace Laser.Orchard.Mobile.Controllers {
    public class TransferDeliveryReportController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IRepository<CommunicationDeliveryReportRecord> _deliveryReportRepository;

        public TransferDeliveryReportController(IContentManager contentManager, IRepository<CommunicationDeliveryReportRecord> deliveryReportRepository) {
            Log = NullLogger.Instance;
            _contentManager = contentManager;
            _deliveryReportRepository = deliveryReportRepository;
        }


        public ILogger Log { get; set; }

        //
        // GET: /TransferDeliveryReport/
        public ContentResult Update() {
            string strResp = "OK";

            Log.Error("Contenuto richiesta: {0}", "InizioErr");

            try {
                Log.Error("InputStream: {0}", Request.InputStream.ToString());

                if (Request.InputStream != null && Request.InputStream.ToString().CompareTo("") != 0) {

                    // Read XML posted via HTTP
                    System.IO.StreamReader reader = new System.IO.StreamReader(Request.InputStream);

                    String xmlData = reader.ReadToEnd();
                    reader.Close();

                    Log.Error("Contenuto richiesta: {0}", xmlData);

                    if (xmlData.CompareTo("") != 0) {
                        Encoding usedEncoding = Encoding.UTF8;

                        // Deserializza oggetto postato
                        Log.Error("deserializzo il messaggio");
                        XmlSerializer serializer = new XmlSerializer(typeof(DeliveryReportVM));

                        MemoryStream ms = new MemoryStream(usedEncoding.GetBytes(xmlData));
                        object objReport = serializer.Deserialize(ms);

                        Log.Error("messaggio deserializzato");

                        DeliveryReportVM report = (DeliveryReportVM)objReport;
                        
                        Log.Error("Driver Id: " + report.DriverId);
                        Log.Error("Id Sms: " + report.MessageId + " Identifier: " + report.MessageIdentifier);
                        Log.Error("Testo Sms: " + report.TestoSms);
                        Log.Error("Stato Sms: " + report.Stato);

                        if (!report.MessageIdentifier.StartsWith("Orchard_")) {
                            return Content("OK");
                        }

                        var messageId = 0;

                        if (int.TryParse(report.MessageIdentifier.Split('_')[1], out messageId)) {
                            var smsPart = _contentManager.Get<SmsGatewayPart>(messageId);

                            if (report.Stato == "DELIVERED" || report.Stato == "ACCEPTED") {
                                smsPart.SmsDeliveredOrAcceptedNumber += 1;
                            } else {
                                smsPart.SmsRejectedOrExpiredNumber += 1;
                            }

                            CommunicationDeliveryReportRecord deliveryReport = new CommunicationDeliveryReportRecord();
                            deliveryReport.CommunicationAdvertisingPartRecord_Id = messageId;
                            deliveryReport.ExternalId = smsPart.ExternalId;
                            deliveryReport.RequestDate = Convert.ToDateTime(report.RequestDate, new CultureInfo("it-IT"));
                            deliveryReport.SubmittedDate = Convert.ToDateTime(report.SubMittedDate, new CultureInfo("it-IT"));
                            deliveryReport.Status = report.Stato;
                            deliveryReport.Recipient = report.To;
                            deliveryReport.Context = report.DriverId;
                            deliveryReport.Medium = "SMS";
                            
                            _deliveryReportRepository.Create(deliveryReport);
                            _deliveryReportRepository.Flush();

                        } else {
                            strResp = "KO";
                        }
                    }
                }
            } catch (Exception ex) {
                strResp = "KO";
                Log.Error("TransferDeliveryReportController Update: " + ex);
            }

            return Content(strResp);
        }
    }
}