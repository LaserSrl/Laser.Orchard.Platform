using Laser.Orchard.GDPR.Models;
using Laser.Orchard.GDPR.Permissions;
using Laser.Orchard.GDPR.Services;
using Laser.Orchard.GDPR.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Laser.Orchard.GDPR.ViewModels;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Title.Models;

namespace Laser.Orchard.GDPR.Controllers {
    [ValidateInput(false), Admin]
    public class GDPRAdminController : Controller {

        private readonly IAuthorizer _authorizer;
        private readonly ISiteService _siteService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IContentGDPRManager _contentGDPRManager;

        public GDPRAdminController(
            IAuthorizer authorizer,
            ISiteService siteService,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IContentGDPRManager contentGDPRManager) {

            _authorizer = authorizer;
            _siteService = siteService;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            Shape = shapeFactory;
            _notifier = notifier;
            _contentGDPRManager = contentGDPRManager;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpGet]
        public ActionResult Index(GDPRAdminViewModel model, PagerParameters pagerParameters) {
            // depending on the user's permissions, we need to show different stuff
            // but if the user has none of the GDPR permissions, they may see nothing
            if (!_authorizer.Authorize(GDPRPermissions.ManageAnonymization)
                && !_authorizer.Authorize(GDPRPermissions.ManageErasure)) {
                return new HttpUnauthorizedResult();
            }

            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters?? new PagerParameters());

            var query = _contentManager
                .Query(VersionOptions.Latest, GetProfileTypes().Select(ctd => ctd.Name).ToArray());

            if (!string.IsNullOrWhiteSpace(model?.SearchExpression)) {
                // TODO: figure out how to make this independent of the TitlePartRecord, maybe
                // by using ITitleAspect, or even the DisplayAlias somehow. Depending on a thing
                // from Orchard.Core (the Title stuff is there) is not so bad, but still...
                query = query.Where<TitlePartRecord>(w => w.Title.Contains(model.SearchExpression));
            }

            var maxPagedCount = _siteService.GetSiteSettings().MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                pager.PageSize = maxPagedCount;
            var pagerShape = Shape.Pager(pager).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            list.AddRange(pageOfContentItems.Select(ci => _contentManager.BuildDisplay(ci, "ProfileItemAdminSummary")));
            
