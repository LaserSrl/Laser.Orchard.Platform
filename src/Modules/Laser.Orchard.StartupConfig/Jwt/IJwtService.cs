using Orchard;

namespace Laser.Orchard.StartupConfig.Jwt
{
    public interface IJwtService : ISingletonDependency
    {
        void JwtLogin();
        void JwtTokenRenew();
        string GetBaseUrl();
        string GetJwtToken();
    }
}