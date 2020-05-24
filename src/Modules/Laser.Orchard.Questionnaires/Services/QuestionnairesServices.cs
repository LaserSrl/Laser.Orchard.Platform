using AutoMapper;
using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Settings;
using Laser.Orchard.Questionnaires.ViewModels;
using Laser.Orchard.StartupConfig.Localization;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using NHibernate.Criterion;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Security;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using Orchard.Email.Services;
using Orchard.Tokens;
using Laser.Orchard.Commons.Services;
using System.Security.Cryptography;
using System.Linq.Expressions;

namespace Laser.Orchard.Questionnaires.Services {

    public class QuestionnairesServices : IQuestionnairesServices {
        private readonly IRepository<UserAnswersRecord> _repositoryUserAnswer;
        private readonly IRepository<TitlePartRecord> _repositoryTitle;
        private readonly IRepository<UserAnswerInstanceRecord> _repositoryinstanceRecords;
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkflowManager _workflowManager;
        private readonly INotifier _notifier;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly ITemplateService _templateService;
        private readonly ITransactionManager _transactionManager;
        private readonly IDateLocalization _dateLocalization;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IDateLocalizationServices _dateServices;
        private readonly ShellSettings _shellSettings;
        private readonly Lazy<ISmtpChannel> _messageManager;
        private readonly ITokenizer _tokenizer;
        private readonly IQuestionAnswerRepositoryService _questionAnswerRepositoryService;

        public Localizer T { get; set; }

        public QuestionnairesServices(IOrchardServices orchardServices,
            IRepository<TitlePartRecord> repositoryTitle,
            IRepository<UserAnswersRecord> repositoryUserAnswer,
            IWorkflowManager workflowManager,
            INotifier notifier,
            IControllerContextAccessor controllerContextAccessor,
            Lazy<ISmtpChannel> messageManager,
            ITemplateService templateService,
            ITokenizer tokenizer,
            ITransactionManager transactionManager,
            IDateLocalization dateLocalization,
            IScheduledTaskManager taskManager,
            IDateLocalizationServices dateServices,
            IQuestionAnswerRepositoryService questionAnswerRepositoryService,
            ShellSettings shellSettings,
            IRepository<UserAnswerInstanceRecord> repositoryinstanceRecords) {

            _orchardServices = orchardServices;
            _repositoryTitle = repositoryTitle;
            _repositoryUserAnswer = repositoryUserAnswer;
            _workflowManager = workflowManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _controllerContextAccessor = controllerContextAccessor;
            _templateService = templateService;
            _messageManager = messageManager;
            _tokenizer = tokenizer;
            _questionAnswerRepositoryService = questionAnswerRepositoryService;
            _transactionManager = transactionManager;
            _dateLocalization = dateLocalization;
            _taskManager = taskManager;
            _dateServices = dateServices;
            _shellSettings = shellSettings;
            _repositoryinstanceRecords = repositoryinstanceRecords;
        }

        private string getusername(int id) {
            if (id > 0) {
                try {
                    return ((dynamic)_orchardServices.ContentManager.Get(id)).UserPart.UserName;
                }
                catch (Exception) {
                    return "No User";
                }
            }
            else
                return "No User";
        }

        public bool SendTemplatedEmailRanking() {
            var query = _orchardServices.ContentManager.Query();
            var list = query.ForPart<GamePart>().Where<GamePartRecord>(x => x.workflowfired == false).List();
            // la condizione Id > 0 serve solo a forzare la join con la tabella RankingPartRecord, perché la clausola ForPart non è sufficiente (fa solo scansione in memoria)
            var listranking = _orchardServices.ContentManager.Query().ForPart<RankingPart>().Where<RankingPartRecord>(x => x.Id > 0).List();
            foreach (GamePart gp in list) {
                ContentItem Ci = gp.ContentItem;
                if (((dynamic)Ci).ActivityPart != null && ((dynamic)Ci).ActivityPart.DateTimeEnd != null) {
                    if (((dynamic)Ci).ActivityPart.DateTimeEnd < DateTime.Now) {
                        //    gp.workflowfired = false; //todo remove this line
                        //      if (gp.workflowfired == false) {
                        if (gp.Settings.GetModel<GamePartSettingVM>().SendEmail) {
                            var listordered = listranking.Where(z => z.As<RankingPart>().ContentIdentifier == Ci.Id).OrderByDescending(y => y.Point);
                            List<RankingTemplateVM> rkt = new List<RankingTemplateVM>();
                            foreach (RankingPart cirkt in listordered) {
                                RankingTemplateVM tmp = new RankingTemplateVM();
                                tmp.Point = cirkt.Point;
                                tmp.ContentIdentifier = cirkt.ContentIdentifier;
                                tmp.Device = cirkt.Device;
                                tmp.Identifier = cirkt.Identifier;
                                tmp.name = getusername(cirkt.User_Id);
                                tmp.UsernameGameCenter = cirkt.UsernameGameCenter;
                                tmp.AccessSecured = cirkt.AccessSecured;
                                tmp.RegistrationDate = cirkt.RegistrationDate;
                                rkt.Add(tmp);
                            }

                            if (SendEmail(Ci, rkt)) {
                                // logger
                            }
                        }
                        _workflowManager.TriggerEvent("GameRankingSubmitted", Ci, () => new Dictionary<string, object> { { "Content", Ci } });
                        gp.workflowfired = true;
                    }
                }
            }
            return true;
        }

