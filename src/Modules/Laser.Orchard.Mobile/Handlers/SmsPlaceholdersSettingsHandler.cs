using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.Mobile.Handlers {

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsPlaceholdersSettingsHandler : ContentHandler {
        public SmsPlaceholdersSettingsHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<SmsPlaceholdersSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<SmsPlaceholdersSettingsPart>("SmsPlaceholdersSettings_Edit", "Parts/SmsPlaceholders.Settings", "SMS"));
            OnUpdated<SmsPlaceholdersSettingsPart>((context, part) => {
                part.PlaceholdersList = new SmsPleaceholdersList {
                    Placeholders = part.PlaceholdersList.Placeholders.Where(w => !w.Delete),
                };
            });

        }

        public Localizer T { get; set; }


    }
}