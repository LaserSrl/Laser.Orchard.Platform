using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard.Environment.Extensions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Laser.Orchard.Mobile.Drivers {

    [OrchardFeature("Laser.Orchard.Sms")]
    public class SmsSettingsPartDriver : ContentPartDriver<SmsSettingsPart> {

        private readonly ISmsServices _smsServices;

        public SmsSettingsPartDriver(ISmsServices smsServices) {
            _smsServices = smsServices;
        }

        protected override string Prefix {
            get { return "Laser.Mobile.SmsSettings"; }
        }

        // GET
        protected override DriverResult Editor(SmsSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        // POST
        protected override DriverResult Editor(SmsSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            const int MSG_MAX_CHAR_NUMBER_SINGOLO = 160;
            const int MSG_MAX_CHAR_NUMBER_CONCATENATI = 1530;

            return ContentShape("SmsSettingsPart_Edit", () => {

                if (updater != null) {

                    if (updater.TryUpdateModel(part, Prefix, null, null)) {
                        SmsServiceReference.Config SmsConfig = _smsServices.GetConfig();

                        if (SmsConfig.ListaAlias != null) {
                            part.MamHaveAlias = (SmsConfig.ListaAlias.Count > 0);
                        } else {
                            part.MamHaveAlias = false;
                        }

                        part.SmsFrom = SmsConfig.Multialias;
                        part.Protocollo = SmsConfig.Protocollo;

                        int MaxLenght = MSG_MAX_CHAR_NUMBER_SINGOLO;
                        if (SmsConfig.MaxLenghtSms > 1) {
                            MaxLenght = MSG_MAX_CHAR_NUMBER_CONCATENATI;
                        }

                        part.MaxLenghtSms = MaxLenght;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts/SmsSettingsPart_Edit", Model: part, Prefix: Prefix);
            })
            .OnGroup("SMS");
        }

    }
}