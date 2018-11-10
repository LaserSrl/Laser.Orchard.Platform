using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.MailCommunication.ViewModels;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.Drivers {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MailCommunicationSettingsPartDriver : ContentPartDriver<MailCommunicationSettingsPart> 
    {
        private readonly ITemplateService _templateService;

        public MailCommunicationSettingsPartDriver(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        protected override string Prefix {
            get { return "MailCommunicationSettings"; }
        }

        // GET
        protected override DriverResult Editor(MailCommunicationSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        // POST
        protected override DriverResult Editor(MailCommunicationSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            return ContentShape("Parts_MailCommunicationSettings_Edit", () => {

                var vModel = new MailCommunicationSettingsVM {
                    TemplatesUnsubscribeList = _templateService.GetTemplates().OrderBy(o=>o.Title),
                    Settings = part
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(part, Prefix, null, null) && updater.TryUpdateModel(vModel, Prefix, null, null)) {
                        part.IdTemplateUnsubscribe = vModel.Settings.IdTemplateUnsubscribe;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts/MailCommunicationSettings", Model: vModel, Prefix: Prefix);
            })
                .OnGroup("MailCommunication");
        }

    }
}