        public bool SendTemplatedEmailRanking(Int32 gameID) {
            var query = _orchardServices.ContentManager.Query();
            var list = query.ForPart<GamePart>().Where<GamePartRecord>(x => x.Id == gameID).List();
            GamePart gp = list.FirstOrDefault();
            if (gp == null) //check if there is actually a game with the given ID (proper usage should make it so this is never true)
                return false;
            //we do not do checks on whether this email was scheduled, because this method is also used to send it out of synch with the tasks
            ContentItem Ci = gp.ContentItem;

            var session = _transactionManager.GetSession();// _sessionLocator.For(typeof(RankingPartRecord));
            var generalRankQuery = QueryForRanking(gameId: gameID, page: 1, pageSize: 10);
            //.GetExecutableQueryOver(session)
            //.TransformUsing(new AliasToBeanVMFromRecord(ConvertFromDBData)) //TODO: use transformers. Start from the IResultTransformer in RankingTemplateVM.cs. Also http://blog.andrewawhitaker.com/queryover-series
            //                                                                //.Future();
            //.Future<RankingTemplateVM>();
            var appleRankQuery = QueryForRanking(gameId: gameID, device: TipoDispositivo.Apple.ToString(), page: 1, pageSize: 10);
            //.GetExecutableQueryOver(session)
            //.TransformUsing(new AliasToBeanVMFromRecord(ConvertFromDBData))
            ////.Future();
            //.Future<RankingTemplateVM>();
            var androidRankQuery = QueryForRanking(gameId: gameID, device: TipoDispositivo.Android.ToString(), page: 1, pageSize: 10);
            //.GetExecutableQueryOver(session)
            //.TransformUsing(new AliasToBeanVMFromRecord(ConvertFromDBData))
            ////.Future();
            //.Future<RankingTemplateVM>();
            var windowsRankQuery = QueryForRanking(gameId: gameID, device: TipoDispositivo.WindowsMobile.ToString(), page: 1, pageSize: 10);
            //.GetExecutableQueryOver(session)
            //.TransformUsing(new AliasToBeanVMFromRecord(ConvertFromDBData))
            ////.Future();
            //.Future<RankingTemplateVM>();

            //List<RankingTemplateVM> generalRank = new List<RankingTemplateVM>();
            //foreach (RankingPartRecord obj in generalRankQuery) { //Trasformers would allow getting rid of these iterations
            //    generalRank.Add(ConvertFromDBData(obj));
            //}
            //List<RankingTemplateVM> appleRank = new List<RankingTemplateVM>();
            //foreach (RankingPartRecord obj in appleRankQuery) {
            //    appleRank.Add(ConvertFromDBData(obj));
            //}
            //List<RankingTemplateVM> androidRank = new List<RankingTemplateVM>();
            //foreach (RankingPartRecord obj in androidRankQuery) {
            //    androidRank.Add(ConvertFromDBData(obj));
            //}
            //List<RankingTemplateVM> windowsRank = new List<RankingTemplateVM>();
            //foreach (RankingPartRecord obj in windowsRankQuery) {
            //    windowsRank.Add(ConvertFromDBData(obj));
            //}

            //if (SendEmail(Ci, generalRank, appleRank, androidRank, windowsRank)) {
            if (SendEmail(Ci, generalRankQuery, appleRankQuery.ToList(), androidRankQuery.ToList(), windowsRankQuery.ToList())) {
                _workflowManager.TriggerEvent("GameRankingSubmitted", Ci, () => new Dictionary<string, object> { { "Content", Ci } });
                gp.workflowfired = true;
                return true;
            }
            return false;
        }

        private bool SendEmail(ContentItem Ci, List<RankingTemplateVM> rkt) {
            //_orchardServices.WorkContext.TryResolve<ISmtpChannel>(out _messageManager);
            string emailRecipe = Ci.As<GamePart>().Settings.GetModel<GamePartSettingVM>().EmailRecipe;
            if (emailRecipe != "") {
                var editModel = new Dictionary<string, object>();
                editModel.Add("Content", Ci);
                editModel.Add("ListRanking", rkt);
                ParseTemplateContext ptc = new ParseTemplateContext();
                ptc.Model = editModel;
                int templateid = Ci.As<GamePart>().Settings.GetModel<GamePartSettingVM>().Template;
                TemplatePart TemplateToUse = _orchardServices.ContentManager.Get(templateid).As<TemplatePart>();
                string testohtml;
                if (TemplateToUse != null) {
                    testohtml = _templateService.ParseTemplate(TemplateToUse, ptc);
                    var datiCI = Ci.Record;
                    var data = new Dictionary<string, object>();
                    data.Add("Subject", "Game Ranking");
                    data.Add("Body", testohtml);
                    //_messageManager.Send(new string[] { emailRecipe }, "ModuleRankingEmail", "email", data);
                    data.Add("Recipients", emailRecipe);
                    _messageManager.Value.Process(data);
                    return true;
                }
                else { // Nessun template selezionato non mando una mail e ritorno false, mail non inviata
                    return false;
                }
            }
            else
                return false;
        }

