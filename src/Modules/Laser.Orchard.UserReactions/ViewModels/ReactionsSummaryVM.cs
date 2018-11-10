using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.ViewModels {
    public class ReactionsSummaryVM {
        public bool UserAuthenticated { get; set; }
        public bool UserAuthorized { get; set; }
        public int ContentId { get; set; }
        public UserReactionsVM[] Reactions { get; set; }

        public ReactionsSummaryVM() {
            Reactions = new UserReactionsVM[0];
        }
    }
}