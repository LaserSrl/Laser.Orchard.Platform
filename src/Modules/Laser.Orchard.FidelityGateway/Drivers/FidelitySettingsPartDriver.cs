using Laser.Orchard.FidelityGateway.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Drivers
{
    public class FidelitySettingsPartDriver: ContentPartDriver<FidelitySettingsPart> {
        private const string TemplateName = "Parts/FidelitySettingsPart";

        public FidelitySettingsPartDriver()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "FidelitySettings"; } }

        protected override DriverResult Editor(FidelitySettingsPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(FidelitySettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_FidelitySettingsPart_Edit", () =>
            {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("FidelityGateway");
        }
    }
}