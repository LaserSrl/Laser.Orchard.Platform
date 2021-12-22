using Laser.Orchard.StartupConfig.AuditTrail.Models;
using Laser.Orchard.StartupConfig.AuditTrail.ViewModels;
using Orchard.AuditTrail;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.AuditTrail.Drivers {
    [OrchardFeature("Laser.Orchard.AuditTrail")]
    public class AuditTrailOutputSettingsPartDriver
        : ContentPartDriver<AuditTrailOutputSettingsPart> {

        private readonly IAuthorizer _authorizer;

        public AuditTrailOutputSettingsPartDriver(
            IAuthorizer authorizer) {

            _authorizer = authorizer;
        }

        protected override DriverResult Editor(
            AuditTrailOutputSettingsPart part, dynamic shapeHelper) {

            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(
            AuditTrailOutputSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (!_authorizer.Authorize(Permissions.ManageAuditTrailSettings))
                return null;

            return ContentShape("Parts_AuditTrailOutputSettings_Edit", () => {
                var viewModel = new AuditTrailOutputSettingsPartViewModel {
                    IsEventViewerEnabled = part.IsEventViewerEnabled,
                    EventViewerSourceName = part.EventViewerSourceName,
                    IsFileSystemEnabled = part.IsFileSystemEnabled
                };
                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        part.IsEventViewerEnabled = viewModel.IsEventViewerEnabled;
                        part.EventViewerSourceName = viewModel.EventViewerSourceName;
                        part.IsFileSystemEnabled = viewModel.IsFileSystemEnabled;
                    }
                }
                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/AuditTrailOutputSettings", 
                    Model: viewModel, 
                    Prefix: Prefix);
            }).OnGroup("Audit Trail");
        }
    }
}