using Orchard;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IApiKeyService : IDependency {
        string ValidateRequestByApiKey(string additionalCacheKey, bool protectAlways = false);
        string GetValidApiKey(string sIV, bool useTimeStamp=false);
    }
}