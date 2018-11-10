using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Handlers {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceLoginSettingsPartHandler : ContentHandler {

        public NonceLoginSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<NonceLoginSettingsPart>("Site"));          
        }
    }
}