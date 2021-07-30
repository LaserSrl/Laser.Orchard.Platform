using Orchard;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    public interface IFacebookShopService : IDependency {
        bool CheckBusiness(FacebookShopServiceContext context);
        bool CheckCatalog(FacebookShopServiceContext context);
        string GenerateAccessToken(FacebookShopServiceContext context);
    }
}