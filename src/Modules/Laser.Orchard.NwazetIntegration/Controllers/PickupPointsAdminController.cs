﻿using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Security;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    [ValidateInput(false), Admin]
    public class PickupPointsAdminController
        : Controller /*ContentControllerBase, IUpdateModel*/ {

        
        // I define a few constants here so I can reference them univocally
        // elsewhere in the feature.
        public const string AreaName = "Laser.Orchard.NwazetIntegration";
        public const string ControllerName = "PickupPointsAdmin";
        /*public const string CreateActionName = "Create";
        public const string EditActionName = "Edit";
        */

        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ISiteService _siteService;
        private readonly ITransactionManager _transactionManager;
        private readonly INotifier _notifier;

        dynamic Shape { get; set; }

        // TODO: different ContentTypes may be used for different types of 
        // pickup points. This will affect most methods in this controller.

        public PickupPointsAdminController(
            IContentManager contentManager,
            IAuthorizer authorizer,
            IWorkContextAccessor workContextAccessor,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            ITransactionManager transactionManager,
            INotifier notifier)
            /*: base(contentManager)*/{

            _contentManager = contentManager;
            _authorizer = authorizer;
            _workContextAccessor = workContextAccessor;
            _siteService = siteService;
            Shape = shapeFactory;
            _transactionManager = transactionManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        
        #region List
        public ActionResult List(
            PickupPointsListViewModel model, PagerParameters pagerParameters) {

            if (!_authorizer.Authorize(
                PickupPointPermissions.MayConfigurePickupPoints,
                null,
                T("Cannot manage pickup points"))) {
                return new HttpUnauthorizedResult();
            }

            var workContext = _workContextAccessor.GetContext();
            var currentCulture = workContext.CurrentCulture;
            var cultureInfo = CultureInfo.GetCultureInfo(currentCulture);

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var versionOptions = VersionOptions.Latest;
            // TODO: version options 

            var query = _contentManager
                .Query(versionOptions, PickupPointPart.DefaultContentTypeName);

            // TODO: filters

            // TODO: order by

            // Pagination
            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            list
                .AddRange(pageOfContentItems
                    .Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                // .Options(model.Options) // TODO
                ;

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }
        #endregion
        /*
        #region Create
        [HttpGet]
        public ActionResult Create() {
            if (!_authorizer.Authorize(
                PickupPointPermissions.MayConfigurePickupPoints,
                null,
                T("Cannot manage pickup points"))) {
                return new HttpUnauthorizedResult();
            }

            var pickupItem = _contentManager
                .New(PickupPointPart.DefaultContentTypeName);
            var model = _contentManager
                .BuildEditor(pickupItem);

            return View(model);
        }
        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(string returnUrl) {
            if (!_authorizer.Authorize(
                PickupPointPermissions.MayConfigurePickupPoints,
                null,
                T("Cannot manage pickup points"))) {
                return new HttpUnauthorizedResult();
            }
            var item = _contentManager
                .New(PickupPointPart.DefaultContentTypeName);
            _contentManager.Create(item, VersionOptions.Draft);
            var model = _contentManager.UpdateEditor(item, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            _contentManager.Publish(item);

            _notifier.Information(string.IsNullOrWhiteSpace(item.TypeDefinition.DisplayName)
                ? T("Your content has been created.")
                : T("Your {0} has been created.", item.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () =>
                RedirectToAction("Edit", new RouteValueDictionary { { "Id", item.Id } }));
        }

        #endregion

        #region Edit

        #endregion

        #region Delete

        #endregion

        #region IUpdateModel implementation
        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public void AddModelError(string key, string errorMessage) {
            ModelState.AddModelError(key, errorMessage);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        #endregion
        */
    }
}