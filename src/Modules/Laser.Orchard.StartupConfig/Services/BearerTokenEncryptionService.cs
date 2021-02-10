using Laser.Orchard.StartupConfig.Helpers;
using Laser.Orchard.StartupConfig.Security;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenEncryptionService : IBearerTokenEncryptionService {
        // in this implementation we are going to cheat and user the same
        // encryption that is used for FomrsAuthentication
        private readonly IClock _clock;
        private readonly IEnumerable<IBearerTokenDataProvider> _bearerTokenDataProviders;

        public BearerTokenEncryptionService(
            IClock clock,
            IEnumerable<IBearerTokenDataProvider> bearerTokenDataProviders) {

            _clock = clock;
            _bearerTokenDataProviders = bearerTokenDataProviders;

            ExpirationTimeSpan = TimeSpan.FromMinutes(30);
        }

        public TimeSpan ExpirationTimeSpan {
            get; set;
            // The public setter allows injecting this from Sites.MyTenant.Config or Sites.config, by using
            // an AutoFac component:
            /*
             <component instance-scope="per-lifetime-scope"
                type="Laser.Orchard.StartupConfig.Services.BearerTokenEncryptionService, Laser.Orchard.StartupConfig"
                service="Laser.Orchard.StartupConfig.Services.IBearerTokenEncryptionService">
                <properties>
                    <property name="ExpirationTimeSpan" value="00:30:00" />
                </properties>
            </component>

             */
        }

        public string CreateNewTokenForUser(IUser user) {
            var ticket = CreateNewTicket(user);

            if (ticket == null) {
                return null;
            }

            return CreateTokenFromTicket(ticket);
        }

        public BearerTokenAuthenticationTicket CreateNewTicket(IUser user) {
            if (user == null || string.IsNullOrWhiteSpace(user.UserName)) {
                return null;
            }
            var now = _clock.UtcNow.ToLocalTime();
            var userData = ComputeUserData(user);

            return new BearerTokenAuthenticationTicket(
                1, user.UserName,
                now.Add(ExpirationTimeSpan), now,
                userData);
        }

        public string CreateTokenFromTicket(BearerTokenAuthenticationTicket ticket) {

            var fTicket = new FormsAuthenticationTicket(
                ticket.Version,
                ticket.Name,
                ticket.IssueDate,
                ticket.Expiration,
                false,// bearer token can't be persistent
                ticket.UserData
                );

            var baseToken = FormsAuthentication.Encrypt(fTicket);
            // bearer tokens are base64 strings
            var token = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(baseToken),
                    Base64FormattingOptions.None);
            return token;
        }

        public BearerTokenAuthenticationTicket ParseToken(string token) {
            if (!string.IsNullOrWhiteSpace(token)) {
                try {
                    var baseToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                    var fTicket = FormsAuthentication.Decrypt(baseToken);
                    if (fTicket == null) {
                        return null;
                    }
                    return new BearerTokenAuthenticationTicket(
                        fTicket.Version, fTicket.Name,
                        fTicket.Expiration, fTicket.IssueDate,
                        fTicket.UserData
                        );
                } catch (Exception) {
                }
            }

            return null;
        }

        private string ComputeUserData(IUser user) {
            // serialize dictionary to userData string
            return BearerTokenHelpers.SerializeUserDataDictionary(ComputeUserDataDictionary(user));
        }
        private Dictionary<string, string> ComputeUserDataDictionary(IUser user) {
            var userDataDictionary = new Dictionary<string, string>();
            // we are already adding UserName, so we don't need a provider for it
            userDataDictionary.Add("UserName", user.UserName);
            foreach (var userDataProvider in _bearerTokenDataProviders) {
                var key = userDataProvider.Key;
                var value = userDataProvider.ComputeUserDataElement(user);
                // TODO: how should duplicate keys be handled? As is, this would throw.
                if (key != null && value != null) {
                    userDataDictionary.Add(key, value);
                }
            }
            return userDataDictionary;
        }
    }
}