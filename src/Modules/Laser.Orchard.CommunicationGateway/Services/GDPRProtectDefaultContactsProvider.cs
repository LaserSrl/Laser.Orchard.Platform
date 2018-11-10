using Laser.Orchard.GDPR.Services;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.GDPR.Models;
using Orchard.ContentManagement;
using Laser.Orchard.CommunicationGateway.Models;
using Orchard.Users.Models;
using Orchard;
using Orchard.Security;

namespace Laser.Orchard.CommunicationGateway.Services {
    [OrchardFeature("Laser.Orchard.GDPR.ContactExtension")]
    public class GDPRProtectDefaultContactsProvider : IGDPRProcessAllowedProvider {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public GDPRProtectDefaultContactsProvider(
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor,
            IAuthorizationService authorizationService) {

            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _authorizationService = authorizationService;
        }

        public bool ProcessIsAllowed(ContentItem contentItem) {
            // The Master contact is protected
            var contactPart = contentItem.As<CommunicationContactPart>();

            if (contactPart == null) {
                // we only have checks to do on CommunicationContactParts
                return true;
            }

            if (contactPart.Master) {
                // cannot anonymize the Master contact, because it should only have
                // stuff for anonymous users anyway.
                return false;
            }

            if (contactPart.UserIdentifier == 0) {
                // no way to make any judgement, so assume that whoever started the process
                // knwos what they are doing
                return true;
            }
            
            var user = _contentManager.Get<UserPart>(contactPart.UserIdentifier);
            if (user == null) {
                // no way to make any judgement, so assume that whoever started the process
                // knwos what they are doing
                return true;
            }

            if (!string.IsNullOrEmpty(_workContextAccessor.GetContext().CurrentSite.SuperUser) &&
                string.Equals(user.UserName, _workContextAccessor.GetContext().CurrentSite.SuperUser, StringComparison.Ordinal)) {
                // protect super user
                return false;
            }

            if (_authorizationService.TryCheckAccess(StandardPermissions.SiteOwner, user, null)) {
                // Prevent processing if the contact is for an admin
                return false;
            }

            // by default we return true, to avoid blocking when we are not sure we should
            return true;
        }

        public bool ProcessIsAllowed(GDPRPart part) {
            return part != null
                && ProcessIsAllowed(part.ContentItem);
        }

        public bool ProcessIsAllowed(GDPRContentContext context) {
            return context != null
                && ProcessIsAllowed(context.ContentItem);
        }
    }
}