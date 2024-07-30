using Laser.Orchard.Commons.Services;
using Laser.Orchard.Pdf.Services;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Widgets.Models;
using System;
using System.Globalization;
using System.IO;
using System.Web.Hosting;

namespace Laser.Orchard.Questionnaires.Handlers {
    [OrchardFeature("Laser.Orchard.QuestionnaireStatsExport")]
    public class StatsPdfExportScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IQuestionnairesServices _questionnairesServices;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly IPdfServices _pdfServices;
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardServices _orchardServices;

        public const string TaskType = "Laser.Orchard.Questionnaires.StatsPdfExport.Task";
        public ILogger Logger { get; set; }

        public StatsPdfExportScheduledTaskHandler(IQuestionnairesServices questionnairesServices,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay,
            IPdfServices pdfServices,
            ShellSettings shellSettings,
            IOrchardServices orchardServices) {

            _questionnairesServices = questionnairesServices;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;
            _pdfServices = pdfServices;
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;

            Logger = NullLogger.Instance;
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

                    var shape = _shapeFactory.Create("QuestionnaireDetail_Pdf", Arguments.From(new {
                        Model = model
                    }));

                    var markup = _shapeDisplay.Display(shape);

                    var buffer = _pdfServices.PdfFromHtml(markup);

                    ContentItem ci = _orchardServices.ContentManager.Get(questionnaireId);
                    string fileName = "";
                    if (ci.As<TitlePart>() != null) {
                        fileName = String.Format("{0}-{1:yyyyMMdd}-{2:yyyyMMdd}.pdf", new CommonUtils().NormalizeFileName(ci.As<TitlePart>().Title, "questionnaire", ' '), filterContext.DateFrom.HasValue ? filterContext.DateFrom.Value : new DateTime(), filterContext.DateTo.HasValue ? filterContext.DateTo.Value : new DateTime());
                    } else if (ci.As<WidgetPart>() != null) {
                        fileName = String.Format("{0}-{1:yyyyMMdd}-{2:yyyyMMdd}.pdf", new CommonUtils().NormalizeFileName(ci.As<WidgetPart>().Title, "questionnaire", ' '), filterContext.DateFrom.HasValue ? filterContext.DateFrom.Value : new DateTime(), filterContext.DateTo.HasValue ? filterContext.DateTo.Value : new DateTime());
                    } else {
                        fileName = String.Format("{0}-{1:yyyyMMdd}-{2:yyyyMMdd}.pdf", "questionnaire", filterContext.DateFrom.HasValue ? filterContext.DateFrom.Value : new DateTime(), filterContext.DateTo.HasValue ? filterContext.DateTo.Value : new DateTime());
                    }

                    string filePath = HostingEnvironment.MapPath(
                        string.Format("~/App_Data/Sites/{0}/QuestionnairesStatistics/{1}/{2}",
                            _shellSettings.Name, questionnaireId, fileName));
                    FileInfo fi = new FileInfo(filePath);
                    if (fi.Directory.Parent.Exists == false) {
                        Directory.CreateDirectory(fi.Directory.Parent.FullName);
                    }
                    // Creo la directory Questionnaires
                    if (!fi.Directory.Exists) {
                        Directory.CreateDirectory(fi.DirectoryName);
                    }

                    File.WriteAllBytes(filePath, buffer);
                } catch (Exception ex) {
                    Logger.Error(ex, "Error on " + TaskType + " : " + ex.Message);
                }
            }
        }
    }
}