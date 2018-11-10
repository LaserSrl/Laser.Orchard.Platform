using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.Fidelity.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;

namespace Laser.Orchard.Fidelity.Menus
{
    public class LoyalzooBackEndMenu : INavigationProvider
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IFidelityService _fidelityService;

        public string MenuName { get { return "admin"; } }
        public Localizer T { get; set; }

        public LoyalzooBackEndMenu(IOrchardServices orchardServices, IFidelityService fidelityService)
        {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
            _fidelityService = fidelityService;
        }

        public void GetNavigation(NavigationBuilder builder)
        {
            try
            {
                if (_orchardServices.Authorizer.Authorize(Permissions.LoyalzooAccessPermission.AccessBackEnd))
                {
                    var fidelitySettings = _orchardServices.WorkContext.CurrentSite.As<FidelitySiteSettingsPart>();

                    string merchantSessionId = fidelitySettings.MerchantSessionId;

                    if (string.IsNullOrWhiteSpace(merchantSessionId))
                    {
                        merchantSessionId = _fidelityService.GetMerchantApiData().MerchantId;
                    }

                    if (!string.IsNullOrWhiteSpace(merchantSessionId))
                    {
                        builder
                            //.AddImageSet("Laser.Orchard.Fidelity")
                            .Add(item => item
                                .Caption(T("Fidelity"))
                                .Position("2")
                                .Action("", "", new { area = "Laser.Orchard.Fidelity" })
                                .Url("https://myaccount.loyalzoo.com/login.php?session_id=" + merchantSessionId)
                            );
                    }
                }
            }
            catch
            { }
        }
    }
}