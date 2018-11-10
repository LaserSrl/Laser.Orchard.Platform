using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.UI.Admin;
using Orchard.Workflows.Services;
using Orchard.Captcha.Services;
using Laser.Orchard.Questionnaires.Settings;

namespace Laser.Orchard.Questionnaires.Controllers {
    public class QuestionnaireController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IQuestionnairesServices _questionnairesServices;
        private readonly ITransactionManager _transactionManager;
        private readonly IWorkflowManager _workflowManager;
        private readonly ICaptchaService _captchaServices;
        public Localizer T { get; set; }

        public dynamic Shape { get; set; }

        public QuestionnaireController(IOrchardServices orchardServices,
            IQuestionnairesServices questionnairesServices,
            IRepository<AnswerRecord> repositoryAnswer,
            ITransactionManager transactionManager,
            IRepository<UserAnswersRecord> repositoryUserAnswers,
            IWorkflowManager workflowManager,
            ICaptchaService captchaServices,
            IShapeFactory shapeFactory) {
            _orchardServices = orchardServices;
            _questionnairesServices = questionnairesServices;
            _transactionManager = transactionManager;
            _captchaServices = captchaServices;
            T = NullLocalizer.Instance;
            _workflowManager = workflowManager;
            Shape = shapeFactory;
        }

        private const string _prefix = "Questionnaire";