        //method to send a single email with separate rankings for the different platforms
        private bool SendEmail(ContentItem Ci, List<RankingTemplateVM> generalRank, List<RankingTemplateVM> appleRank,
            List<RankingTemplateVM> androidRank, List<RankingTemplateVM> windowsRank) {
            //_orchardServices.WorkContext.TryResolve<ISmtpChannel>(out _messageManager);
            string emailRecipe = Ci.As<GamePart>().Settings.GetModel<GamePartSettingVM>().EmailRecipe;
            if (emailRecipe != "") {
                var editModel = new Dictionary<string, object>();
                editModel.Add("Content", Ci);
                editModel.Add("ListRanking", generalRank);
                editModel.Add("GeneralRanking", generalRank);
                editModel.Add("AppleRanking", appleRank);
                editModel.Add("AndroidRanking", androidRank);
                editModel.Add("WindowsRanking", windowsRank);
                ParseTemplateContext ptc = new ParseTemplateContext();
                ptc.Model = editModel;
                int templateid = Ci.As<GamePart>().Settings.GetModel<GamePartSettingVM>().Template;
                TemplatePart TemplateToUse = _orchardServices.ContentManager.Get(templateid).As<TemplatePart>();
                string testohtml;
                if (TemplateToUse != null) {
                    testohtml = _templateService.ParseTemplate(TemplateToUse, ptc);
                    var datiCI = Ci.Record;
                    var data = new Dictionary<string, object>();
                    data.Add("Subject", "Game Ranking");
                    data.Add("Body", testohtml);
                    //_messageManager.Send(new string[] { emailRecipe }, "ModuleRankingEmail", "email", data);
                    data.Add("Recipients", emailRecipe);
                    _messageManager.Value.Process(data);
                    return true;
                }
                else { // Nessun template selezionato non mando una mail e ritorno false, mail non inviata
                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Create a task to schedule sending a summary email with the game results after the game has ended. the update of the task, in case the game has been modified,
        /// is done by deleting the existing task and creating a new one. The two parameters can for exaple be extracted from a <type>GamePart</type> called part as 
        /// gameID = part.part.Record.Id; and timeGameEnd = ((dynamic)part.ContentItem).ActivityPart.DateTimeEnd;
        /// </summary>
        /// <param name="gameID">The unique <type>Int32</type> identifier of the game.</param>
        /// <param name="timeGameEnd">The <type>DateTime</type> object telling when the game will end.</param>
        public void ScheduleEmailTask(Int32 gameID, DateTime timeGameEnd) {
            UnscheduleEmailTask(gameID);
            string taskTypeStr = Laser.Orchard.Questionnaires.Handlers.ScheduledTaskHandler.TaskType + " " + gameID.ToString();
            DateTime taskDate = timeGameEnd.AddMinutes(5);
            //Local time to UTC conversion
            //taskDate = (DateTime)( _dateServices.ConvertFromLocal(taskDate.ToLocalTime()));
            taskDate = (DateTime)(_dateServices.ConvertFromLocalizedString(_dateLocalization.WriteDateLocalized(taskDate), _dateLocalization.WriteTimeLocalized(taskDate)));
            //taskDate = taskDate.Subtract(new TimeSpan ( 2, 0, 0 )); //subtract two hours
            taskDate = taskDate.ToUniversalTime(); //this problay does nothing
            _taskManager.CreateTask(taskTypeStr, taskDate, null);
        }

        /// <summary>
        /// Check whether an email task exists for a game identiied by the given Id, and destroy it if that is the case.
        /// </summary>
        /// <param name="gameID">The unique <type>Int32</type> identifier of the game.</param>
        public void UnscheduleEmailTask(Int32 gameID) {
            string taskTypeStr = Laser.Orchard.Questionnaires.Handlers.ScheduledTaskHandler.TaskType + " " + gameID.ToString();
            var tasks = _taskManager.GetTasks(taskTypeStr);
            foreach (var ta in tasks) {
                //if we are here, it means the task ta exists with the same game id as the current game
                //hence we should update the task. We fall in this condition when we are updating the information for a game.
                _taskManager.DeleteTasks(ta.ContentItem); //maybe
            }
        }

        /// <summary>
        /// This method generates a <type>QueryOver</type> object that can be used to invoke a query using 
        /// .GetExecutableQueryOver(session). In particular, the queries generated from this method are used to get 
        /// pages of rankings from the DB. They can be executed calling GetExecutableQueryOver(session).List(), or 
        /// GetExecutableQueryOver(session).Future&lt;RankingPartRecord&gt;()
        /// </summary>
        /// <param name="gameId">The Id of the game for which we want the ranking table.</param>
        /// <param name="device">A string representing the tipe of device for which we want the ranking. This is obtained as a 
        /// TipoDispositivo.value.ToString(). Any other string causes this method to default to returning the general ranking.</param>
        /// <param name="page">The page of the results in the ranking. This is 1-based (the top-most results are on page 1).</param>
        /// <param name="pageSize">The size of a page of results, corresponding to the maximum number of socres to return.</param>
        /// <param name="Ascending">A flag to determine the sorting order for the scores: <value>true</value> order by ascending score; 
        /// <value>false</value> order by descending score.</param>
        /// <returns>A <type>QueryOver</type> object built from the parameters and that can be used to execute queries on the DB.</returns>
        public List<RankingTemplateVM> QueryForRanking(
            Int32 gameId, string device = "General", int page = 1, int pageSize = 10, bool Ascending = false) {
            var session = _transactionManager.GetSession();
            RankingPartRecord rprAlias = null; //used as alias in correlated subqueries
            QueryOver<RankingPartRecord, RankingPartRecord> qoRpr = QueryOver.Of<RankingPartRecord>(() => rprAlias)
                .Where(t => t.ContentIdentifier == gameId);
            qoRpr = CheckDeviceType(qoRpr, device);

            QueryOver<RankingPartRecord, RankingPartRecord> subPoints = QueryOver.Of<RankingPartRecord>()
                .Where(t => t.ContentIdentifier == gameId);
            subPoints = CheckDeviceType(subPoints, device);
            subPoints = subPoints
                .Where(s => s.Identifier == rprAlias.Identifier)
                .SelectList(li => li
                    .SelectMax(rec => rec.Point)
                );

            QueryOver<RankingPartRecord, RankingPartRecord> subDate = QueryOver.Of<RankingPartRecord>()
                .Where(t => t.ContentIdentifier == gameId);
            subDate = CheckDeviceType(subDate, device);
            subDate = subDate
                .Where(s => s.Identifier == rprAlias.Identifier)
                .And(s => s.Point == rprAlias.Point)
                .SelectList(li => li
                    .SelectMin(rec => rec.RegistrationDate)
                );

            qoRpr = qoRpr
                .WithSubquery.WhereProperty(rpr => rpr.Point).Eq(subPoints)
                .WithSubquery.WhereProperty(rpr => rpr.RegistrationDate).In(subDate);

            qoRpr = CheckSortDirection(qoRpr, Ascending);

            var rank = (qoRpr.Skip(pageSize * (page - 1)).Take(pageSize));


            return rank.GetExecutableQueryOver(session).List().Select(x => ConvertFromDBData(x)).ToList();


        }

        /// <summary>
        /// Verifies what kind of device we want to query for, and eventually adds the corresponding query.
        /// </summary>
        /// <param name="qoRpr">The <type>QueryOver</type> object we are building the query on.</param>
        /// <param name="devType">A <type>string</type> containing the name of the device type.</param>
        /// <returns>A <type>QueryOver</type> object built from the one passed as parameter by adding the device query.</returns>
        private QueryOver<RankingPartRecord, RankingPartRecord> CheckDeviceType(QueryOver<RankingPartRecord, RankingPartRecord> qoRpr, string devType) {
            if (devType == TipoDispositivo.Apple.ToString())
                return qoRpr.Where(t => t.Device == TipoDispositivo.Apple);
            else if (devType == TipoDispositivo.Android.ToString())
                return qoRpr.Where(t => t.Device == TipoDispositivo.Android);
            else if (devType == TipoDispositivo.WindowsMobile.ToString())
                return qoRpr.Where(t => t.Device == TipoDispositivo.WindowsMobile);
            return qoRpr; //general case
        }
        /// <summary>
        /// Verifies the sort direction required
        /// </summary>
        /// <param name="qoRpr">The <type>QueryOver</type> object we are building the query on.</param>
        /// <param name="Ascending">A <type>bool</type> telling the sorting direction.</param>
        /// <returns>A <type>QueryOver</type> object built from the one passed as parameter by adding the sort query.</returns>
        private QueryOver<RankingPartRecord, RankingPartRecord> CheckSortDirection(QueryOver<RankingPartRecord, RankingPartRecord> qoRpr, bool Ascending) {
            if (Ascending)
                return qoRpr.OrderBy(r => r.Point).Asc;
            else
                return qoRpr.OrderBy(r => r.Point).Desc;
        }

        /// <summary>
        /// Method filling up a <type>RanKingTemplateVM</type> object from a <type>RankingPartRecordIntermediate</type> object. The <type>RanKingTemplateVM</type>
        /// object has a filed of type <type>TipoDispositivo</type>. The corresponding record is read as a <type>string</type> from the DB. For this reason we use
        /// an intermediate class to store what we read from the db, and then convert it into the proper type.
        /// </summary>
        /// <param name="rpri">A <type>RankingPartRecordIntermediate</type> object, corresponding to a record as read from the DB.</param>
        /// <returns>A <type>RanKingTemplateVM</type> object, corresponding to the record we read from the DB.</returns>
        private RankingTemplateVM ConvertFromDBData(RankingPartRecordIntermediate rpri) {
            RankingTemplateVM ret = new RankingTemplateVM();
            ret.Point = rpri.Point;
            ret.Identifier = rpri.Identifier;
            ret.UsernameGameCenter = rpri.UsernameGameCenter;
            if (rpri.Device == TipoDispositivo.Android.ToString())
                ret.Device = TipoDispositivo.Android;
            else if (rpri.Device == TipoDispositivo.Apple.ToString())
                ret.Device = TipoDispositivo.Apple;
            else if (rpri.Device == TipoDispositivo.WindowsMobile.ToString())
                ret.Device = TipoDispositivo.WindowsMobile;
            ret.ContentIdentifier = rpri.ContentIdentifier;
            ret.name = getusername(rpri.User_Id);
            ret.AccessSecured = rpri.AccessSecured;
            ret.RegistrationDate = rpri.RegistrationDate;
            return ret;
        }
        /// <summary>
        /// Method filling up a <type>RanKingTemplateVM</type> object from a <type>RankingPartRecord</type> object. The <type>RanKingTemplateVM</type>
        /// object has a filed of type <type>TipoDispositivo</type>. The corresponding record is read as a <type>string</type> from the DB. For this reason we use
        /// an intermediate class to store what we read from the db, and then convert it into the proper type.
        /// </summary>
        /// <param name="rpr">A <type>RankingPartRecord</type> object, corresponding to a record as read from the DB.</param>
        /// <returns>A <type>RanKingTemplateVM</type> object, corresponding to the record we read from the DB.</returns>
        private RankingTemplateVM ConvertFromDBData(RankingPartRecord rpr) {
            RankingTemplateVM ret = new RankingTemplateVM();
            ret.Point = rpr.Point;
            ret.Identifier = rpr.Identifier;
            ret.UsernameGameCenter = rpr.UsernameGameCenter;
            ret.Device = rpr.Device;
            ret.ContentIdentifier = rpr.ContentIdentifier;
            ret.name = getusername(rpr.User_Id);
            ret.AccessSecured = rpr.AccessSecured;
            ret.RegistrationDate = rpr.RegistrationDate;
            return ret;
        }

        private HashAlgorithm _hashAlgorithm;
        private HashAlgorithm GetHashAlgorithm() {
            if (_hashAlgorithm == null) {
                _hashAlgorithm = SHA256.Create();
            }
            return _hashAlgorithm;
        }
        private string GetHash(string input) {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = GetHashAlgorithm()
                .ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++) {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        public bool Save(QuestionnaireWithResultsViewModel editModel, IUser currentUser, string SessionID) {
            bool result = false;
            var questionnaireModuleSettings = _orchardServices.WorkContext
                .CurrentSite.As<QuestionnaireModuleSettingsPart>();
            var part = _orchardServices.ContentManager
                .Get<QuestionnairePart>(editModel.Id);
            var questionnairePartSettings = part
                .Settings.GetModel<QuestionnairesPartSettingVM>();
            var content = _orchardServices.ContentManager.Get(editModel.Id);
            bool exit = false;

            if (String.IsNullOrWhiteSpace(editModel.Context)) { //fallback into questionnaire part settings context if context is null
                // Tokenize Settings Context
                editModel.Context = _tokenizer
                    .Replace(questionnairePartSettings.QuestionnaireContext,
                        new Dictionary<string, object> { { "Content", content } });
            }
            if (editModel.Context.Length > 255) {// limits context to 255 chars
                editModel.Context = editModel.Context.Substring(0, 255);
            }
            // The Disposable flag is used for scenarios where a user is not allowed to answer to
            // a questionnaire more than once. Checking the UserAnswersRecord table is not enough
            // to ensure that, because when none of the questions have mandatory answers (i.e the
            // user may be submitting a valid form that has no answer to any question). However,
            // UserAnswerInstanceRecord has a record for each single time a user submitted a form
            // to answer a questionnaire, even when no answer was given. Issue with that is that
            // for questionnaires older than its introduction, we should still try the original
            // query, since there cannot be an UserAnswerInstanceRecord there.
            if (questionnaireModuleSettings.Disposable) {
                if (currentUser != null) {
                    var answeredInstance = GetMostRecentInstanceId(part, currentUser, editModel.Context);
                    if (!string.IsNullOrWhiteSpace(answeredInstance)
                        || _repositoryUserAnswer
                            .Fetch(x => x.User_Id == currentUser.Id
                                && x.QuestionnairePartRecord_Id == editModel.Id
                                && x.Context == editModel.Context).Count() > 0) {
                        exit = true;
                    }
                } else { // anonymous user => check SessionID
                    var answeredInstance = !string.IsNullOrWhiteSpace(SessionID)
                        ? GetMostRecentInstanceId(part, SessionID, editModel.Context)
                        : string.Empty;
                    if (!string.IsNullOrWhiteSpace(answeredInstance)
                        || _repositoryUserAnswer
                            .Fetch(x => x.SessionID == SessionID
                                && x.QuestionnairePartRecord_Id == editModel.Id
                                && x.Context == editModel.Context).Count() > 0) {
                        exit = true;
                    }
                }
            }
            if (!exit) {
                // here we loop to actually generate the records for the answers given
                var dateTimeNow = DateTime.UtcNow;
                var instanceId = GetHash(
                    (currentUser != null ? currentUser.Id.ToString() : SessionID)
                    + editModel.Id.ToString() + dateTimeNow.ToString());
                var instanceRecord = new UserAnswerInstanceRecord() {
                    QuestionnairePartRecord_Id = editModel.Id,
                    User_Id = (currentUser == null || questionnairePartSettings.ForceAnonymous) 
                        ? 0 : currentUser.Id,
                    SessionID = SessionID,
                    Context = editModel.Context,
                    AnswerDate = dateTimeNow,
                    AnswerInstance = instanceId
                };
                editModel.AnswersInstance = instanceId;
                // get the answer text where it's not open
                var answerIds = editModel.QuestionsWithResults
                    // for all questions that don't have open Answer
                    .Where(q => q.QuestionType == QuestionType.SingleChoice
                        || q.QuestionType == QuestionType.MultiChoice)
                    // select the answer ids
                    .SelectMany(q => q.QuestionType == QuestionType.SingleChoice
                        // we make a list of this to merge it with the others
                        ? new List<int>() { q.SingleChoiceAnswer }
                        // all selected answers
                        : q.AnswersWithResult.Where(a => a.Answered).Select(a => a.Id))
                    // Distinct is probably not neeeded
                    .Distinct();
                // get the text from db (candidate for caching)
                var allSelectedAnswers = _questionAnswerRepositoryService
                    .AnswersRepository()
                    .Fetch(ar => answerIds.Contains(ar.Id));
                // a dictionary is easy to get stuff out of
                var answerTexts = allSelectedAnswers
                    .ToDictionary(ar => ar.Id);
                foreach (var q in editModel.QuestionsWithResults) {
                    Action<UserAnswersRecord> CreationAction = (uar) => {
                        uar.QuestionText = q.Question;
                        uar.QuestionRecord_Id = q.Id;
                        uar.User_Id = (currentUser == null || questionnairePartSettings.ForceAnonymous) ? 0 : currentUser.Id;
                        uar.QuestionnairePartRecord_Id = editModel.Id;
                        uar.SessionID = SessionID;
                        uar.Context = editModel.Context;
                        uar.AnswerInstance = instanceId;
                        uar.QuestionType = q.QuestionType;
                        // I really wish I could create several records in a single call
                        CreateUserAnswers(uar);
                    };
                    if (q.QuestionType == QuestionType.OpenAnswer) {
                        if (!String.IsNullOrWhiteSpace(q.OpenAnswerAnswerText)) {
                            var userAnswer = new UserAnswersRecord();
                            userAnswer.AnswerText = q.OpenAnswerAnswerText;
                            CreationAction(userAnswer);
                        }
                    }
                    else if (q.QuestionType == QuestionType.SingleChoice) {
                        if (q.SingleChoiceAnswer > 0) {
                            var userAnswer = new UserAnswersRecord();
                            userAnswer.AnswerRecord_Id = q.SingleChoiceAnswer;
                            userAnswer.AnswerText = answerTexts[q.SingleChoiceAnswer].Answer;//GetAnswer(q.SingleChoiceAnswer).Answer;
                            CreationAction(userAnswer);
                            // for a single choice answer we aren't carrying the answer's text
                            // in the view model. This would prevent us from using it in workflows.
                            // So we go ahead and add it to the list of answers for this question.
                            if (q.AnswersWithResult == null) {
                                q.AnswersWithResult = new List<AnswerWithResultViewModel>();
                            }
                            q.AnswersWithResult.Add(new AnswerWithResultViewModel() {
                                Id = q.SingleChoiceAnswer,
                                Answered = true,
                                AnswerText = answerTexts[q.SingleChoiceAnswer].Answer
                            });
                        }
                    }
                    else if (q.QuestionType == QuestionType.MultiChoice) {
                        var answerList = q.AnswersWithResult.Where(w => w.Answered);
                        foreach (var a in answerList) {
                            var userAnswer = new UserAnswersRecord();
                            userAnswer.AnswerRecord_Id = a.Id;
                            userAnswer.AnswerText = answerTexts[a.Id].Answer;// GetAnswer(a.Id).Answer;
                            CreationAction(userAnswer);
                        }
                    }
                }
                // create the instance record
                _repositoryinstanceRecords.Create(instanceRecord);
                // fire events
                _workflowManager.TriggerEvent(
                    "QuestionnaireSubmitted",
                    content, () => new Dictionary<string, object> {
                        { "Content", content },
                        { "QuestionnaireContext", editModel.Context },
                        { "QuestionnaireWithResults", editModel }
                    });
                result = true;
            }
            return result;
        }

        public QuestionnaireWithResultsViewModel GetMostRecentAnswersInstance(
            QuestionnairePart part, IUser user, string context = null) {
            if (part == null) {
                throw new ArgumentNullException("part");
            }
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            var instanceId = GetMostRecentInstanceId(part, user, context);
            if (instanceId == null) {
                // never answered
                return null;
            }
            if (string.IsNullOrWhiteSpace(instanceId)) {
                throw new InvalidOperationException("The latest answers to this question for the user were recorded before the introduction of answers' instances.");
            }
            return GetAnswersInstance(instanceId, part, user);
        }
        public string GetMostRecentInstanceId(QuestionnairePart part, IUser user, string context = null) {
            if (part == null) {
                throw new ArgumentNullException("part");
            }
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            
            string instanceId = null;
            if (context == null) {
                instanceId = _repositoryinstanceRecords
                    .Table
                    .Where(uair => uair.User_Id == user.Id
                        && uair.QuestionnairePartRecord_Id == part.Id)
                    .OrderByDescending(uair => uair.AnswerDate)
                    .Select(uair => uair.AnswerInstance)
                    .FirstOrDefault();
            } else {
                instanceId = _repositoryinstanceRecords
                    .Table
                    .Where(uair => uair.User_Id == user.Id
                        && uair.QuestionnairePartRecord_Id == part.Id
                        && uair.Context == context)
                    .OrderByDescending(uair => uair.AnswerDate)
                    .Select(uair => uair.AnswerInstance)
                    .FirstOrDefault();
            }
            return instanceId;
        }
        public string GetMostRecentInstanceId(QuestionnairePart part, string sessionId, string context = null) {
            if (part == null) {
                throw new ArgumentNullException("part");
            }
            if (string.IsNullOrWhiteSpace(sessionId)) {
                throw new ArgumentNullException("sessionId");
            }
            string instanceId = null;
            if (context == null) {
                instanceId = _repositoryinstanceRecords
                    .Table
                    .Where(uair => uair.SessionID == sessionId
                        && uair.QuestionnairePartRecord_Id == part.Id)
                    .OrderByDescending(uair => uair.AnswerDate)
                    .Select(uair => uair.AnswerInstance)
                    .FirstOrDefault();
            } else {
                instanceId = _repositoryinstanceRecords
                    .Table
                    .Where(uair => uair.SessionID == sessionId
                        && uair.QuestionnairePartRecord_Id == part.Id
                        && uair.Context == context)
                    .OrderByDescending(uair => uair.AnswerDate)
                    .Select(uair => uair.AnswerInstance)
                    .FirstOrDefault();
            }
            return instanceId;
        }
        public QuestionnaireWithResultsViewModel GetAnswersInstance(
            string instance, QuestionnairePart part, IUser user) {
            if (part == null) {
                throw new ArgumentNullException("part");
            }
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            if (string.IsNullOrWhiteSpace(instance)) {
                throw new ArgumentNullException("instance");
            }

            var viewModel = new QuestionnaireWithResultsViewModel() {
                Id = part.Id,
                AnswersInstance = instance
            };
            // get all answers belonging to this instance
            var userAnswers = _repositoryUserAnswer
                .Fetch(uar => uar.User_Id == user.Id
                    && uar.QuestionnairePartRecord_Id == part.Id
                    && uar.AnswerInstance == instance);
            if (!userAnswers.Any()) {
                // never answered (or nothing found for that instance)
                return null;
            }
            // based on the answers we fetched, do the inverse of what is done when saving
            // them to build the view model.
            var answerGroups = userAnswers.GroupBy(uar => uar.QuestionRecord_Id);
            var answerResults = new List<AnswerWithResultViewModel>();
            foreach (var group in answerGroups) {
                var answer = group.First();
                if (answer.QuestionType == QuestionType.MultiChoice) {
                    answerResults = group.Select(uar => new AnswerWithResultViewModel() {
                        Id = uar.AnswerRecord_Id ?? 0,
                        AnswerText = uar.AnswerText,
                        QuestionRecord_Id = uar.QuestionRecord_Id,
                        Answered = true //if the the record is there means the user answered with that option
                    }).ToList();
                }
                viewModel.QuestionsWithResults
                    .Add(new QuestionWithResultsViewModel() {
                        Id = answer.QuestionRecord_Id,
                        Question = answer.QuestionText,
                        QuestionType = answer.QuestionType,
                        SingleChoiceAnswer = answer.QuestionType == QuestionType.SingleChoice
                            ? answer.AnswerRecord_Id.Value : 0,
                        OpenAnswerAnswerText = answer.QuestionType == QuestionType.OpenAnswer
                            ? answer.AnswerText : string.Empty,
                        QuestionnairePartRecord_Id = part.Id,
                        AnswersWithResult = answerResults
                    });
            }
            return viewModel;
        }

        public void UpdateForContentItem(ContentItem item, QuestionnaireEditModel partEditModel) {
            try {
                var partRecord = item.As<QuestionnairePart>().Record;
                var part = item.As<QuestionnairePart>();
                var PartID = partRecord.Id;
                var storedQuestions = part.Questions.ToList(); // ensure list of questions is in memory
                var storedAnswers = storedQuestions.SelectMany(x => x.Answers).ToList();// ensure list of answers is in memory

                Mapper.Initialize(cfg => {
                    cfg.CreateMap<QuestionnaireEditModel, QuestionnairePart>().ForMember(dest => dest.Questions, opt => opt.Ignore());
                    cfg.CreateMap<QuestionEditModel, QuestionRecord>().ForMember(dest => dest.Answers, opt => opt.Ignore());
                });
                Mapper.Map<QuestionnaireEditModel, QuestionnairePart>(partEditModel, part);
                var mappingA = new Dictionary<string, string>();

                // Insert, Update and Delete questions
                foreach (var quest in partEditModel.Questions) {
                    QuestionRecord questionRecord;
                    if (!string.IsNullOrWhiteSpace(quest.GUIdentifier)) {
                        questionRecord = storedQuestions.SingleOrDefault(x =>
                        x.GUIdentifier == quest.GUIdentifier) ?? new QuestionRecord(); //Get data of question by Identifier or create a new question
                    }
                    else {
                        questionRecord = storedQuestions.SingleOrDefault(x =>
                        x.Id == quest.Id) ?? new QuestionRecord(); //Get data of question by Id or create a new question
                    }
                    var originalQuestionRecordId = questionRecord.Id; // 0 if new, a valid Id if get from DB
                    if (quest.Delete) {
                        try {
                            foreach (var answer in questionRecord.Answers) {
                                if (answer.Id > 0) {
                                    _questionAnswerRepositoryService.DeleteAnswer(answer.Id);
                                }
                            }
                            if (quest.Id > 0) {
                                _questionAnswerRepositoryService.DeleteQuestion(quest.Id);
                            }
                        }
                        catch (Exception ex) {
                            throw new Exception("quest.Delete\r\n" + ex.Message);
                        }
                    }
                    else {
                        Mapper.Initialize(cfg => {
                            cfg.CreateMap<QuestionEditModel, QuestionRecord>().ForMember(dest => dest.Answers, opt => opt.Ignore());
                        });
                        Mapper.Map<QuestionEditModel, QuestionRecord>(quest, questionRecord);
                        questionRecord.QuestionnairePartRecord_Id = PartID;
                        if (questionRecord.Id == 0 && originalQuestionRecordId > 0) {
                            questionRecord.Id = originalQuestionRecordId;
                        }
                        _questionAnswerRepositoryService.UpdateQuestion(questionRecord);
                        var recordQuestionID = questionRecord.Id;
                        try {
                            foreach (var answer in quest.Answers) { ///Insert, Update and delete Answer
                                if (answer.Delete) {
                                    if (answer.Id > 0) {
                                        _questionAnswerRepositoryService.DeleteAnswer(answer.Id);
                                    }
                                }
                                else {
                                    AnswerRecord answerRecord;
                                    if (!string.IsNullOrWhiteSpace(answer.GUIdentifier)) {
                                        answerRecord = storedAnswers.SingleOrDefault(x => x.GUIdentifier == answer.GUIdentifier) ?? new AnswerRecord(); //Get data of answer by Identifier or create a new answer
                                    }
                                    else {
                                        answerRecord = storedAnswers.SingleOrDefault(x => x.Id == answer.Id) ?? new AnswerRecord(); //Get data of answer by Identifier or create a new answer
                                    }
                                    var originalAnswerRecordId = answerRecord.Id; // 0 if new, a valid Id if get from DB

                                    Mapper.Initialize(cfg => {
                                        cfg.CreateMap<AnswerEditModel, AnswerRecord>();
                                    });
                                    Mapper.Map<AnswerEditModel, AnswerRecord>(answer, answerRecord);
                                    answerRecord.QuestionRecord_Id = recordQuestionID;
                                    if (answerRecord.Id == 0 && originalAnswerRecordId > 0) {
                                        answerRecord.Id = originalAnswerRecordId;
                                    }
                                    _questionAnswerRepositoryService.UpdateAnswer(answerRecord);
                                    if (answer.OriginalId > 0) {
                                        mappingA[answer.OriginalId.ToString()] = answerRecord.Id.ToString();
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            throw new Exception("quest.Update\r\n" + ex.Message);
                        }
                        try {
                            // fix condtions if necessary
                            if (mappingA.Count() > 0 && questionRecord.Condition != null) {
                                Regex re = new Regex(@"[0-9]+", RegexOptions.Compiled);
                                questionRecord.Condition = re.Replace(questionRecord.Condition, match => mappingA[match.Value] != null ? mappingA[match.Value].ToString() : match.Value);
                                _questionAnswerRepositoryService.UpdateQuestion(questionRecord);
                                re = null;
                            }
                        }
                        catch (Exception ex) {
                            throw new Exception("quest.CorrezzioneCondizioni\r\n" + ex.Message);
                        }

                    }
                }
            }
            catch (Exception ex) {
                throw new Exception("quest.UpdateTotale\r\n" + ex.Message);
            }
        }

        public QuestionnaireEditModel BuildEditModelForQuestionnairePart(QuestionnairePart part) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<QuestionRecord, QuestionEditModel>().ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers.OrderBy(o => o.Position)));
                cfg.CreateMap<QuestionnairePart, QuestionnaireEditModel>().ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));
                cfg.CreateMap<AnswerRecord, AnswerEditModel>();
            });
            part.QuestionsToDisplay = null; // ensure we actually use the Questions from the record (see how the Questions property is defined in the part)
            var editModel = Mapper.Map<QuestionnaireEditModel>(part);
            return (editModel);
        }

