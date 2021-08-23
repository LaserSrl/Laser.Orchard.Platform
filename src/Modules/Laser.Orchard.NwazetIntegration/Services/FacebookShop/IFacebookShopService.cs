using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    public interface IFacebookShopService : IDependency {
        bool CheckBusiness(FacebookShopServiceContext context);
        bool CheckCatalog(FacebookShopServiceContext context);
        string GenerateAccessToken(FacebookShopServiceContext context);
        FacebookShopProductUpdateRequest SyncProduct(ContentItem product);
        FacebookShopProductUpdateRequest SyncProduct(FacebookShopProductUpdateRequest context);
        FacebookShopProductDeleteRequest RemoveProduct(ContentItem product);
        void SyncProducts();
    }
}