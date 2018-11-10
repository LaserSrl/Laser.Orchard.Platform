using Laser.Orchard.GDPR.Helpers;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.GDPR.Handlers {
    public class UserPartGDPRHandler : ContentGDPRHandler {

        public UserPartGDPRHandler() {

            // Early in the process we disbale the users, to make sure that other handlers
            // don't try to to something with them
            OnAnonymizing<UserPart>(DisableUser);
            OnErasing<UserPart>(DisableUser);

            // After everything else has been done for a User, we may have to clear their
            // userName and email address. We are doing it in the On-ed filters, so it happens
            // after most other handlers (the ones for -ing)
            OnAnonymized<UserPart>(ClearUser);
            OnErased<UserPart>(ClearUser);
        }

        private void DisableUser(GDPRContentContext context, UserPart userPart) {
            // for all versions, make sure the user is amrked as disabled
            var partVersions = PartVersions(context);
            foreach (var uPart in partVersions) {
                // disable the user.
                uPart.RegistrationStatus = UserStatus.Pending;
                uPart.EmailStatus = UserStatus.Pending;
            }
        }

        private void ClearUser(GDPRContentContext context, UserPart userPart) {
            // for all versions, clear username and email address. We make them into unique 
            // strings.
            var partVersions = PartVersions(context);
            foreach (var uPart in partVersions) {
                // UserName
                uPart.UserName = uPart.UserName.GenerateUniqueString();
                uPart.NormalizedUserName = uPart.UserName.ToLowerInvariant();
                // Email
                uPart.Email = uPart.Email.GenerateUniqueString();

                // then disable the user (again, just in case).
                uPart.RegistrationStatus = UserStatus.Pending;
                uPart.EmailStatus = UserStatus.Pending;
            }

        }

        private IEnumerable<UserPart> PartVersions(GDPRContentContext context) {
            return context.AllVersions
                .Select(civ => civ
                    .Parts
                    .FirstOrDefault(pa => pa is UserPart))
                .Where(pa => pa != null)
                .Cast<UserPart>();
        }
    }
}