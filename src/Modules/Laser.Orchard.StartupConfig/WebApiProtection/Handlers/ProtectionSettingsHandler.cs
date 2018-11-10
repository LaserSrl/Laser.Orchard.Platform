using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Handlers {

    [OrchardFeature("Laser.Orchard.StartupConfig.WebApiProtection")]
    public class ProtectionSettingsHandler : ContentHandler {
        private readonly IApiKeySettingService _apiKeySettingService;
        public ProtectionSettingsHandler(IApiKeySettingService apiKeySettingService) {
            _apiKeySettingService = apiKeySettingService;
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<ProtectionSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<ProtectionSettingsPart>("ProtectionSettings_Edit", "Parts/ProtectionSettingsPart.Edit", T("WebApi").Text));
            OnUpdated<ProtectionSettingsPart>((context, part) => {
                part.ExternalApplicationList = new ExternalApplicationList {
                    ExternalApplications = part.ExternalApplicationList.ExternalApplications.Where(w => !w.Delete),
                };
                _apiKeySettingService.Refresh();
            });

        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("WebApi")));
        }


    }
}