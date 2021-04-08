using Laser.Orchard.StartupConfig.AppleEnvironment.Permissions;
using Laser.Orchard.StartupConfig.AppleEnvironment.Services;
using Laser.Orchard.StartupConfig.AppleEnvironment.ViewModels;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.AppleEnvironment.Controllers {
    [Admin]
    [ValidateInput(false)]
    [OrchardFeature("Laser.Orchard.ApplePay.DomainAssociation")]
    public class MerchantDomainAssociationAdminController : Controller {
        private readonly IMerchantDomainAssociationService _merchantDomainAssociationService;

        public MerchantDomainAssociationAdminController(IMerchantDomainAssociationService merchantDomainAssociationService, IOrchardServices orchardServices) {
            _merchantDomainAssociationService = merchantDomainAssociationService;
            Services = orchardServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Authorized())
                return new HttpUnauthorizedResult();
            var manifest = _merchantDomainAssociationService.Get();
            return View(new MerchantDomainAssociationViewModel() {
                Text = manifest.FileContent,
                Enable = manifest.Enable,
            });
        }

        [HttpPost]
        public ActionResult Index(MerchantDomainAssociationViewModel viewModel) {
            if (!Authorized())
                return new HttpUnauthorizedResult();
            _merchantDomainAssociationService.Save(viewModel);
            Services.Notifier.Information(T("File settings successfully saved."));
            return View(viewModel);
        }

        private bool Authorized() {
            return Services.Authorizer.Authorize(MerchantDomainAssociationPermissions.MerchantDomainAssociationFile, T("Cannot manage Apple Merchant Domain Association file"));
        }
    }
}