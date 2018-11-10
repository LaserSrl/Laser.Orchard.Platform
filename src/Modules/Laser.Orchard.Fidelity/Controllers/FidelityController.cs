using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.Fidelity.Services;
using Laser.Orchard.Fidelity.ViewModels;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.Fidelity.Controllers
{
    public class FidelityController : Controller
    {
        private readonly IFidelityService _fidelityService;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public FidelityController(IFidelityService fidelityService, IContentManager contentManager, INotifier notifier)
        {
            _fidelityService = fidelityService;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public bool CreateLoyalzooAccountFromPart(int partId)
        {
            try
            {
                ContentItem part = _contentManager.Get(partId);

                IUser userPart = part.As<IUser>();
                LoyalzooUserPart loyalzooPart = part.As<LoyalzooUserPart>();

                if (userPart != null && loyalzooPart != null)
                {
                    APIResult createResult = _fidelityService.CreateLoyalzooAccount(loyalzooPart, userPart.UserName, userPart.Email);

                    if (createResult.success)
                        return true;
                    else
                    {
                        _notifier.Error(T("Registration failed: {0}", createResult.message));
                    }
                }
                else
                     _notifier.Error(T("The provided part cannot be used for registration."));

                return false;
            }
            catch (Exception e)
            {
                _notifier.Error(T("Unable to register: {0}", e.Message));
                return false;
            }
        }
    }
}