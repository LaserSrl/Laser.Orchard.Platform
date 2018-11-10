using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.GDPR.Models;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard;
using Orchard.Users.Models;

namespace Laser.Orchard.GDPR.Services {
    public class GDPRProtectAdminsProvider : IGDPRProcessAllowedProvider {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public GDPRProtectAdminsProvider(
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor,
            IAuthorizationService authorizationService) {

            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _authorizationService = authorizationService;
        }

        public bool ProcessIsAllowed(ContentItem contentItem) {
            // protect admin users
            var user = contentItem.As<UserPart>();
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