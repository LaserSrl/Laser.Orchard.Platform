using Orchard;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.Translator
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IOrchardServices _orchardServices;

        public Localizer T { get; set; }

        public AdminMenu(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            try
            {
                if (_orchardServices.Authorizer.Authorize(Permissions.TranslatorPermission.Translate))
                {
                    builder.Add(item => item
                        .Caption(T("Translator"))
                        .Position("20")
                        .Action("Index", "TranslatorTree", new { area = "Laser.Orchard.Translator" })
                    );
                }
            }
            catch { }
        }
    }
}