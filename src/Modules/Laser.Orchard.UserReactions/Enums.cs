using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard;
using Orchard.Data;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.UserReactions.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.UserReactions {
    public enum StyleFileNameProviders { Reactions };
    public enum ReactionsNames {
        angry,
        boring,
        exhausted,
        happy,
        joke,
        kiss,
        love,
        pain,
        sad,
        shocked,
        silent,
        like,
        iwasthere,
        excited,
        curious,
        interested
    }; // Se si aggiungono enumeratori, provvedere opportuna traduzione in UserReactionsService.GetReactionEnumTranslations

      
    public enum UserChoiceBehaviourValues {
        Inherit,
        AllowMultiple,
        RestrictToSingle
    }


    public enum UserReactionsFieldOperator {
        LessThan,
        LessThanEquals,
        Equals,
        NotEquals,
        GreaterThanEquals,
        GreaterThan,
        Between,
        NotBetween
    }
    public class StyleAcroName {
        public string StyleAcronime = "glyph-icon icon-";

    }

}
