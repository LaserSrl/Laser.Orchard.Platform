using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.Fidelity.Services;
using Laser.Orchard.Fidelity.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.Fidelity.Handlers
{
    public class LoyalzooUserHandler : ContentHandler
    {
        private readonly IFidelityService _fidelityService;
        private readonly IControllerContextAccessor _controllerContextAccessor;

        public LoyalzooUserHandler(IFidelityService fidelityService, IControllerContextAccessor controllerContextAccessor)
        {
            _fidelityService = fidelityService;
            _controllerContextAccessor = controllerContextAccessor;

            OnCreated<LoyalzooUserPart>((context, part) => AssociateLoyalzooAccount(context));
        }

        protected void AssociateLoyalzooAccount(CreateContentContext context)
        {
            //Non eseguo la creazione di un nuovo utente Loyalzoo se chiamo dal pannello di amministrazione di Orchard
            if (_controllerContextAccessor.Context.Controller.GetType().FullName != "Orchard.Users.Controllers.AdminController")
            {
                APIResult accountCreationResult = _fidelityService.CreateLoyalzooAccountFromContext(context);

                if (accountCreationResult.success == true)
                    _controllerContextAccessor.Context.Controller.TempData.Add("LoyalzooRegistrationSuccess", true);
                else
                    _controllerContextAccessor.Context.Controller.TempData.Add("LoyalzooRegistrationSuccess", false);
            }
        }
    }
}