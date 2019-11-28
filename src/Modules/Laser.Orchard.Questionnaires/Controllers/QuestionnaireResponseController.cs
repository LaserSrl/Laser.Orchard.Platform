using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.Questionnaires.Controllers {
    [WebApiKeyFilter(false)]
    public class QuestionnaireResponseController : ApiController {
        private readonly IQuestionnairesServices _questionnairesServices;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IRepository<QuestionRecord> _repositoryQuestions;
        private readonly IRepository<AnswerRecord> _repositoryAnswer;
        private readonly IUtilsServices _utilsServices;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public QuestionnaireResponseController(
            IQuestionnairesServices questionnairesServices
            , IContentManager contentManager
            , IOrchardServices orchardServices
            , ICsrfTokenHelper csrfTokenHelper
            , IRepository<QuestionRecord> repositoryQuestions
            , IRepository<AnswerRecord> repositoryAnswer
            , IUtilsServices utilsServices
            ) {
            _questionnairesServices = questionnairesServices;
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _csrfTokenHelper = csrfTokenHelper;
            _repositoryQuestions = repositoryQuestions;
            _repositoryAnswer = repositoryAnswer;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _utilsServices = utilsServices;
        }

        /// <summary>
        /// esempio [{"Answered":1,"AnswerText":"ciao","Id":5,"QuestionRecord_Id":5}]
        /// </summary>
        /// <param name="Risps">elenco con valorizzati solo id della risposta scelta  nel caso di risposta semplice
        /// QuestionRecord_Id e e AnswerText nel caso di risposta con testo libero
        /// </param>
        /// <returns></returns>
        [WebApiKeyFilter(true)]
        public Response Post([FromBody] List<AnswerWithResultViewModel> Risps) {
            if (Risps != null) {
                return ExecPost(Risps);
            } else {
                return (_utilsServices.GetResponse(ResponseType.Validation, T("Validation: invalid input data structure.").ToString()));
            }
        }
        /// <summary>
        /// esempio [{"Answered":1,"AnswerText":"ciao","Id":5,"QuestionRecord_Id":5}]
        /// </summary>
        /// <param name="Risps">elenco con valorizzati solo id della risposta scelta  nel caso di risposta semplice
        /// QuestionRecord_Id e e AnswerText nel caso di risposta con testo libero
        /// </param>
        /// <param name="qContext">Questo parametro è preso dall'URL: è dichiarato nella rotta.</param>
        /// <returns></returns>
        [HttpPost]
        [WebApiKeyFilter(true)]
        public Response PostContext([FromBody] List<AnswerWithResultViewModel> Risps, string qContext) {
            if (Risps != null) {
                return ExecPost(Risps, qContext);
            }
            else {
                return (_utilsServices.GetResponse(ResponseType.Validation, T("Validation: invalid input data structure.").ToString()));
            }
        }
        private Response ExecPost(List<AnswerWithResultViewModel> Risps, string QuestionnaireContext = "") {
#if DEBUG
            Logger.Error(Request.Headers.ToString());
#endif
            int QuestionId = 0;

            if (_orchardServices.Authorizer.Authorize(Permissions.SubmitQuestionnaire)) {

                if (Risps.Count > 0) {
                    var currentUser = _orchardServices.WorkContext.CurrentUser;

                    if (Risps[0].Id > 0)
                        QuestionId = _repositoryAnswer.Fetch(x => x.Id == Risps[0].Id).FirstOrDefault().QuestionRecord_Id;
                    else
                        QuestionId = Risps[0].QuestionRecord_Id;
                    Int32 id = _repositoryQuestions.Fetch(x => x.Id == QuestionId).FirstOrDefault().QuestionnairePartRecord_Id;

	                var content = _contentManager.Get(id);
	                var qp = content.As<QuestionnairePart>();
                    QuestionnaireWithResultsViewModel qVM = _questionnairesServices.BuildViewModelWithResultsForQuestionnairePart(qp);
                    foreach (QuestionWithResultsViewModel qresult in qVM.QuestionsWithResults) {
                        if (qresult.QuestionType == QuestionType.OpenAnswer) {
                            foreach (AnswerWithResultViewModel Risp in Risps) {
                                if (qresult.Id == Risp.QuestionRecord_Id && !(string.IsNullOrEmpty(Risp.AnswerText))) {
                                    qresult.OpenAnswerAnswerText = Risp.AnswerText;
                                }
                            }
                        } else {
                            foreach (AnswerWithResultViewModel asw in qresult.AnswersWithResult) {
                                foreach (AnswerWithResultViewModel Risp in Risps) {
                                    if (asw.Id == Risp.Id) {
                                        if (qresult.QuestionType == QuestionType.SingleChoice) {
                                            qresult.SingleChoiceAnswer = asw.Id;
                                        } else
                                            asw.Answered = true;
                                    }
                                }
                            }
                        }
                    }

                    var context = new ValidationContext(qVM, serviceProvider: null, items: null);
                    var results = new List<ValidationResult>();
                    var isValid = Validator.TryValidateObject(qVM, context, results);
                    if (!isValid) {
                        string messaggio = "";
                        foreach (var validationResult in results) {
                            messaggio += validationResult.ErrorMessage + " ";
                        }
                        return (_utilsServices.GetResponse(ResponseType.Validation, T("Validation:").ToString() + messaggio));
                    } else {
                        qVM.Context = QuestionnaireContext;

                        string uniqueId;
                        var request = HttpContext.Current.Request;

                        if (request != null && request.Headers["x-uuid"] != null) {
                            uniqueId = request.Headers["x-uuid"];
                        } else {
                            uniqueId = HttpContext.Current.Session.SessionID;
                        }

                        if (_questionnairesServices.Save(qVM, currentUser, uniqueId)) {
                            return (_utilsServices.GetResponse(ResponseType.Success));
                        } else {
                            return (_utilsServices.GetResponse(ResponseType.Validation, T("Questionnaire already submitted.").ToString()));
                        }
                    }
                }
                else 
                    return (_utilsServices.GetResponse(ResponseType.Validation, T("Validation: data list is empty.").ToString()));
            }
            else
                return (_utilsServices.GetResponse(ResponseType.UnAuthorized));
        }
    }
}