            var viewModel = new GDPRAdminViewModel {
                ContentItems = list,
                Pager = pagerShape,
                SearchExpression = model?.SearchExpression ?? ""
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Search")]
        public ActionResult IndexSearchPOST(GDPRAdminViewModel viewModel) {
            
            return Index(viewModel, null);
        }

        private IEnumerable<ContentTypeDefinition> GetProfileTypes() {
            return _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(ctd =>
                    ctd
                    .Parts
                    .Any(p => p.PartDefinition.Name == "GDPRPart"))
                .Where(ctd =>
                    ctd
                    .Parts
                    .FirstOrDefault(p => p.PartDefinition.Name == "GDPRPart")
                    .Settings
                    .GetModel<GDPRPartTypeSettings>()
                    .IsProfileItemType)
                .Where(ctd => {
                    var newItem = _contentManager.New(ctd.Name);
                    return _authorizer.Authorize(GDPRPermissions.ManageAnonymization, newItem)
                        || _authorizer.Authorize(GDPRPermissions.ManageErasure, newItem);
                });
        }
        
        [HttpPost]
        public ActionResult Anonymize(int id, string returnUrl) {
            return GDPRProcess(new GDPRProcessOptions {
                Id = id,
                ReturnUrl = returnUrl,
                Permission = GDPRPermissions.ManageAnonymization,
                ActionString = T("anonymization"),
                ActedString = T("anonymized"),
                Process = _contentGDPRManager.Anonymize,
                ErrorChecks = GDPRErrorChecks
            });
        }

        [HttpPost]
        public ActionResult Erase(int id, string returnUrl) {
            return GDPRProcess(new GDPRProcessOptions {
                Id = id,
                ReturnUrl = returnUrl,
                Permission = GDPRPermissions.ManageErasure,
                ActionString = T("erasure"),
                ActedString = T("erased"),
                Process = _contentGDPRManager.Erase,
                ErrorChecks = GDPRErrorChecks
            });
        }
        
        [HttpPost]
        public ActionResult SetProtection(int id, string returnUrl) {
            return GDPRProcess(new GDPRProcessOptions {
                Id = id,
                ReturnUrl = returnUrl,
                Permission = GDPRPermissions.ManageItemProtection,
                ActionString = T("protection status"),
                ActedString = T(""),
                Process = SetProtectionFlag,
                ErrorChecks = ProtectionErrorChecks
            });
        }

        [HttpPost]
        public ActionResult ResetProtection(int id, string returnUrl) {
            return GDPRProcess(new GDPRProcessOptions {
                Id = id,
                ReturnUrl = returnUrl,
                Permission = GDPRPermissions.ManageItemProtection,
                ActionString = T("protection status"),
                ActedString = T(""),
                Process = ResetProtectionFlag,
                ErrorChecks = ProtectionErrorChecks
            });
        }

        private ActionResult GDPRProcess(GDPRProcessOptions options) {
            var unauthorizedMessage = T("Unauthorized to manage {0} of contents.", options.ActionString);
            if (!_authorizer.Authorize(options.Permission, unauthorizedMessage)) {
                return new HttpUnauthorizedResult();
            }

            var item = _contentManager.Get(options.Id, VersionOptions.Latest);
            if (item == null) {
                return HttpNotFound();
            }

            // earlier we checked the generic permission, here we check for the specific items
            if (!_authorizer.Authorize(options.Permission, item, unauthorizedMessage)) {
                return new HttpUnauthorizedResult();
            }

            // Check other error conditions: if any is found, write down the corresponding message
            LocalizedString msg = options.ErrorChecks(options, item);

            if (msg == null) {
                options.Process(item);
            } else {
                _notifier.Error(msg);
                Logger.Debug(msg.Text);
            }

            return this.RedirectLocal(options.ReturnUrl, () => RedirectToAction("Index"));
        }

        private LocalizedString GDPRErrorChecks(GDPRProcessOptions options, ContentItem item) {
            // Check other error conditions: if any is found, write down the corresponding message
            LocalizedString msg = null;
            // Content items without a GDPRPart cannot be configured for anonymization/erasure
            if (item.As<GDPRPart>() == null) {
                msg = T("The item has no configuration saved for {0}.", options.ActionString);
            }
            // Protected Items should not be anonymized/erased
            if (msg == null && item.As<GDPRPart>().IsProtected) {
                msg = T("The item is protected and cannot be {0}.", options.ActedString);
            }
            // The actions can only be called on ProfileItems
            if (msg == null && !item.As<GDPRPart>().TypePartDefinition.Settings.GetModel<GDPRPartTypeSettings>().IsProfileItemType) {
                msg = T("The item is not a Profile.");
            }
            return msg;
        }

        private LocalizedString ProtectionErrorChecks(GDPRProcessOptions options, ContentItem item) {
            // Check other error conditions: if any is found, write down the corresponding message
            LocalizedString msg = null;
            // Content items without a GDPRPart cannot be configured for anonymization/erasure
            if (item.As<GDPRPart>() == null) {
                msg = T("The item has no configuration saved for {0}.", options.ActionString);
            }
            return msg;
        }
        
        private void SetProtectionFlag(ContentItem item) {
            item.As<GDPRPart>().IsProtected = true;
        }

        private void ResetProtectionFlag(ContentItem item) {
            item.As<GDPRPart>().IsProtected = false;
        }


        class GDPRProcessOptions {
            public int Id { get; set; }
            public string ReturnUrl { get; set; }
            public Permission Permission { get; set; }
            public LocalizedString ActionString { get; set; }
            public LocalizedString ActedString { get; set; }

            public Action<ContentItem> Process { get; set; }

            public Func<GDPRProcessOptions, ContentItem, LocalizedString> ErrorChecks { get; set; }
        }
    }
}