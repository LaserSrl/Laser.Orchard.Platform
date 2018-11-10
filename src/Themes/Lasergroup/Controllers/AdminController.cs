using Orchard.ContentManagement;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.Security;
using Orchard.Themes;
using Lasergroup.ViewModels;
using Lasergroup.Extensions;
using Lasergroup.Models;
using Orchard.UI.Notify;

namespace Lasergroup.Controllers {
    [ValidateInput(false), Admin]
    public class AdminController : Controller {

        private readonly IAuthorizer _authorizer;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;

        public AdminController(
            IAuthorizer authorizer,
            ISiteService siteService,
            IContentManager contentManager,
            INotifier notifier) {

            _authorizer = authorizer;
            _siteService = siteService;
            _contentManager = contentManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't update theme settings")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            var additionalCssPart = site.As<AdditionalCssSettingsPart>();

            var vm = new AdditionalCssSettingsPartEditorViewModel {
                AdditionalCss = additionalCssPart.AdditionalCss
            };

            return View(
                "~/Themes/Lasergroup/Views/Admin/Index.cshtml",
                vm);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost(AdditionalCssSettingsPartEditorViewModel viewModel) {
            if (!_authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't update theme settings")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            var additionalCssPart = site.As<AdditionalCssSettingsPart>();
            additionalCssPart.AdditionalCss = viewModel.AdditionalCss;
            _notifier.Information(T("Your settings have been saved."));

            return RedirectToAction("Index");
        }
    }
}