using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Contents;
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

namespace Laser.Orchard.StartupConfig.Controllers {

    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    [Admin]
    public class MaintenanceAdminController : Controller {
        public dynamic _shapeFactory { get; set; }
        private readonly ISiteService _siteService;
        private readonly IOrchardServices _orchardServices;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public MaintenanceAdminController(
            IMaintenanceService maintenanceService,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IOrchardServices orchardServices,
            IContentManager contentManager,
            INotifier notifier) {
            _maintenanceService = maintenanceService;
            _shapeFactory = shapeFactory;
            _siteService = siteService;
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            _notifier = notifier;
        }

        public ActionResult Unpublish(Int32 id) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("Couldn't unpublish content")))
                return new HttpUnauthorizedResult();

            if (contentItem.ContentType == "Maintenance") {
                _contentManager.Unpublish(contentItem);

                _notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName) ? T("That content has been unpublished.") : T("That {0} has been unpublished.", contentItem.TypeDefinition.DisplayName));
            }
            return RedirectToAction("Index");
            //  return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public ActionResult Delete(Int32 id) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.DeleteContent, contentItem, T("Couldn't remove content")))
                return new HttpUnauthorizedResult();

            if (contentItem.ContentType == "Maintenance") {
                _contentManager.Remove(contentItem);
                _notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                   ? T("That content has been removed.")
                   : T("That {0} has been removed.", contentItem.TypeDefinition.DisplayName));
                //        Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName) ? T("That content has been unpublished.") : T("That {0} has been unpublished.", contentItem.TypeDefinition.DisplayName));
            }
            return RedirectToAction("Index");
            //  return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public ActionResult Index(PagerParameters pagerParameters) {
            MaintenanceIndexVM MVM = new MaintenanceIndexVM();

            MVM.Pager = pagerParameters;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters.Page, pagerParameters.PageSize);

            // Apply paging
            var maintenance = _maintenanceService.ListAll().Skip(pager.GetStartIndex()).Take(pager.PageSize);

            // Construct a Pager shape
            var pagerShape = _shapeFactory.Pager(pager).TotalItemCount(_maintenanceService.Get().Count());

            // Create the viewmodel
            var model = new MaintenanceIndexVM(maintenance, pagerShape);

            //// Create a basic query that selects all customer content items, joined with the UserPartRecord table
            //var customerQuery = _maintenanceService.Get();

            //// If the user specified a search expression, update the query with a filter
            //if (!string.IsNullOrWhiteSpace(search.Expression)) {
            //    var expression = search.Expression.Trim();

            //    customerQuery = from customer in customerQuery
            //                    where
            //                        customer.Firstname.Contains(expression, StringComparison.InvariantCultureIgnoreCase) ||
            //                        customer.Surname.Contains(expression, StringComparison.InvariantCultureIgnoreCase) ||
            //                        customer.As<UserPart>().Email.Contains(expression)
            //                    select customer;
            //}

            //// Project the query into a list of customer shapes
            //var customersProjection = from customer in customerQuery
            //                          select Shape.Customer
            //                          (
            //                            Id: customer.Id,
            //                            FirstName: customer.Firstname,
            //                            LastName: customer.Surname,
            //                            Email: customer.As<UserPart>().Email
            //                              // ,CreatedAt: customer.CreatedAt
            //                          );

            //// The pager is used to apply paging on the query and to create a PagerShape
            //var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters.Page, pagerParameters.PageSize);

            //// Apply paging
            //var customers = customersProjection.Skip(pager.GetStartIndex()).Take(pager.PageSize);

            //// Construct a Pager shape
            //var pagerShape = Shape.Pager(pager).TotalItemCount(customerQuery.Count());

            //// Create the viewmodel
            //var model = new CustomersIndexVM(customers, search, pagerShape);

            return View(model);
        }
    }
}