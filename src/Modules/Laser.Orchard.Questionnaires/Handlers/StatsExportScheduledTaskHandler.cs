using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Globalization;

namespace Laser.Orchard.Questionnaires.Handlers {
    public class StatsExportScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IQuestionnairesServices _questionnairesServices;
        public const string TaskType = "Laser.Orchard.Questionnaires.StatsExport.Task";
        public ILogger Logger { get; set; }
        public StatsExportScheduledTaskHandler(IOrchardServices orchardServices, IQuestionnairesServices questionnairesServices) {
            _orchardServices = orchardServices;
            _questionnairesServices = questionnairesServices;
            Logger = NullLogger.Instance;
        }
        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == TaskType) {
                try {
                    int id = 0;
                    // ricava i parametri
                    var filterContext = new StatsDetailFilterContext();
                    string filters = context.Task.ContentItem.As<TitlePart>().Title;
                    string[] pars = filters.Split('&');
                    foreach (var parameter in pars) {
                        string[] keyValue = parameter.Split('=');
                        switch (keyValue[0].ToLower()) {
                            case "id":
                                id = Convert.ToInt32(keyValue[1]);
                                break;
                            case "from":
                                if (string.IsNullOrWhiteSpace(keyValue[1]) == false) {
                                    filterContext.DateFrom = DateTime.ParseExact(keyValue[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                                }
                                break;
                            case "to":
                                if (string.IsNullOrWhiteSpace(keyValue[1]) == false) {
                                    filterContext.DateTo = DateTime.ParseExact(keyValue[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                                }
                                break;
                            case "filtercontext":
                                    filterContext.Context = keyValue[1];
                                break;
                        }
                    }
                    // estrae le statistiche su file
                    _questionnairesServices.SaveQuestionnaireUsersAnswers(id, new ViewModels.StatsDetailFilterContext {
                        DateFrom = filterContext.DateFrom,
                        DateTo = filterContext.DateTo,
                        Context = filterContext.Context
                    });
                }
                catch (Exception ex) {
                    string idcontenuto = "nessun id ";
                    try {
                        idcontenuto = context.Task.ContentItem.Id.ToString();
                    }
                    catch (Exception ex2) { Logger.Error(ex2, ex2.Message); }
                    Logger.Error(ex, "Error on " + TaskType + " for ContentItem id = " + idcontenuto + " : " + ex.Message);
                }
            }
        }
    }
}