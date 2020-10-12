using Laser.Orchard.SEO.Exceptions;
using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.Services;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.SEO.Controllers {
    [Admin]
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectsAdminController : Controller {
        private readonly IRedirectService _redirectService;
        private readonly IOrchardServices _orchardServices;
        private readonly ISiteService _siteService;

        private readonly string[] _includeProperties = { "SourceUrl", "DestinationUrl", "IsPermanent" };

        private dynamic Shape { get; set; }

        public RedirectsAdminController(
            IRedirectService redirectService,
            IOrchardServices orchardServices,
            ISiteService siteService,
            IShapeFactory shapeFactory) {

            _redirectService = redirectService;
            _orchardServices = orchardServices;
            _siteService = siteService;
            Shape = shapeFactory;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpGet]
        public ActionResult Index(PagerParameters pagerParameters) {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var tot = _redirectService.GetTable().Count();
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(tot);
            // adjust page value (in case of a previous deletion)
            var firstItemInPage = pager.PageSize * (pager.Page - 1) + 1;
            if (firstItemInPage > tot) {
                pager.Page = decimal.ToInt32(decimal.Ceiling(new decimal(tot) / pager.PageSize));
                pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(tot);
            }

            var items = _redirectService.GetRedirects(pager.GetStartIndex(), pager.PageSize);
            dynamic viewModel = Shape.ViewModel()
                .Redirects(items)
                .Pager(pagerShape);
            return View((object)viewModel);
        }

        [HttpGet]
        public ActionResult Add() {
            return View();
        }

        [HttpPost, ActionName("Add")]
        public ActionResult AddPost() {
            var redirect = new RedirectRule();

            //thanks to the _includeProperties, Update succeeds even if we are not adding the id from UI
            if (!TryUpdateModel(redirect, _includeProperties)) {
                _orchardServices.TransactionManager.Cancel();
                return View(redirect);
            }
            return Validate(redirect, (red) => _redirectService.Add(red));
        }

        [HttpGet]
        public ActionResult Edit(int id) {
            var redirect = _redirectService.GetTable().FirstOrDefault(x => x.Id == id);
            if (redirect == null)
                return HttpNotFound();

            return View(redirect);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id) {
            var redirect = _redirectService.GetTable().FirstOrDefault(x => x.Id == id);
            if (redirect == null)
                return HttpNotFound();

            if (!TryUpdateModel(redirect, _includeProperties)) {
                _orchardServices.TransactionManager.Cancel();
                return View(redirect);
            }
            return Validate(redirect, (red) => _redirectService.Update(red));
        }

        [HttpGet]
        public ActionResult ClearCache(PagerParameters pagerParameters) {
            _redirectService.ClearCache();
            _orchardServices.Notifier.Information(T("Redirect cache successfully reloaded"));
            return RedirectToAction("Index", pagerParameters);
        }

        private ActionResult Validate(RedirectRule redirect, Func<RedirectRule, RedirectRule> doOnSuccess) {
            redirect.SourceUrl = redirect.SourceUrl.TrimEnd('/');
            redirect.DestinationUrl = redirect.DestinationUrl.TrimEnd('/');
            if (redirect.SourceUrl == redirect.DestinationUrl) {
                ModelState.AddModelError("SourceUrl", T("Source url is equal to Destination url").Text);
                _orchardServices.TransactionManager.Cancel();
                return View(redirect);
            }

            try {
                var resultRule = doOnSuccess(redirect);
            }
            catch (RedirectRuleDuplicateException) {
                _orchardServices.Notifier.Error(T("A rule for this Source URL already exists."));
            }
            return RedirectToAction("Index", new { page = Request["page"] });
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            _redirectService.Delete(id);
            _orchardServices.Notifier.Information(T("Redirect record was deleted"));
            return RedirectToAction("Index", new { page = Request["page"] });
        }
    }
}