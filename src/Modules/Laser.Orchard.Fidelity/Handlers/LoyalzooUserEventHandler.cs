using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.Fidelity.Services;
using Laser.Orchard.Fidelity.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Events;
using System;

namespace Laser.Orchard.Fidelity.Handlers
{
    public class LoyalzooUserEventHandler : IUserEventHandler
    {
        private readonly IFidelityService _fidelityService;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IOrchardServices _orchardServices;

        public LoyalzooUserEventHandler(IFidelityService fidelityService, IControllerContextAccessor controllerContextAccessor, IOrchardServices orchardServices)
        {
            _fidelityService = fidelityService;
            _controllerContextAccessor = controllerContextAccessor;
            _orchardServices = orchardServices;
        }

        public void LoggedIn(IUser user)
        {
            try
            {
                var fidelitySettings = _orchardServices.WorkContext.CurrentSite.As<FidelitySiteSettingsPart>();

                if (!string.IsNullOrWhiteSpace(fidelitySettings.DeveloperKey) && // Verifico che l'utente abbia già compilato le impostazioni della fidelity
                    (fidelitySettings.RegisterOnLogin == LoyalzooRegistrationEnum.Always ||
                     (fidelitySettings.RegisterOnLogin == LoyalzooRegistrationEnum.External && _controllerContextAccessor.Context.Controller.GetType().FullName != "Orchard.Users.Controllers.AccountController")))
                {
                    LoyalzooUserPart loyalzooUserData = user.As<LoyalzooUserPart>();

                    if (string.IsNullOrWhiteSpace(loyalzooUserData.LoyalzooUsername) && string.IsNullOrWhiteSpace(loyalzooUserData.LoyalzooPassword) && string.IsNullOrWhiteSpace(loyalzooUserData.CustomerSessionId))
                    {
                        APIResult accountCreationResult = _fidelityService.CreateLoyalzooAccountFromCookie();

                        if (accountCreationResult.success == true)
                            _controllerContextAccessor.Context.Controller.TempData.Add("LoyalzooRegistrationSuccess", true);
                        else
                            _controllerContextAccessor.Context.Controller.TempData.Add("LoyalzooRegistrationSuccess", false);
                    }
                }
            }
            catch
            { }
        }

        #region Unused events
        public void Approved(IUser user)
        {
        }

        public void Moderate(IUser user)
        {
        }

        public void Created(UserContext context)
        {
        }

        public void Creating(UserContext context)
        {
        }

        public void LoggedOut(IUser user)
        {
        }

        public void AccessDenied(IUser user)
        {
        }

        public void ChangedPassword(IUser user)
        {
        }

        public void SentChallengeEmail(IUser user)
        {
        }

        public void ConfirmedEmail(IUser user)
        {
        }

        public void LoggingIn(string userNameOrEmail, string password) {
        }

        public void LogInFailed(string userNameOrEmail, string password) {
        }
        #endregion
    }
}