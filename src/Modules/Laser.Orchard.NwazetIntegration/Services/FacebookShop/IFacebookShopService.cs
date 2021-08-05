using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    public interface IFacebookShopService : IDependency {
        bool CheckBusiness(FacebookShopServiceContext context);
        bool CheckCatalog(FacebookShopServiceContext context);
        string GenerateAccessToken(FacebookShopServiceContext context);
        FacebookServiceJsonContext SyncProduct(ContentItem product);
    }
}