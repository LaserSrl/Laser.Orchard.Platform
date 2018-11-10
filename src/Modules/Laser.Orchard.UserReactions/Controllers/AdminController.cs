using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard.Data;
using Laser.Orchard.UserReactions.Models;
using Orchard.UI.Navigation;
using Orchard.DisplayManagement;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.Tasks.Scheduling;


namespace Laser.Orchard.UserReactions.Controllers {

    public class AdminController : Controller {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserReactionsService _reactionsService;
        private readonly IRepository<Models.UserReactionsTypesRecord> _repoTypes;
        private readonly INotifier _notifier;
        private readonly IScheduledTaskManager _taskManager;

        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        // GET: /Admin/
        public AdminController(
            IAuthenticationService authenticationService,
            IMembershipService membershipService, IOrchardServices orcharcServices,
             IUserReactionsService reactionsService,
            IRepository<UserReactions.Models.UserReactionsTypesRecord> repoTypes,
            INotifier notifier,
            IShapeFactory shapeFactory,
            IScheduledTaskManager taskManager
            ) {
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _orchardServices = orcharcServices;
            _reactionsService = reactionsService;
            T = NullLocalizer.Instance;
            _repoTypes = repoTypes;
            _notifier = notifier;
            Shape = shapeFactory;
            _taskManager = taskManager;
        }

        

        [HttpGet]
        public ActionResult ListSummaryReactionByUsers(int Content, int? page, int? pageSize) 
        {
            return ListSummaryReactionByUsers(Content, new PagerParameters {
                Page = page,
                PageSize = pageSize
            });
        }


        [HttpPost]
        [Admin, Themed(false)]
        public ActionResult ListSummaryReactionByUsers(int content, PagerParameters pagerParameters) 
        {
            var routes = _reactionsService.GetListTotalReactions(content);

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(routes.Count());

            var list = _orchardServices.New.List();
            list.AddRange(routes.Skip(pager.GetStartIndex())
                                .Take(pager.PageSize)
                                );
            

            var viewModel = Shape.ViewModel() 
                .ContentItems(list)
                .Pager(pagerShape);
          
            return View(viewModel);
        }

        


        [HttpGet]
        public ActionResult Settings() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit Reaction types settings!")))
                return new HttpUnauthorizedResult();

            var model = _reactionsService.GetTypesTableWithStyles();
            return View(model);
        }


        [HttpPost]
        public ActionResult Settings(UserReactionsTypes model) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit Reaction types settings!")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", T("check your input!")));
                return View(model);
            }

            var reactionSettings = _orchardServices.WorkContext.CurrentSite.As<UserReactionsSettingsPart>();
            reactionSettings.StyleFileNameProvider = model.CssName;
            reactionSettings.AllowMultipleChoices = model.AllowMultipleChoices;
            bool newTypesCreated = false;

            foreach (var item in model.UserReactionsType) {
                if (item.Id == 0) {
                    var styleAcronime = new Laser.Orchard.UserReactions.StyleAcroName();
                    string styleAcronimeName = styleAcronime.StyleAcronime + item.TypeName;
                    _repoTypes.Create(new Models.UserReactionsTypesRecord {
                        Priority = item.Priority,
                        TypeName = item.TypeName,
                        Activating = item.Activating
                    });
                    newTypesCreated = true;
                }
                else {
                    var record = _repoTypes.Get(item.Id);
                    record.Priority = item.Priority;
                    record.TypeName = item.TypeName;
                    record.Activating = item.Activating;
                    _repoTypes.Update(record);
                }
            }

            if (newTypesCreated) {
                // allinea i contenuti tramite un task schedulato
                _taskManager.CreateTask("Laser.Orchard.UserReactionsSettings.Task", DateTime.UtcNow.AddSeconds(5), null);
                _notifier.Add(NotifyType.Warning, T("A task has been scheduled to update reaction summaries for existing contents."));
            }
            _notifier.Add(NotifyType.Information, T("UserReaction settings updating"));
            return RedirectToActionPermanent("Settings");
        }
    }
}
