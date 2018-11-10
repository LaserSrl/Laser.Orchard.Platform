using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.UsersExtensions.Handlers {
    public class FavoriteCulturePartHandler : ContentHandler{
        public FavoriteCulturePartHandler() {
            Filters.Add(new ActivatingFilter<FavoriteCulturePart>("User"));
        }
    }
}