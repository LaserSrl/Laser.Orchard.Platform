using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.OpenAuthentication.Handlers {
    /// <summary>
    /// This handler should take care of cleaning all information from UserProviderRecords
    /// related to the user managed by the Open Authenticatio providers.
    /// </summary>
    [OrchardFeature("Laser.Orchard.GDPR.OpenAuthExtension")]
    public class UserProviderGDPRHandler : ContentGDPRHandler {

        private readonly IRepository<UserProviderRecord> _userProviderRepository;

        public UserProviderGDPRHandler(
            IRepository<UserProviderRecord> userProviderRepository) {

            _userProviderRepository = userProviderRepository;

            // OpenAuth uses an UserProvidersPart attached to a User to "contain" all the providers'
            // information for the user.
            OnAnonymizing<UserProvidersPart>(HandleProviders);
            OnErasing<UserProvidersPart>(HandleProviders);
        }

        private void HandleProviders(GDPRContentContext context, UserProvidersPart userProvidersPart) {
            // fetch the records directly, because the part does not contain them (rather, it contains
            // some "derived" objects)
            var providerRecords = _userProviderRepository
                .Fetch(x => x.UserId == context.ContentItem.Id);
            foreach (var record in providerRecords) {
                // ProviderUserId is the identifier for the user in the provider
                record.ProviderUserId = string.Empty;
                // ProviderUserData contains any additional information related to the user. This usually
                // is personal identifiable information.
                record.ProviderUserData = string.Empty;
            }
        }
    }
}