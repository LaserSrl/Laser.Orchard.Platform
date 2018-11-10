using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Models {
    public class UserReactionsSettingTypesSel
    {
        public int Id { get; set; }
        public string nameReaction { get; set; }
        public bool checkReaction { get; set; }
    }
}