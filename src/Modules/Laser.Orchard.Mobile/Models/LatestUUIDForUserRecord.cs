using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Models {
    /// <summary>
    /// This class looks the same as UserDeviceRecord, however:
    /// - There is at most one record for each user, since we are only interested in
    ///     storing whichever the latest device used by the user is.
    /// - We could not use the UserDeviceRecord, because that is created and populated
    ///     on the user's first LoggedIn event (with the device), while this record has
    ///     to be created during the handling of the SignIn, that happens before the 
    ///     events for the user are fired. For this reason we cannot use a relation into
    ///     the UserDeviceRecord.
    /// </summary>
    public class LatestUUIDForUserRecord {
        public virtual int Id { get; set; } // Primary Key
        public virtual UserPartRecord UserPartRecord { get; set; }
        public virtual string UUID { get; set; }
    }
}