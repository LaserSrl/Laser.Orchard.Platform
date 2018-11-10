using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.Fidelity.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.Fidelity.Services
{
    public interface IFidelityService : IDependency
    {
        APIResult CreateLoyalzooAccountFromCookie();

        APIResult CreateLoyalzooAccountFromContext(CreateContentContext context);

        APIResult CreateLoyalzooAccount(LoyalzooUserPart loyalzooPart, string username, string email);

        APIResult GetCustomerDetails();

        APIResult GetPlaceData();

        APIResult AddPoints(int numPoints);

        APIResult AddPointsFromAction(string actionId, string completionPercent);

        APIResult GiveReward(string rewardId);

        APIResult UpdateSocial(string token, string tokenType);

        MerchantApiData GetMerchantApiData();
    }
}