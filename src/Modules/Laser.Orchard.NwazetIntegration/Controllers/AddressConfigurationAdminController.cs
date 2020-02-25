using Laser.Orchard.NwazetIntegration.Services;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    [ValidateInput(false), Admin]
    public class AddressConfigurationAdminController : Controller, IUpdateModel {
        private readonly IAuthorizer _authorizer;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly INotifier _notifier;
        private readonly IAddressConfigurationService _addressConfigurationService;
        private readonly IAddressConfigurationSettingsService _addressSettingsService;

        private const string groupInfoId = "AddressConfigurationSiteSettings";
        
        public AddressConfigurationAdminController(
            IAuthorizer authorizer,
            ISiteService siteService,
            IContentManager contentManager,
            ITransactionManager transactionManager,
            INotifier notifier,
            IAddressConfigurationService addressConfigurationService,
            IAddressConfigurationSettingsService addressSettingsService) {

            _authorizer = authorizer;
            _siteService = siteService;
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _notifier = notifier;
            _addressConfigurationService = addressConfigurationService;
            _addressSettingsService = addressSettingsService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_authorizer.Authorize(CommercePermissions.ManageCommerce,
                null, T("Not authorized to manage address configuration settings")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            dynamic model = _contentManager.BuildEditor(site, groupInfoId);

            if (model == null)
                return HttpNotFound();

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!_authorizer.Authorize(CommercePermissions.ManageCommerce, 
                null, T("Not authorized to manage address configuration settings")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            var model = _contentManager.UpdateEditor(site, this, groupInfoId);

            if (model == null) {
                _transactionManager.Cancel();
                return HttpNotFound();
            }

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();

                return View(model);
            }
            _notifier.Information(T("Address configuration settings updated"));

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        [HttpPost]
        public JsonResult GetChildren(int territoryId = 0) {
            var parent = _addressConfigurationService
                .SingleTerritory(territoryId);
            if (parent == null) {
                // this is an error
                return null;
            } else {
                return Json(new {
                    Success = true,
                    Territories = parent.Children
                        .Select(ci => {
                            var tp = ci.As<TerritoryPart>();
                            var id = tp.Record.TerritoryInternalRecord.Id;
                            bool isCountry = false, isProvince = false, isCity = false, isNone = false;
                            if (_addressSettingsService.SelectedCountryIds
                                    .Contains(id)) {
                                isCountry = true;
                            } else if (_addressSettingsService.SelectedProvinceIds
                                    .Contains(id)) {
                                isProvince = true;
                            } else if (_addressSettingsService.SelectedCityIds
                                    .Contains(id)) {
                                isCity = true;
                            }else {
                                isNone = true;
                            }
                            return new {
                                Id = id,
                                DisplayText = _contentManager
                                    .GetItemMetadata(ci).DisplayText,
                                IsCountry = isCountry,
                                IsProvince = isProvince,
                                IsCity = isCity,
                                IsNone = isNone,
                                CountryISO = _addressSettingsService.GetCountryISOCode(id),
                                HasChildren = tp.Record.Children.Any(),
                                ChildrenCount = tp.Record.Children.Count()
                            };
                        })
                });
            }
            // TODO
            return Json(new List<string>());
        }
    }
}