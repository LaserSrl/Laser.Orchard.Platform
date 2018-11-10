using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.ViewModels {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushMessage {
        public string Title { get; set; }
        public string Text { get; set; }
        public int idRelated { get; set; }
        public int idContent { get; set; }
        public string Sound { get; set; }
        public bool ValidPayload { get; set; }
        public string Ct { get; set; }
        public string Al { get; set; }
        public string Eu { get; set; }
    }
}