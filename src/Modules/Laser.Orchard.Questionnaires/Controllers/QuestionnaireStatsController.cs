using Laser.Orchard.Questionnaires.Handlers;
using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Extensions;
using Orchard.Core.Title.Models;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Widgets.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.Questionnaires.Controllers {
    public class QuestionnaireStatsController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IQuestionnairesServices _questionnairesServices;
        private readonly IScheduledTaskManager _taskManager;
        private readonly INotifier _notifier;
        private readonly ShellSettings _shellSettings;
        public Localizer T { get; set; }

        public QuestionnaireStatsController(IOrchardServices orchardServices, IQuestionnairesServices questionnairesServices, IScheduledTaskManager taskManager, INotifier notifier, ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _questionnairesServices = questionnairesServices;
            _taskManager = taskManager;
            _notifier = notifier;
            _shellSettings = shellSettings;
            T = NullLocalizer.Instance;
        }

        [HttpGet]
        [Admin]
        public ActionResult QuestionDetail(int idQuestionario, int idDomanda, int? page, int? pageSize, StatsDetailFilterContext filterContext) {
            var stats = _questionnairesServices.GetStats(idQuestionario, filterContext).QuestionsStatsList.Where(x => x.QuestionId == idDomanda).FirstOrDefault();

            var orderedAnswers = stats.Answers.OrderByDescending(x => x.LastDate).ThenByDescending(o => o.Count).ThenBy(o => o.Answer).ToList();

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, new PagerParameters { Page = page, PageSize = pageSize });
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(stats.Answers.Count());

            stats.Answers = orderedAnswers.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            QuestionnaireStatsDetailViewModel model = new QuestionnaireStatsDetailViewModel();
            model.AnswersStats = stats;
            model.Pager = pagerShape;

            return View((object)model);
        }

        [HttpGet]
        [Admin]
        public ActionResult Detail(int idQuestionario, StatsDetailFilterContext filterContext) {
            var qci = _orchardServices.ContentManager.Get(idQuestionario);
            // Check for the permissions on the specific questionnaire, using the QuestionnaireSpecificAccessPart
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessSpecificQuestionnaireStatistics, qci)) {
                return new HttpUnauthorizedResult();
            }

            var model = _questionnairesServices.GetStats(idQuestionario, filterContext);
            if (filterContext.Export == true) {
                // Check for the permission to export specific questionnaires
                if (!_orchardServices.Authorizer.Authorize(Permissions.ExportSpecificQuestionnaireStatistics, qci)) {
                    _notifier.Error(T("Not authorized to export questionnaire stats"));
                    return View((object)model);
                }

                ContentItem filters = _orchardServices.ContentManager.Create("QuestionnaireStatsExport");
                filters.As<TitlePart>().Title = string.Format("id={0}&from={1:yyyyMMdd}&to={2:yyyyMMdd}&filtercontext={3}", idQuestionario, filterContext.DateFrom.HasValue ? filterContext.DateFrom.Value : new DateTime(), filterContext.DateTo.HasValue ? filterContext.DateTo.Value : new DateTime(), filterContext.Context);
                _orchardServices.ContentManager.Publish(filters);
                _taskManager.CreateTask(StatsExportScheduledTaskHandler.TaskType, DateTime.UtcNow.AddSeconds(-1), filters);
                //_questionnairesServices.SaveQuestionnaireUsersAnswers(idQuestionario, fromDate, toDate);
                _notifier.Add(NotifyType.Information, T("Export started. Please check 'Show Exported Files' in a few minutes to get the result."));
            }
            return View((object)model);
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, StatsSearchContext searchContext) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics) && !_orchardServices.Authorizer.Authorize(Permissions.AccessSpecificQuestionnaireStatistics))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, searchContext);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, StatsSearchContext searchContext) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics) && !_orchardServices.Authorizer.Authorize(Permissions.AccessSpecificQuestionnaireStatistics))
                return new HttpUnauthorizedResult();
            var contentQuery = _orchardServices.ContentManager.Query()
                .ForVersion(VersionOptions.Latest);
            contentQuery = contentQuery
                .Join<QuestionnairePartRecord>();

            // Only get questionnaires current user has access to
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics)) {
                contentQuery = contentQuery
                    .Join<QuestionnaireSpecificAccessPartRecord>()
                    .Where<QuestionnaireSpecificAccessPartRecord>(qsapr => qsapr.SerializedUserIds
                        .Contains("{" + _orchardServices.WorkContext.CurrentUser.Id.ToString() + "}"));
            }
            //contentQuery = contentQuery
            //    .Where<QuestionnairePartRecord>(qpr => _questionnairesServices.CheckPermission(Permissions.AccessSpecificQuestionnaireStatistics, qpr.Id));
            //contentQuery = contentQuery
            //    .Where<QuestionnairePartRecord>(qpr => _orchardServices.Authorizer
            //        .Authorize(Permission.Named(string.Format("{0}{1}", Permissions.AccessSpecificQuestionnaireStatistics.Name, qpr.Id))));

            if (!string.IsNullOrEmpty(searchContext.SearchText)) {
                if (searchContext.SearchType == StatsSearchContext.SearchTypeOptions.Contents) {
                    contentQuery = contentQuery
                        .Where<TitlePartRecord>(w => w.Title.Contains(searchContext.SearchText));
                } else {
                    contentQuery = contentQuery
                        .Where<WidgetPartRecord>(w => w.Title.Contains(searchContext.SearchText));
                }
            }

            contentQuery = contentQuery
                .OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(contentQuery.Count());

            var pageOfContentItems = contentQuery.Slice(pager.GetStartIndex(), pager.PageSize);

            var model = new QuestionnaireSearchViewModel();
            model.Pager = pagerShape;
            model.SearchContext = searchContext;
            model.Questionnaires = pageOfContentItems.Select(x => x);

            return View((object)model);
        }
    }
}