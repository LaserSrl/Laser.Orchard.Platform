using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Orchard;

namespace Laser.Orchard.StartupConfig.Jwt
{
    public interface IFakeJwtService : IJwtService
    {

    }
    public class FakeJwt : JwtServiceBase, IFakeJwtService
    {
        public FakeJwt(IOrchardServices orchardServices) : base(orchardServices)
        {

        }

        public override KeyValuePair<string, string> GetAuthHeader()
        {
            return new KeyValuePair<string, string>(null, "");
        }

        public override string GetBaseUrl()
        {
            return "http://www.laser-group.com";
        }

        public override void JwtLogin()
        {
            JwtToken = new JwtSecurityToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");
        }

        public override void JwtTokenRenew()
        {
            
        }
    }
}