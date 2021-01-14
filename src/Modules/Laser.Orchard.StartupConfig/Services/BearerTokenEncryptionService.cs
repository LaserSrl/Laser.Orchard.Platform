using Laser.Orchard.StartupConfig.Security;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Services;
using System;
using System.Text;
using System.Web.Security;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class BearerTokenEncryptionService : IBearerTokenEncryptionService {
        // in this implementation we are going to cheat and user the same
        // encryption that is used for FomrsAuthentication
        private readonly IClock _clock;
        private readonly ShellSettings _shellSettings;

        public BearerTokenEncryptionService(
            IClock clock,
            ShellSettings shellSettings) {

            _clock = clock;
            _shellSettings = shellSettings;

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

        public string CreateTokenFromTicket(BearerTokenAuthenticationTicket ticket) {
            var now = _clock.UtcNow.ToLocalTime();

            var fTicket = new FormsAuthenticationTicket(
                ticket.Version,
                ticket.Name,
                now,
                now.Add(ExpirationTimeSpan),
                false,// bearer token can't be persistent
                ticket.UserData
                );

            var baseToken = FormsAuthentication.Encrypt(fTicket);

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
        
    }
}