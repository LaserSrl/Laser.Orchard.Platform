using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.Mobile.Models;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Laser.Orchard.Mobile.Handlers {
    [OrchardFeature("Laser.Orchard.GDPR.SmsExtension")]
    public class UserPwdRecoveryPartGDPRHandler : ContentGDPRHandler {

        public UserPwdRecoveryPartGDPRHandler() {

            OnAnonymizing<UserPwdRecoveryPart>(HandlerPart);
            OnErasing<UserPwdRecoveryPart>(HandlerPart);
        }

        private void HandlerPart(GDPRContentContext context, UserPwdRecoveryPart part) {
            // clear the phone number from all versions
            var partVersions = context.AllVersions
                .Select(civ => civ
                    .Parts
                    .FirstOrDefault(pa => pa is UserPwdRecoveryPart))
                .Where(pa => pa != null)
                .Cast<UserPwdRecoveryPart>();
            foreach (var pv in partVersions) {
                pv.PhoneNumber = string.Empty;
            }
        }
    }
}
