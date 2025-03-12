using System;

namespace Laser.Orchard.Mobile.Models {
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
