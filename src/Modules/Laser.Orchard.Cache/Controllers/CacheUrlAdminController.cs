using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Laser.Orchard.Cache.Models;
using Laser.Orchard.Cache.Services;
using Laser.Orchard.Cache.ViewModels;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Laser.Orchard.Cache.Controllers {

    public class CacheURLAdminController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ICacheAliasServices _cacheAliasServices;
        private readonly IRepository<CacheUrlRecord> _cacheUrlRepository;
        private readonly IRepository<CacheUrlSetting> _cacheUrlSetting;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }
        public CacheURLAdminController(
            IRepository<CacheUrlRecord> cacheUrlRepository,
            IRepository<CacheUrlSetting> cacheUrlSetting,
            IOrchardServices orchardServices,
            ICacheAliasServices cacheAliasServices,
            INotifier notifier
            ) {
            _notifier = notifier;
            _orchardServices = orchardServices;
            _cacheUrlRepository = cacheUrlRepository;
            _cacheUrlSetting = cacheUrlSetting;
            _cacheAliasServices = cacheAliasServices;
            T = NullLocalizer.Instance;
        }

        [HttpGet]
        [Admin]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index(int? page, int? pageSize, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.UrlCache)) {
                return new HttpUnauthorizedResult();
            }
            IEnumerable<CacheUrlRecord> records;
            IEnumerable<CacheUrlRecord> searchrecords;
            var setting = _cacheUrlSetting.Table.ToList().FirstOrDefault();
            int totItems = 0;
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, page, pageSize);
            var expression = search.Expression?.ToLower();
            searchrecords = string.IsNullOrEmpty(expression) ? (IEnumerable<CacheUrlRecord>)_cacheUrlRepository.Fetch(x => true).OrderBy(x => x.CacheURL) : (IEnumerable<CacheUrlRecord>)_cacheUrlRepository.Fetch(x => x.CacheToken.Contains(expression) || x.CacheURL.Contains(expression)).OrderBy(x => x.CacheURL);
            totItems = searchrecords.Count();
            records = searchrecords.Skip(pager.GetStartIndex()).Take(pager.PageSize);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(totItems);
            return View("Index", new SearchIndexVM(records, search, pagerShape, setting));
        }

        [HttpPost]
        [Admin]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.UrlCache)) {
                return new HttpUnauthorizedResult();
            }
            var setting = _cacheUrlSetting.Table.ToList().FirstOrDefault();
            setting.ActiveLog = Request.Form["Option.ActiveLog"].Contains("true");
            setting.PreventDefaultAuthenticatedCache = Request.Form["Option.PreventDefaultAuthenticatedCache"].Contains("true");
            setting.PreventDefaultNotContentItemAuthenticatedCache=Request.Form["Option.PreventDefaultNotContentItemAuthenticatedCache"].Contains("true");
            _cacheUrlSetting.Update(setting);
            _cacheUrlSetting.Flush();
            _cacheAliasServices.RefreshCachedRouteConfig();
            return RedirectToAction("index");
        }
        [HttpGet]
        [Admin]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Edit(int id) {
            var model = new CacheUrlRecord();
            if (id != 0)
                model = _cacheUrlRepository.Get(x => x.Id == id);
            return View("Edit", model);
        }

        [HttpPost]
        [Admin]
        public ActionResult Edit(CacheUrlRecord record) {
            var save = (_orchardServices.WorkContext.HttpContext.Request.Form["btnSave"] == "Save");
            var delete = (_orchardServices.WorkContext.HttpContext.Request.Form["btnDelete"] == "Delete");

            if (record.Id == 0) {
                if (save) {
                    record.CacheURL = record.CacheURL?.ToLower();
                    record.CacheToken = record.CacheToken?.Replace("}{", "}||{");
                    _cacheUrlRepository.Create(record);
                    _notifier.Add(NotifyType.Information, T("Cache Url added: {0}", record.CacheURL));
                }
            }
            else {
                if (delete) {
                    var oldUrl = record.CacheURL;
                    _cacheUrlRepository.Delete(_cacheUrlRepository.Get(r => r.Id == record.Id));
                    _notifier.Add(NotifyType.Information, T("Cache Url removed: {0}", oldUrl));
                }
                else {
                    if (save) {
                        record.CacheURL = record.CacheURL?.ToLower();
                        record.CacheToken = record.CacheToken?.Replace("}{", "}||{");
                        _cacheUrlRepository.Update(record);
                        _notifier.Add(NotifyType.Information, T("Cache Url updated: {0}", record.CacheURL));
                    }
                }
            }
            _cacheUrlRepository.Flush();
            _cacheAliasServices.RefreshCachedRouteConfig();
            return RedirectToAction("Index");
        }
    }
}