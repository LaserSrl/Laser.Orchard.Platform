using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.GDPR.Handlers;
using Orchard.ContentManagement;
using Laser.Orchard.GDPR.Models;

namespace Laser.Orchard.GDPR.Services {
    public class GDPRPartProtectedProvider : IGDPRProcessAllowedProvider {

        public bool ProcessIsAllowed(GDPRContentContext context) {
            if (context.GDPRPart == null) {
                // do nothing if the item is not configured for GDPR processing
                return false;
            }
            // do nothing for protected items
            return !context.GDPRPart.IsProtected;
        }

        public bool ProcessIsAllowed(ContentItem contentItem) {
            // left side may be null, true or false
            return ProcessIsAllowed(contentItem?.As<GDPRPart>());
        }

        public bool ProcessIsAllowed(GDPRPart part) {

            return part != null && !part.IsProtected;
        }
    }
}