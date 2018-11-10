using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Users.Models;



namespace Laser.Orchard.UserReactions.Models {
    public class UserReactionsClickRecord {

        public virtual int Id { get; set; }
        public virtual UserPartRecord UserPartRecord { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        public virtual int ContentItemRecordId { get; set; }
        public virtual UserReactionsTypesRecord UserReactionsTypesRecord { get; set; }
        public virtual int ActionType { get; set; }
        public virtual string UserGuid { get; set; }

    }
}