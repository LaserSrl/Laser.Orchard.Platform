using Laser.Orchard.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.ViewModels {

    public class SmsGatewayVM {
        public SmsServiceReference.enumProtocollo Protocollo { get; set; }
        public List<string> AliasList { get; set; }
        public int MaxLenghtSms { get; set; }
        public SmsGatewayPart SmsGateway { get; set; }
        public SmsPlaceholdersSettingsPart Settings { get; set; }
        public bool ShortlinkExist { get; set; }
    }

}