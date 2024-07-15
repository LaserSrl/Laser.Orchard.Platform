using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard.Environment.Extensions;
using System.Web.Mvc;

namespace Laser.Orchard.Questionnaires.Controllers {
        [OrchardFeature("Laser.Orchard.QuestionnaireStatsExport")]
    [OutputCache(NoStore = true, Duration = 0)]
    public class QuestionnaireStatsExportController : Controller {
        private readonly IQuestionnairesServices _questionnairesServices;

        public QuestionnaireStatsExportController(IQuestionnairesServices questionnairesServices) {
            _questionnairesServices = questionnairesServices;
        }

        public ActionResult PdfExport(int questionnaireId, StatsDetailFilterContext filterContext, string redirectUrl) {
            var model = _questionnairesServices.GetStats(questionnaireId, filterContext);
            return Redirect(redirectUrl);
        }
    }
}