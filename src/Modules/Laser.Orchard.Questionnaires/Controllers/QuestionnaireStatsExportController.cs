using Laser.Orchard.Questionnaires.Handlers;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Notify;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.Questionnaires.Controllers {
    [OrchardFeature("Laser.Orchard.QuestionnaireStatsExport")]
    [OutputCache(NoStore = true, Duration = 0)]
    public class QuestionnaireStatsExportController : Controller {
        
        private readonly INotifier _notifier;        
        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _taskManager;

        public Localizer T { get; set; }

        public QuestionnaireStatsExportController( INotifier notifier,
            IOrchardServices orchardServices,
            IScheduledTaskManager taskManager) {

            _notifier = notifier;
            _orchardServices = orchardServices;
            _taskManager = taskManager;

            T = NullLocalizer.Instance;
        }

        public ActionResult PdfExport(int questionnaireId, string dateFrom, string dateTo, string dateFormat, string context, string redirectUrl) {
            DateTime? dtDateFrom = null;
            DateTime? dtDateTo = null;
            if (DateTime.TryParse(dateFrom, out var dt1)) {
                dtDateFrom = new DateTime?(dt1);
            }
            if (DateTime.TryParse(dateTo, out var dt2)) {
                dtDateTo = new DateTime?(dt2);
            }

            var filterContext = new StatsDetailFilterContext {
                DateFrom = dtDateFrom,
                DateTo = dtDateTo,
                Context = context
            };

            ContentItem filters = _orchardServices.ContentManager.Create("QuestionnaireStatsExport");
            filters.As<TitlePart>().Title = string.Format("id={0}&from={1:yyyyMMdd}&to={2:yyyyMMdd}&filtercontext={3}", questionnaireId, filterContext.DateFrom.HasValue ? filterContext.DateFrom.Value : new DateTime(), filterContext.DateTo.HasValue ? filterContext.DateTo.Value : new DateTime(), filterContext.Context);
            _orchardServices.ContentManager.Publish(filters);
            _taskManager.CreateTask(StatsPdfExportScheduledTaskHandler.TaskType, DateTime.UtcNow.AddSeconds(-1), filters);

            _notifier.Add(NotifyType.Information, T("Export started. Please check 'Show Exported Files' in a few minutes to get the result."));

            return Redirect(redirectUrl);
        }
    }
}