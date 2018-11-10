using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class ContactInfoPartDriver : ContentPartDriver<ContactInfoPart> {
        protected override DriverResult Display(ContactInfoPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_ContactInfo", () => shapeHelper.Parts_ContactInfo());
        }
    }
}