﻿using Laser.Orchard.Cookies;
using Laser.Orchard.Cookies.Services;
using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Laser.Orchard.PaymentGateway.Controllers;
using Laser.Orchard.PaymentGateway.ViewModels;
using Nwazet.Commerce.Extensions;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Orchard.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Filters {
    public class InfoResultPaymentFilter : FilterProvider, IActionFilter {
        private readonly IContentManager _contentManager;
        private readonly IGTMProductService _GTMProductService;
        private readonly dynamic _shapeFactory;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRepository<AddedMeasuringPurchase> _addedMeauringPurchaseRepository;
        private readonly IGTMMeasuringPurchaseService _GTMMeasuringPurchaseService;

        public InfoResultPaymentFilter(
            IContentManager contentManager,
            IGTMProductService GTMProductService,
            IShapeFactory shapeFactory,
            IWorkContextAccessor workContextAccessor,
            IRepository<AddedMeasuringPurchase> addedMeauringPurchaseRepository,
            IGTMMeasuringPurchaseService GTMMeasuringPurchaseService) {

            _contentManager = contentManager;
            _GTMProductService = GTMProductService;
            _shapeFactory = shapeFactory;
            _workContextAccessor = workContextAccessor;
            _addedMeauringPurchaseRepository = addedMeauringPurchaseRepository;
            _GTMMeasuringPurchaseService = GTMMeasuringPurchaseService;
        }


        public void OnActionExecuted(ActionExecutedContext filterContext) {
            if (filterContext.Controller is PaymentController
                && filterContext.ActionDescriptor.ActionName.Equals("Info")) {
                var viewResult = filterContext.Result as ViewResult;
                if (viewResult != null) {
                    var model = viewResult.Model as PaymentVM;
                    if (model != null
                        && model.Record != null
                        && model.Record.Success) {
                        // We only have to inject GTM stuff if Statistical cookies are allowed
                        if (_GTMProductService.ShoulAddEcommerceTags()) {
                            // Check if I'm using GA4
                            var useGA4 = _GTMProductService.UseGA4();

                            // select the contentitemid which is the id of the order
                            var orderId = model.Record.ContentItemId;
                            // select order
                            var order = _contentManager.Get<OrderPart>(orderId);

                            // verify if existing record
                            var existing = _addedMeauringPurchaseRepository
                                .Fetch(mp => mp.OrderPartRecord == order.Record
                                    && mp.AddedScript == true);

                            if (!existing.Any()) {
                                var checkoutItems = order.Items.ToList();
                                var products = _contentManager
                                   .GetMany<IContent>(
                                       checkoutItems.Select(p => p.ProductId).Distinct(),
                                       VersionOptions.Published,
                                       QueryHints.Empty)
                                    .ToList();
                                // initialize list of GTMProductVM (now using interface IGAProductVM to manage GA4ProductVM too)
                                var productList = new List<IGAProductVM>();
                                foreach (var p in products) {
                                    // populate list of GTMProductVM 
                                    var part = p.As<GTMProductPart>();
                                    _GTMProductService.FillPart(part);
                                    IGAProductVM vm;
                                    if (useGA4) {
                                        vm = new GA4ProductVM(part);
                                    } else {
                                        vm = new GTMProductVM(part);
                                    }
                                    var checkoutItem = checkoutItems
                                        .Where(c => c.ProductId == p.Id)
                                        .FirstOrDefault();
                                    // add quantity
                                    vm.Quantity = checkoutItem == null ? 0 : checkoutItem.Quantity;
                                    productList.Add(vm);
                                }

                                // populate ViewModel to send at shape
                                var purchaseVM = new GTMPurchaseVM();
                                purchaseVM.ActionField = new GTMActionField {
                                    Id = model.Record.TransactionId,
                                    Revenue = model.Record.Amount,
                                    Tax = _GTMMeasuringPurchaseService.GetVatDue(order),
                                    // shipping with VAT
                                    Shipping = order.ShippingOption.Price
                                };
                                purchaseVM.ProductList = productList;
                                // add coupon information to the ActionField
                                var xCoupons = order.AdditionalElements
                                    .Where(el =>
                                        CouponingUtilities.CouponAlterationType.Equals(el.Name.ToString(), StringComparison.InvariantCultureIgnoreCase));
                                if (xCoupons.Any()) {
                                    purchaseVM.ActionField.Coupon = string.Join(",",
                                        xCoupons.Select(el => el.Attr(CouponingUtilities.CouponAttributeName).ToString()));
                                }

                                // add the record to store the operation
                                _addedMeauringPurchaseRepository.Create(new AddedMeasuringPurchase {
                                    OrderPartRecord = order.Record,
                                    AddedScript = true
                                });

                                _workContextAccessor.GetContext(filterContext)
                                    .Layout.Zones.Head
                                    .Add(_shapeFactory.GTMPurchase(GTMPurchaseVM: purchaseVM));
                            }
                        }
                    }
                }

            }
        }
        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }
    }
}