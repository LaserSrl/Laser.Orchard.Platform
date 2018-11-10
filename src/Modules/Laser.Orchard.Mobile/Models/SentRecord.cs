using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Models {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class SentRecord {
        public virtual int Id { get; set; }
        public virtual int PushNotificationRecord_Id { get; set; }
        public virtual int PushedItem { get; set; }
        public virtual DateTime SentDate { get; set; }
        public virtual string DeviceType { get; set; }
        public virtual string Outcome { get; set; }
        public virtual bool Repeatable { get; set; }
        public SentRecord() {
            SentDate = DateTime.UtcNow;
        }
    }
}
