using iTextSharp.text;
using Laser.Orchard.Commons.Services;
using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using MiniExcelLibs;
using NHibernate.Util;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Laser.Orchard.Questionnaires.Handlers {
    public class AnswerCount {
        public string Answer { get; set; }
        public int Count { get; set; }
    }

    [OrchardFeature("Laser.Orchard.QuestionnaireStatsExport")]
    public class StatsExcelExportScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IQuestionnairesServices _questionnairesServices;
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardServices _orchardServices;

        public const string TaskType = "Laser.Orchard.Questionnaires.StatsExcelExport.Task";
        public ILogger Logger { get; set; }
        public Localizer T {get; set;}

        public StatsExcelExportScheduledTaskHandler(IQuestionnairesServices questionnairesServices,
            ShellSettings shellSettings,
            IOrchardServices orchardServices) {

            _questionnairesServices = questionnairesServices;
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == TaskType) {
                try {
                    int questionnaireId = 0;
                    // Read parameters from context title
                    var filterContext = new StatsDetailFilterContext();
                    string filters = context.Task.ContentItem.As<TitlePart>().Title;
                    string[] pars = filters.Split('&');
                    foreach (var parameter in pars) {
                        string[] keyValue = parameter.Split('=');
                        switch (keyValue[0].ToLower()) {
                            case "id":
                                questionnaireId = Convert.ToInt32(keyValue[1]);
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

                    var model = _questionnairesServices.GetStats(questionnaireId, filterContext);
                    var questionnaire = _orchardServices.ContentManager.Get(questionnaireId);
                    if (questionnaire != null && questionnaire.As<QuestionnairePart>() != null) {
                        var qPart = questionnaire.As<QuestionnairePart>();
                        // Find all sections of the questionnaire
                        // A single Excel Workbook needs to be created for each section.
                        var sections = qPart.Questions.Select(q => q.Section).Distinct();

                        ContentItem ci = _orchardServices.ContentManager.Get(questionnaireId);
                        string fileName = "";

                        foreach (var s in sections) {
                            if (ci.As<TitlePart>() != null) {
                                fileName = String.Format("{0}-{1}-{2:yyyyMMdd}-{3:yyyyMMdd}.xlsx",
                                    new CommonUtils().NormalizeFileName(ci.As<TitlePart>().Title, "questionnaire", ' '),
                                    string.IsNullOrWhiteSpace(s) ? "no section" : s,
                                    filterContext.DateFrom.HasValue ? filterContext.DateFrom.Value : new DateTime(),
                                    filterContext.DateTo.HasValue ? filterContext.DateTo.Value : new DateTime());
                            } else if (ci.As<WidgetPart>() != null) {
                                fileName = String.Format("{0}-{1}-{2:yyyyMMdd}-{3:yyyyMMdd}.xlsx",
                                    new CommonUtils().NormalizeFileName(ci.As<WidgetPart>().Title, "questionnaire", ' '),
                                    string.IsNullOrWhiteSpace(s) ? "no section" : s,
                                    filterContext.DateFrom.HasValue ? filterContext.DateFrom.Value : new DateTime(),
                                    filterContext.DateTo.HasValue ? filterContext.DateTo.Value : new DateTime());
                            } else {
                                fileName = String.Format("{0}-{1}-{2:yyyyMMdd}-{3:yyyyMMdd}.xlsx",
                                    "questionnaire",
                                    string.IsNullOrWhiteSpace(s) ? "no section" : s,
                                    filterContext.DateFrom.HasValue ? filterContext.DateFrom.Value : new DateTime(),
                                    filterContext.DateTo.HasValue ? filterContext.DateTo.Value : new DateTime());
                            }

                            string filePath = HostingEnvironment.MapPath(
                                string.Format("~/App_Data/Sites/{0}/Export/QuestionnairesStatistics/{1}",
                                _shellSettings.Name, fileName));
                            FileInfo fi = new FileInfo(filePath);
                            if (fi.Directory.Parent.Exists == false) {
                                Directory.CreateDirectory(fi.Directory.Parent.FullName);
                            }
                            // Creo la directory Questionnaires
                            if (!fi.Directory.Exists) {
                                Directory.CreateDirectory(fi.DirectoryName);
                            }

                            // For each section, get only the answers for current section
                            var sectionStats = model.QuestionsStatsList
                                .Where(qsl => qPart.Questions
                                    .Where(q => q.Section.Equals(s))
                                    .Select(q => q.Id)
                                .Contains(qsl.QuestionId));

                            var sheets = new Dictionary<string, object>();

                            foreach (var ss in sectionStats) {
                                var answers = new DataTable();
                                answers.TableName = ss.Question;
                                answers.Columns.Add(T("Answer").Text, typeof(string));
                                answers.Columns.Add(T("Count").Text, typeof(int));

                                foreach (var asvm in ss.Answers) {
                                    var a = answers.NewRow();
                                    a[T("Answer").Text] = asvm.Answer;
                                    a[T("Count").Text] = asvm.Count;
                                    answers.Rows.Add(a);
                                }

                                var sheetName = ss.Question;
                                // Sheet name must be limited to 30 characters.
                                if (sheetName.Length > 28) {
                                    // TODO: avoid sheet name duplicates
                                    sheetName = ss.Question.Substring(0, 28);
                                }
                                
                                sheets.Add(sheetName, answers);
                            }

                            MiniExcel.SaveAs(filePath, sheets, overwriteFile: true);
                        }
                    }

                } catch (Exception ex) {
                    Logger.Error(ex, "Error on " + TaskType + " : " + ex.Message);
                }
            }
        }
    }
}