        [HttpPost]
        public ActionResult Save(string returnUrl) {
            var editModel = new ViewModels.QuestionnaireWithResultsViewModel();
            var currentUser = _orchardServices.WorkContext.CurrentUser;

            try {
                if (TryUpdateModel(editModel, _prefix)) {
                    TempData["QuestUpdatedEditModel"] = editModel; // devo avere modo di fare non perdere le risposte date finora!!!
                    TempData["HasAcceptedTerms"] = editModel.HasAcceptedTerms;

                    QuestionnairesPartSettingVM questionnairePartSettings = null;
                    var questionnaire = _orchardServices.ContentManager.Get(editModel.Id);
                    if (questionnaire != null && questionnaire.As<QuestionnairePart>() != null) {
                        questionnairePartSettings = questionnaire.As<QuestionnairePart>().Settings.GetModel<QuestionnairesPartSettingVM>();
                    }

                    // verifica se il questionario può essere compilato una volta sola ed è già stato compilato
                    bool canBeFilled = true;
                    var questionnaireModuleSettings = _orchardServices.WorkContext.CurrentSite.As<QuestionnaireModuleSettingsPart>();
                    if (questionnaireModuleSettings.Disposable) {
                        if (currentUser == null || (questionnairePartSettings != null && questionnairePartSettings.ForceAnonymous)) {
                            var cookie = Request.Cookies["Questionnaires"];
                            if (cookie != null) {
                                var ids = cookie.Value;
                                if (ids.Contains("," + editModel.Id + ",")) {
                                    canBeFilled = false;
                                }
                            }
                        }
                    }
                    if (canBeFilled) {
                        canBeFilled = _questionnairesServices.Save(editModel, currentUser, ControllerContext.HttpContext.Session.SessionID);
                    }
                    //if (editModel.UseRecaptcha && !_captchaServices.IsCaptchaValid(_orchardServices.WorkContext.HttpContext.Request.Form, _orchardServices.WorkContext.HttpContext.Request.UserHostAddress)) {
                    //    throw new Exception("Invalid captcha!");
                    //}

                    //if (editModel.MustAcceptTerms && !editModel.HasAcceptedTerms) {
                    //    TempData["QuestError"] = T("Please, accept our terms and conditions!");
                    //} else {
                    //    foreach (var q in editModel.QuestionsWithResults) {
                    //        if (q.QuestionType == QuestionType.OpenAnswer) {
                    //            if (!String.IsNullOrWhiteSpace(q.OpenAnswerAnswerText)) {
                    //                var userAnswer = new UserAnswersRecord();
                    //                userAnswer.AnswerText = q.OpenAnswerAnswerText;
                    //                userAnswer.QuestionText = q.Question;
                    //                userAnswer.QuestionRecord_Id = q.Id;
                    //                userAnswer.User_Id = currentUser.Id;
                    //                userAnswer.QuestionnairePartRecord_Id = editModel.Id;
                    //                userAnswer.SessionID = ControllerContext.HttpContext.Session.SessionID;
                    //                _questionnairesServices.CreateUserAnswers(userAnswer);
                    //            }
                    //        } else if (q.QuestionType == QuestionType.SingleChoice) {
                    //            if (q.SingleChoiceAnswer > 0) {
                    //                var userAnswer = new UserAnswersRecord();
                    //                userAnswer.AnswerRecord_Id = q.SingleChoiceAnswer;
                    //                userAnswer.AnswerText = _questionnairesServices.GetAnswer(q.SingleChoiceAnswer).Answer;
                    //                userAnswer.QuestionRecord_Id = q.Id;
                    //                userAnswer.User_Id = currentUser.Id;
                    //                userAnswer.QuestionText = q.Question;
                    //                userAnswer.QuestionnairePartRecord_Id = editModel.Id;
                    //                userAnswer.SessionID = ControllerContext.HttpContext.Session.SessionID;
                    //                _questionnairesServices.CreateUserAnswers(userAnswer);
                    //            }
                    //        } else if (q.QuestionType == QuestionType.MultiChoice) {
                    //            var answerList = q.AnswersWithResult.Where(w => w.Answered);
                    //            foreach (var a in answerList) {
                    //                var userAnswer = new UserAnswersRecord();
                    //                userAnswer.AnswerRecord_Id = a.Id;
                    //                userAnswer.AnswerText = _questionnairesServices.GetAnswer(a.Id).Answer;
                    //                userAnswer.QuestionRecord_Id = q.Id;
                    //                userAnswer.User_Id = currentUser.Id;
                    //                userAnswer.QuestionText = q.Question;
                    //                userAnswer.QuestionnairePartRecord_Id = editModel.Id;
                    //                userAnswer.SessionID = ControllerContext.HttpContext.Session.SessionID;
                    //                _questionnairesServices.CreateUserAnswers(userAnswer);
                    //            }
                    //        }
                    //    }
                    if (canBeFilled == false) {
                        TempData["QuestError"] = T("Sorry, you already submitted this questionnaire.");
                        TempData["AlreadySubmitted"] = true;
                    } else {
                        TempData["QuestSuccess"] = T("Thank you for submitting your feedback.");
                    }
                    //var content = _orchardServices.ContentManager.Get(editModel.Id);
                    //_workflowManager.TriggerEvent("QuestionnaireSubmitted", content, () => new Dictionary<string, object> { { "Content", content } });


                    //}
                } else {
                    TempData["QuestUpdatedEditModel"] = editModel; // devo avere modo di fare non perdere le risposte date finora!!!
                    _transactionManager.Cancel();
                    var errors = ModelState.Values.Where(w => w.Errors.Count() > 0).Select(s => s.Errors.First().ErrorMessage);
                    TempData["QuestError"] = String.Join("\r\n", errors);
                }
            } catch (Exception ex) {
                TempData["QuestUpdatedEditModel"] = editModel; // devo avere modo di fare non perdere le risposte date finora!!!
                _transactionManager.Cancel();
                TempData["QuestError"] = T(ex.Message) + ex.StackTrace;
            }
            return this.RedirectLocal(returnUrl, "~/");

        }

        public ActionResult AcceptTerms(string returnUrl) {
            var editModel = new ViewModels.QuestionnaireWithResultsViewModel();
            if (TryUpdateModel(editModel, _prefix)) {
                TempData["HasAcceptedTerms"] = editModel.HasAcceptedTerms;
                if (!editModel.HasAcceptedTerms) {
                    TempData["QuestError"] = T("Please, accept our terms and conditions!");
                }
            } else {
                var errors = ModelState.Values.Where(w => w.Errors.Count() > 0).Select(s => s.Errors.First().ErrorMessage);
                TempData["QuestError"] = String.Join("\r\n", errors);
            }
            return this.RedirectLocal(returnUrl, "~/");

        }

        [Admin]
        public ActionResult StatsIndex() {
            var model = Shape.StatsIndex(valore: "");
            return View((object)model);
        }

        [Admin]
        public ActionResult IndexUserAnswers(QuestionType type) {

            var stats = _questionnairesServices.GetStats(type);
            return View(stats);
        }
    }
}