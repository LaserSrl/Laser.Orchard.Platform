using System;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.OpenId;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.ViewModels;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class CustomOpenIdAuthenticationClient {
        private readonly string _providerName;

        public CustomOpenIdAuthenticationClient(string providerName) {
            _providerName = providerName;
        }

        public IAuthenticationClient Build(ProviderConfigurationViewModel providerConfiguration) {
            if (string.IsNullOrWhiteSpace(providerConfiguration.ProviderIdentifier))
                throw new Exception("Client Identifier must be known if Provider is unknown.");

            return new OpenIdClient(_providerName, Identifier.Parse(providerConfiguration.ProviderIdentifier));
        }
    }
}