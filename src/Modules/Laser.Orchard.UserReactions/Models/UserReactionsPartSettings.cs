using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Models {
    public class UserReactionsPartSettings 
    {
        public bool Filtering { get; set; }
        public List<UserReactionsSettingTypesSel> TypeReactionsPartsSelected { get; set; }
        public UserChoiceBehaviourValues UserChoiceBehaviour { get; set; } 
    }
}