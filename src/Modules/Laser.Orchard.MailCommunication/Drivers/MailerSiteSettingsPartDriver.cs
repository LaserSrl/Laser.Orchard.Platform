using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.MailCommunication.ViewModels;
using Laser.Orchard.TemplateManagement.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Laser.Orchard.MailCommunication.Drivers
{
    [OrchardFeature("Laser.Orchard.MailerUtility")]
    public class MailerSiteSettingsPartDriver : ContentPartDriver<MailerSiteSettingsPart>
    {
        protected override string Prefix { get { return "MailerSettings"; } }

        // GET
        protected override DriverResult Editor(MailerSiteSettingsPart part, dynamic shapeHelper) {

            return Editor(part, null, shapeHelper);
        }

        // POST
        protected override DriverResult Editor(MailerSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_MailerSiteSettings_Edit", () => {

                var vModel = new MailerSiteSettingsVM { Settings = part };

                if (updater != null) {
                    updater.TryUpdateModel(vModel, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts/MailerSiteSettings", Model: vModel, Prefix: Prefix);
            })
                .OnGroup("Mailer");
        }
    }
}