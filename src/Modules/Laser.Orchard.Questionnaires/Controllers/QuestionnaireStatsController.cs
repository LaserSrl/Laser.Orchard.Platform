using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Laser.Orchard.Questionnaires.Handlers;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System;
using System.Globalization;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Notify;
using Orchard.Localization;
using Laser.Orchard.StartupConfig.FileDownloader.ViewModels;
using Orchard.Environment.Configuration;
using System.Text;
using Laser.Orchard.Questionnaires.Models;
using NHibernate.Linq;
using System.Runtime.ConstrainedExecution;
using Orchard.Widgets.Models;

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
            var model = _questionnairesServices.GetStats(idQuestionario, filterContext);
            if (filterContext.Export == true) {
                ContentItem filters = _orchardServices.ContentManager.Create("QuestionnaireStatsExport");
                filters.As<TitlePart>().Title = string.Format("id={0}&from={1:yyyyMMdd}&to={2:yyyyMMdd}&filtercontext={3}", idQuestionario, filterContext.DateFrom.HasValue? filterContext.DateFrom.Value:new DateTime(), filterContext.DateTo.HasValue ? filterContext.DateTo.Value : new DateTime(), filterContext.Context);
                _orchardServices.ContentManager.Publish(filters);
                _taskManager.CreateTask(StasExportScheduledTaskHandler.TaskType, DateTime.UtcNow.AddSeconds(-1), filters);
                //_questionnairesServices.SaveQuestionnaireUsersAnswers(idQuestionario, fromDate, toDate);
                _notifier.Add(NotifyType.Information, T("Export started. Please check 'Show Exported Files' in a few minutes to get the result."));
            }
            return View((object)model);
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, StatsSearchContext searchContext) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, searchContext);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, StatsSearchContext searchContext) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics))
                return new HttpUnauthorizedResult();
            var contentQuery = _orchardServices.ContentManager.Query()
                .ForVersion(VersionOptions.Latest);
            contentQuery = contentQuery
                .Join<QuestionnairePartRecord>();
            if (!string.IsNullOrEmpty(searchContext.SearchText)) {
                if (searchContext.SearchType == StatsSearchContext.SearchTypeOptions.Contents) {
                    contentQuery = contentQuery
                        .Where<TitlePartRecord>(w => w.Title.Contains(searchContext.SearchText));
                }
                else {
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