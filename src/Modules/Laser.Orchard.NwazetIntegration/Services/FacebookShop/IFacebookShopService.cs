using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    public interface IFacebookShopService : IDependency {
        bool CheckBusiness(FacebookShopServiceContext context);
        bool CheckCatalog(FacebookShopServiceContext context);
        FacebookShopRequestContainer SyncProduct(ContentItem product);
        FacebookShopRequestContainer SyncProduct(FacebookShopProductUpdateRequest context);
        FacebookShopRequestContainer RemoveProduct(ContentItem product);
        void SyncProducts();
        void ScheduleProductSynchronization();
    }
}