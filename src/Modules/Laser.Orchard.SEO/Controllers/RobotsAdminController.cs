using Laser.Orchard.SEO.Services;
using Laser.Orchard.SEO.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.SEO.Controllers {
    [Admin]
    [ValidateInput(false)]
	public class RobotsAdminController : Controller {
		private readonly IRobotsService _robotsService;

		public RobotsAdminController(IRobotsService robotsService, IOrchardServices orchardServices) {
			_robotsService = robotsService;
			Services = orchardServices;
			T = NullLocalizer.Instance;
		}

		public IOrchardServices Services { get; set; }
		public Localizer T { get; set; }

		public ActionResult Index() {
			if (!Authorized())
				return new HttpUnauthorizedResult();
			return View(new RobotsFileViewModel() { Text = _robotsService.Get().FileContent });
		}

		[HttpPost]
		public ActionResult Index(RobotsFileViewModel viewModel) {
			if (!Authorized())
				return new HttpUnauthorizedResult();
			var saveResult = _robotsService.Save(viewModel.Text);
			if (saveResult.Item1)
				Services.Notifier.Information(T("Robots.txt settings successfully saved"));
			else {
				Services.Notifier.Information(T("Robots.txt saved with warnings"));
				saveResult.Item2.ToList().ForEach(error =>
					Services.Notifier.Warning(T(error))
				);
			}
			return View(viewModel);
		}

		private bool Authorized() {
			return Services.Authorizer.Authorize(Permissions.ConfigureRobotsTextFile, T("Cannot manage robots.txt file"));
		}
	}
}