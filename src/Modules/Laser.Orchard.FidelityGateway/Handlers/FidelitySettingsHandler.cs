﻿using Laser.Orchard.FidelityGateway.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Handlers
{
    public class FidelitySettingsHandler : ContentHandler {

        public FidelitySettingsHandler()
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            Filters.Add(new ActivatingFilter<FidelitySettingsPart>("Site"));     
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("FidelityGateway")));
        }

    }
}