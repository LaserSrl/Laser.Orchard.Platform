using Laser.Orchard.Questionnaires.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Handlers {
    public class StasExportScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IQuestionnairesServices _questionnairesServices;
        public const string TaskType = "Laser.Orchard.Questionnaires.StatsExport.Task";
        public ILogger Logger { get; set; }
        public StasExportScheduledTaskHandler(IOrchardServices orchardServices, IQuestionnairesServices questionnairesServices) {
            _orchardServices = orchardServices;
            _questionnairesServices = questionnairesServices;
            Logger = NullLogger.Instance;
        }
        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == TaskType) {
                try {
                    int id = 0;
                    DateTime? from = null;
                    DateTime? to = null;
                    // ricava i parametri
                    string filters = context.Task.ContentItem.As<TitlePart>().Title;
                    string[] pars = filters.Split('&');
                    foreach (var parameter in pars) {
                        string[] keyValue = parameter.Split('=');
                        switch (keyValue[0]) {
                            case "id":
                                id = Convert.ToInt32(keyValue[1]);
                                break;
                            case "from":
                                if (string.IsNullOrWhiteSpace(keyValue[1]) == false) {
                                    from = DateTime.ParseExact(keyValue[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                                }
                                break;
                            case "to":
                                if (string.IsNullOrWhiteSpace(keyValue[1]) == false) {
                                    to = DateTime.ParseExact(keyValue[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                                }
                                break;
                        }
                    }
                    // estrae le statistiche su file
                    _questionnairesServices.SaveQuestionnaireUsersAnswers(id, from, to);
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