using Lasergroup.Models;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lasergroup.Handlers {
    public class AdditionalCssSettingsHandler : ContentHandler {

        public AdditionalCssSettingsHandler() {

            Filters.Add(new ActivatingFilter<AdditionalCssSettingsPart>("Site"));
        }
    }
}