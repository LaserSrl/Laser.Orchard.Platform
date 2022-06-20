using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Security;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointPartDriver
        : ContentPartDriver<PickupPointPart> {
        private readonly IAuthorizer _authorizer;

        public PickupPointPartDriver(
            IAuthorizer authorizer) {

            _authorizer = authorizer;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "PickupPointPart"; }
        }


        protected override DriverResult Editor(PickupPointPart part, dynamic shapeHelper) {
            if (!Authorized(part)) {
                return null;
            }
            return ContentShape("Parts_PickupPointPart_Edit", () => {
                return shapeHelper.EditorTemplate(
                    TemplateName: "Parts/PickupPoint",
                    Model: CreateVM(part),
                    Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(PickupPointPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!Authorized(part)) {
                return null;
            }

            return null;
            //return base.Editor(part, updater, shapeHelper);
        }

        private bool Authorized(PickupPointPart part) {
            return _authorizer.Authorize(
                PickupPointPermissions.MayConfigurePickupPoints, 
                part, 
                T("Cannot manage pickup points"));
        }

        private PickupPointPartEditViewModel CreateVM(PickupPointPart part) {
            // TODO
            return null;
        }
    }
}