using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Security;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    [ValidateInput(false), Admin]
    public class PickupPointsAdminController
        : Controller /*ContentControllerBase, IUpdateModel*/ {

        
        // I define a few constants here so I can reference them univocally
        // elsewhere in the feature.
        public const string AreaName = "Laser.Orchard.NwazetIntegration";
        public const string ControllerName = "PickupPointsAdmin";

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
            INotifier notifier) {

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
    }
}