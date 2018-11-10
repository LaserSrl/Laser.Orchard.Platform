using Laser.Orchard.FidelityGateway.Services;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.FidelityGateway.Menus
{
    public class AdminMenu : INavigationProvider
    {
        public string MenuName
        {
            get { return "admin"; }
        }

        public AdminMenu()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(item => item
                .Caption(T("Action Fidelity"))
                .Position("1.08")
                .Action("Index", "ActionCampaign", new { area = "Laser.Orchard.FidelityGateway" })
                //.AddImageSet("CustomQuery")              
       );
        }
    }
}
