using Laser.Orchard.UserReactions.Models;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Handlers {
    public class UserReactionsSettingsPartHandler:  ContentHandler {

        public UserReactionsSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<UserReactionsSettingsPart>("Site"));
        }
    }
}