        public QuestionnaireViewModel BuildViewModelForQuestionnairePart(QuestionnairePart part) {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<QuestionRecord, QuestionEditModel>().ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers.OrderBy(o => o.Position)));
                cfg.CreateMap<QuestionnairePart, QuestionnaireEditModel>().ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));
                cfg.CreateMap<AnswerRecord, AnswerEditModel>();
            });
            var viewModel = Mapper.Map<QuestionnaireViewModel>(part);
            return (viewModel);
        }

        #region [ FrontEnd Model ]

        public QuestionnaireWithResultsViewModel BuildViewModelWithResultsForQuestionnairePart(QuestionnairePart part) {
            // Mapper.CreateMap<AnswerRecord, AnswerWithResultViewModel>();

            if (part.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionsSortedRandomlyNumber > 0) {
                if (part.Settings.GetModel<QuestionnairesPartSettingVM>().RandomResponse)
                    Mapper.Initialize(cfg => {
                        cfg.CreateMap<AnswerRecord, AnswerWithResultViewModel>();
                        cfg.CreateMap<QuestionRecord, QuestionWithResultsViewModel>().ForMember(dest => dest.AnswersWithResult, opt => opt.MapFrom(src => src.Answers.Where(w => w.Published)));
                        cfg.CreateMap<QuestionnairePart, QuestionnaireWithResultsViewModel>().ForMember(dest => dest.QuestionsWithResults, opt => opt.MapFrom(src => src.Questions.Where(w => w.Published)));
                    });
                else
                    Mapper.Initialize(cfg => {
                        cfg.CreateMap<AnswerRecord, AnswerWithResultViewModel>();
                        cfg.CreateMap<QuestionRecord, QuestionWithResultsViewModel>().ForMember(dest => dest.AnswersWithResult, opt => opt.MapFrom(src => src.Answers.Where(w => w.Published)));
                        cfg.CreateMap<QuestionnairePart, QuestionnaireWithResultsViewModel>().ForMember(dest => dest.QuestionsWithResults, opt => opt.MapFrom(src => src.Questions.Where(w => w.Published).OrderBy(o => o.Position)));
                    });
            }
            else {
                if (part.Settings.GetModel<QuestionnairesPartSettingVM>().RandomResponse)
                    Mapper.Initialize(cfg => {
                        cfg.CreateMap<AnswerRecord, AnswerWithResultViewModel>();
                        cfg.CreateMap<QuestionRecord, QuestionWithResultsViewModel>().ForMember(dest => dest.AnswersWithResult, opt => opt.MapFrom(src => src.Answers.Where(w => w.Published).OrderBy(o => o.Position)));
                        cfg.CreateMap<QuestionnairePart, QuestionnaireWithResultsViewModel>().ForMember(dest => dest.QuestionsWithResults, opt => opt.MapFrom(src => src.Questions.Where(w => w.Published)));
                    });
                else
                    Mapper.Initialize(cfg => {
                        cfg.CreateMap<AnswerRecord, AnswerWithResultViewModel>();
                        cfg.CreateMap<QuestionRecord, QuestionWithResultsViewModel>().ForMember(dest => dest.AnswersWithResult, opt => opt.MapFrom(src => src.Answers.Where(w => w.Published).OrderBy(o => o.Position)));
                        cfg.CreateMap<QuestionnairePart, QuestionnaireWithResultsViewModel>().ForMember(dest => dest.QuestionsWithResults, opt => opt.MapFrom<IOrderedEnumerable<QuestionRecord>>(src => src.Questions.Where(w => w.Published).OrderBy(o => o.Position)));
                    });
            }
            var viewModel = Mapper.Map<QuestionnaireWithResultsViewModel>(part);
            return (viewModel);
        }

        #endregion [ FrontEnd Model ]

        public void CreateUserAnswers(UserAnswersRecord userAnswerRecord) {
            _repositoryUserAnswer.Create(userAnswerRecord);
        }

        public AnswerRecord GetAnswer(int id) {
            return (_questionAnswerRepositoryService.AnswersRepository().Get(id));
        }

        private List<ExportUserAnswersVM> GetUsersAnswers(int questionnaireId, DateTime? from = null, DateTime? to = null) {
            var answersQuery = _repositoryUserAnswer.Fetch(x => x.QuestionnairePartRecord_Id == questionnaireId);
            if (from.HasValue && from.Value > DateTime.MinValue) {
                answersQuery = answersQuery.Where(w => w.AnswerDate >= from);
            }
            if (to.HasValue && to.Value > DateTime.MinValue) {
                answersQuery = answersQuery.Where(w => w.AnswerDate <= to);
            }
            var result = answersQuery.Select(x => new ExportUserAnswersVM {
                Answer = x.AnswerText,
                Question = x.QuestionText,
                AnswerDate = x.AnswerDate,
                UserName = x.SessionID,
                UserId = x.User_Id,
                Contesto = x.Context
            }).ToList();
            int contentId = 0;
            TitlePart titlePart = null;
            UserPart usrPart = null;
            foreach (var answ in result) {
                // cerca di rendere più leggibili l'utente sostituendolo con lo username, dove è possibile
                // altrimenti lo lascia valorizzato con il SessionID
                if (answ.UserId > 0) {
                    usrPart = _orchardServices.ContentManager.Get<UserPart>(answ.UserId);
                    if (usrPart != null) {
                        answ.UserName = usrPart.UserName;
                    }
                }
                // cerca di rendere più leggibile il contesto accodando il titolo relativo, dove è possibile
                if (int.TryParse(answ.Contesto, out contentId)) {
                    titlePart = _orchardServices.ContentManager.Get<TitlePart>(contentId);
                    if (titlePart != null) {
                        answ.Contesto = string.Format("{0} {1}", contentId, titlePart.Title);
                    }
                }
            }
            return result;
        }
        public void SaveQuestionnaireUsersAnswers(int questionnaireId, DateTime? from = null, DateTime? to = null) {
            string separator = ";";
            var elenco = GetUsersAnswers(questionnaireId, from, to);
            ContentItem ci = _orchardServices.ContentManager.Get(questionnaireId);
            string fileName = String.Format("{0}-{1:yyyyMMdd}-{2:yyyyMMdd}.csv", new CommonUtils().NormalizeFileName(ci.As<TitlePart>().Title, "questionnaire", ' '), from, to);
            string filePath = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSettings.Name + @"\Export\QuestionnairesStatistics\" + fileName;
            // Creo la directory Export
            FileInfo fi = new FileInfo(filePath);
            if (fi.Directory.Parent.Exists == false) {
                System.IO.Directory.CreateDirectory(fi.Directory.Parent.FullName);
            }
            // Creo la directory Questionnaires
            if (!fi.Directory.Exists) {
                System.IO.Directory.CreateDirectory(fi.DirectoryName);
            }
            using (FileStream fStream = new FileStream(filePath, FileMode.Create)) {
                using (BinaryWriter bWriter = new BinaryWriter(fStream, Encoding.UTF8)) {
                    byte[] buffer = null;
                    string row = string.Format("\"Utente\"{0}\"Data\"{0}\"Domanda\"{0}\"Risposta\"{0}\"Contesto\"\r\n", separator);
                    buffer = Encoding.UTF8.GetBytes(row);
                    buffer = Encoding.UTF8.GetPreamble().Concat(buffer).ToArray(); //GetPreamble è necessario per aggiungere un header UTF8 riconoscibile da Excel
                    bWriter.Write(buffer);
                    foreach (var line in elenco) {
                        row = string.Format("\"{1}\"{0}\"{2:yyyy-MM-dd}\"{0}\"{3}\"{0}\"{4}\"{0}\"{5}\"\r\n",
                            separator,
                            EscapeString(line.UserName),
                            line.AnswerDate,
                            EscapeString(line.Question),
                            EscapeString(line.Answer),
                            EscapeString(line.Contesto));
                        buffer = Encoding.UTF8.GetBytes(row);
                        bWriter.Write(buffer);
                    }
                }
            }
        }
        private string EscapeString(string text) {
            return (text ?? "").Replace('\"', '\'').Replace('\n', ' ').Replace('\r', ' ');
        }
        public QuestionnaireStatsViewModel GetStats(int questionnaireId, DateTime? from = null, DateTime? to = null) {
            var questionnaireData = _orchardServices.ContentManager.Query<QuestionnairePart, QuestionnairePartRecord>(VersionOptions.Latest)
                                                       .Where(q => q.Id == questionnaireId)
                                                       .List().FirstOrDefault();
            var questionsCount = questionnaireData.Questions.Count();
            var questionnaireStatsQuery = _repositoryUserAnswer.Table
                .Join(_questionAnswerRepositoryService.QuestionsRepository().Table,
                    l => l.QuestionRecord_Id,
                    r => r.Id,
                    (l, r) => new { UserAnswers = l, Questions = r })
                .Where(w => w.Questions.QuestionnairePartRecord_Id == questionnaireId);

            if (from.HasValue && from.Value > DateTime.MinValue) {
                questionnaireStatsQuery = questionnaireStatsQuery.Where(w => w.UserAnswers.AnswerDate >= from);
            }
            if (to.HasValue && to.Value > DateTime.MinValue) {
                questionnaireStatsQuery = questionnaireStatsQuery.Where(w => w.UserAnswers.AnswerDate <= to);
            }

            var questionnaireStats = questionnaireStatsQuery.ToList();

            if (questionnaireStats.Count == 0) {
                QuestionnaireStatsViewModel empty = new QuestionnaireStatsViewModel {
                    Id = questionnaireData.Id,
                    Title = questionnaireData.As<TitlePart>().Title,
                    QuestionsStatsList = new List<QuestionStatsViewModel>()
                };
                return empty;
            }
            else {
                var aggregatedStats = questionnaireStats.Select(s => new QuestionStatsViewModel {
                    QuestionnairePart_Id = s.Questions.QuestionnairePartRecord_Id,
                    QuestionnaireTitle = questionnaireData.As<TitlePart>().Title,
                    QuestionId = s.Questions.Id,
                    Question = s.Questions.Question,
                    Position = s.Questions.Position,
                    QuestionType = s.Questions.QuestionType,
                    Answers = new List<AnswerStatsViewModel>()
                })
                                                         .GroupBy(g => g.QuestionId)
                                                         .Select(s => s.First()).ToList();

                for (int i = 0; i < aggregatedStats.Count(); i++) {
                    var question = aggregatedStats.ElementAt(i);
                    var queryAnswer = questionnaireStats.Where(w => w.Questions.Id == question.QuestionId);
                    var answers = queryAnswer.GroupBy(g => g.UserAnswers.AnswerText, StringComparer.InvariantCultureIgnoreCase)
                                                    .Select(s => new AnswerStatsViewModel {
                                                        Answer = s.Key,
                                                        Count = s.Count(),
                                                        LastDate = s.OrderByDescending(x => x.UserAnswers.AnswerDate).FirstOrDefault() != null ? s.OrderByDescending(x => x.UserAnswers.AnswerDate).FirstOrDefault().UserAnswers.AnswerDate : DateTime.MinValue
                                                    });

                    question.Answers.AddRange(answers.OrderBy(o => o.Answer));
                }
                var FullyAnsweringPeople = questionnaireStats.Select(x => new { x.UserAnswers.SessionID, x.UserAnswers.QuestionRecord_Id })
                    .Distinct()
                    .GroupBy(info => info.SessionID)
                    .Select(group => new { group.Key, Count = group.Count() })
                    .Where(w => w.Count >= questionsCount)
                    .Count();
                return new QuestionnaireStatsViewModel {
                    Title = aggregatedStats[0].QuestionnaireTitle,
                    Id = questionnaireId,
                    NumberOfQuestions = questionsCount,
                    ReplyingPeopleCount = questionnaireStats.Select(x => x.UserAnswers.SessionID).Distinct().Count(),
                    FullyAnsweringPeople = FullyAnsweringPeople,
                    QuestionsStatsList = aggregatedStats.OrderBy(o => o.Position).ToList()
                };
            }
        }

        public List<QuestStatViewModel> GetStats(QuestionType type) {
            var listaQuest = _orchardServices.ContentManager.Query<QuestionnairePart, QuestionnairePartRecord>(VersionOptions.Latest)
                .List();
            var fullStat = new List<QuestStatViewModel>();
            foreach (var quest in listaQuest) {
                var title = quest.As<TitlePart>();
                var stats = _repositoryUserAnswer.Table
                    .Join(_questionAnswerRepositoryService.QuestionsRepository().Table,
                        l => l.QuestionRecord_Id,
                        r => r.Id,
                        (l, r) => new { UserAnswers = l, Questions = r })
                    .Where(w => w.Questions.QuestionType == type && w.Questions.QuestionnairePartRecord_Id == quest.Id)
                    .GroupBy(g => new { g.Questions.QuestionnairePartRecord_Id, g.UserAnswers.QuestionText, g.UserAnswers.AnswerText })
                    .Select(s => new QuestStatViewModel {
                        Question = s.Key.QuestionText,
                        Answer = s.Key.AnswerText,
                        QuestionnairePart_Id = s.Key.QuestionnairePartRecord_Id,
                        QuestionnaireTitle = title.Title,
                        Count = s.Count(),
                    });
                fullStat.AddRange(stats);
            }
            return fullStat.OrderBy(o => o.QuestionnaireTitle).ThenBy(o => o.Question).ThenBy(o => o.Answer).ToList();
        }

    }
}