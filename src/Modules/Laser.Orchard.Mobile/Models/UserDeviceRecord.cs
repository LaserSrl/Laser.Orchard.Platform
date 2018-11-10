using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Models {
    public class UserDeviceRecord {
        public virtual int Id { get; set; }
        public virtual UserPartRecord UserPartRecord { get; set; }
        public virtual string UUIdentifier { get; set; }
    }
}