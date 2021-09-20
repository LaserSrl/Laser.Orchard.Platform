using System.Collections.Generic;
using DotNetOpenAuth.AspNet;
using Orchard.Validation;

namespace Laser.Orchard.OpenAuthentication.Models {
    public class OrchardAuthenticationClientData {
        public OrchardAuthenticationClientData(
            IAuthenticationClient authenticationClient,
            string displayName,
            bool isWebLoginEnabled,
            IDictionary<string, object> extraData) {

            Argument.ThrowIfNull(authenticationClient, "authenticationClient");

            AuthenticationClient = authenticationClient;
            DisplayName = displayName;
            ExtraData = extraData;
            IsWebSiteLoginEnabled = isWebLoginEnabled;
        }

        public IAuthenticationClient AuthenticationClient { get; private set; }
        public string ProviderName { get { return AuthenticationClient.ProviderName; } }
        public string DisplayName { get; private set; }
        public bool IsWebSiteLoginEnabled { get; private set; }
        public IDictionary<string, object> ExtraData { get; private set; }
    }
}