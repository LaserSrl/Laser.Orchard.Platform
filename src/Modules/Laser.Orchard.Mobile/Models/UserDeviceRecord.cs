using Orchard.Users.Models;

namespace Laser.Orchard.Mobile.Models {
    public class UserDeviceRecord {
        public virtual int Id { get; set; }
        public virtual UserPartRecord UserPartRecord { get; set; }
        public virtual string UUIdentifier { get; set; }
    }
}