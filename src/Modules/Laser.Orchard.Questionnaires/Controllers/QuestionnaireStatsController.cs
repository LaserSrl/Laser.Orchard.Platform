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
        public ActionResult QuestionDetail(int idQuestionario, int idDomanda, int? page, int? pageSize, string from = null, string to = null) {
            DateTime fromDate, toDate;
            CultureInfo provider = CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture);
            DateTime.TryParse(from, provider, DateTimeStyles.None, out fromDate);
            DateTime.TryParse(to, provider, DateTimeStyles.None, out toDate);

            var stats = _questionnairesServices.GetStats(idQuestionario, (DateTime?)fromDate, (DateTime?)toDate).QuestionsStatsList.Where(x => x.QuestionId == idDomanda).FirstOrDefault();

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
        public ActionResult Detail(int idQuestionario, string from = null, string to = null, bool export = false) {
            DateTime fromDate, toDate;
            CultureInfo provider = CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture);
            DateTime.TryParse(from, provider, DateTimeStyles.None, out fromDate);
            DateTime.TryParse(to, provider, DateTimeStyles.None, out toDate);
            var model = _questionnairesServices.GetStats(idQuestionario, (DateTime?)fromDate, (DateTime?)toDate);
            if (export == true) {
                ContentItem filters = _orchardServices.ContentManager.Create("QuestionnaireStatsExport");
                filters.As<TitlePart>().Title = string.Format("id={0}&from={1:yyyyMMdd}&to={2:yyyyMMdd}", idQuestionario, fromDate, toDate);
                _orchardServices.ContentManager.Publish(filters);
                _taskManager.CreateTask(StasExportScheduledTaskHandler.TaskType, DateTime.UtcNow.AddSeconds(-1), filters);
                //_questionnairesServices.SaveQuestionnaireUsersAnswers(idQuestionario, fromDate, toDate);
                _notifier.Add(NotifyType.Information, T("Export started. Please check 'Show Exported Files' in a few minutes to get the result."));
            }
            return View((object)model);
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, string searchExpression) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, searchExpression);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, string searchExpression) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics))
                return new HttpUnauthorizedResult();

            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query()
                                                                                     .ForType("Questionnaire")
                                                                                     .ForVersion(VersionOptions.Latest)
                                                                                     .OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);
            if (!string.IsNullOrEmpty(searchExpression))
                contentQuery = contentQuery.Where<TitlePartRecord>(w => w.Title.Contains(searchExpression));

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(contentQuery.Count());
            var pageOfContentItems = contentQuery.Slice(pager.GetStartIndex(), pager.PageSize);

            var model = new QuestionnaireSearchViewModel();
            model.Pager = pagerShape;
            model.Questionnaires = pageOfContentItems;

            return View((object)model);
        }
    }
}