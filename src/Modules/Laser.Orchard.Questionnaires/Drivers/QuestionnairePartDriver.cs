using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.Settings;
using Laser.Orchard.Questionnaires.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Captcha.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.OutputCache;
using Orchard.Security;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Laser.Orchard.Questionnaires.Drivers {
    public class QuestionnairePartDriver : ContentPartCloningDriver<QuestionnairePart>, ICachingEventHandler {
        private readonly IQuestionnairesServices _questServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IOrchardServices _orchardServices;
        private readonly ICaptchaService _capthcaServices;
        private readonly ITokenizer _tokenizer;
        private readonly ICurrentContentAccessor _currentContentAccessor;

        public QuestionnairePartDriver(IQuestionnairesServices questServices,
            ICurrentContentAccessor currentContentAccessor,
            IOrchardServices orchardServices,
            IControllerContextAccessor controllerContextAccessor,
            ICaptchaService capthcaServices,
            ITokenizer tokenizer) {
            _questServices = questServices;
            _orchardServices = orchardServices;
            _controllerContextAccessor = controllerContextAccessor;
            T = NullLocalizer.Instance;
            _capthcaServices = capthcaServices;
            _tokenizer = tokenizer;
            _currentContentAccessor = currentContentAccessor;
            _isAuthorized = new Lazy<bool>(() =>
                _orchardServices.Authorizer.Authorize(Permissions.SubmitQuestionnaire)
            );
        }

        private Lazy<bool> _isAuthorized;
        private bool IsAuthorized {
            get {
                return _isAuthorized.Value;
            }
        }
        public Localizer T { get; set; }
        protected override string Prefix {
            get {
                return "Questionnaire";
            }
        }
        protected override DriverResult Display(QuestionnairePart part, string displayType, dynamic shapeHelper) {
            if (displayType == "Summary")
                return ContentShape("Parts_Questionnaire_Summary",
                    () => shapeHelper.Parts_Questionnaire_Summary(
                        QuestionsCount: part.Questions.Count(c => c.Published)
                        ));
            if (displayType == "SummaryAdmin")
                return ContentShape("Parts_Questionnaire_SummaryAdmin",
                    () => shapeHelper.Parts_Questionnaire_SummaryAdmin(
                        QuestionsCount: part.Questions.Count(c => c.Published),
                        QuestionsTotalCount: part.Questions.Count()
                        ));

            if (IsAuthorized) {
                var partSettings = part.Settings.TryGetModel<QuestionnairesPartSettingVM>();
                if (partSettings == null) {
                    partSettings = new QuestionnairesPartSettingVM();
                }
                QuestionnaireWithResultsViewModel viewModel;
                if (_controllerContextAccessor.Context != null) {
                    bool questionnaireHasJustBeenSubmitted;
                    // valorizza il context
                    var questionnaireContext = partSettings.QuestionnaireContext;
                    var currentUser = _orchardServices.WorkContext.CurrentUser;
                    questionnaireContext = _tokenizer.Replace(questionnaireContext, new Dictionary<string, object> { { "Content", _currentContentAccessor.CurrentContentItem } });
                    // TempData may contains current answers to the current questionnaire instance.
                    // So if TempData is not null means that the current user just answered to the questionnaire.
                    var fullModelWithAnswers = _controllerContextAccessor.Context.Controller.TempData["QuestUpdatedEditModel"]; 
                    var hasAcceptedTerms = _controllerContextAccessor.Context.Controller.TempData["HasAcceptedTerms"];
                    questionnaireHasJustBeenSubmitted = fullModelWithAnswers != null;
                    viewModel = _questServices.BuildViewModelWithResultsForQuestionnairePart(part); //Modello mappato senza risposte
                    if (currentUser != null && fullModelWithAnswers == null && partSettings.ShowLatestAnswers) {
                        //if the current questionnaire instance has not answers and settings require to show current user's latest answers
                        fullModelWithAnswers = _questServices.GetMostRecentAnswersInstance(part, currentUser, questionnaireContext);
                    }
                    viewModel.Context = questionnaireContext;

                    // limita la lunghezza del context a 255 chars
                    if (viewModel.Context.Length > 255) {
                        viewModel.Context = viewModel.Context.Substring(0, 255);
                    }

                    // valorizza le altre proprietà del viewModel
                    if (fullModelWithAnswers != null) { // Mappo le risposte
                        var risposteModel = (QuestionnaireWithResultsViewModel)fullModelWithAnswers;
                        //Mappo l'oggetto principale per evitare che mi richieda di accettare le condizioni
                        viewModel.MustAcceptTerms = risposteModel.MustAcceptTerms;
                        viewModel.HasAcceptedTerms = risposteModel.HasAcceptedTerms;

                        for (var i = 0; i < viewModel.QuestionsWithResults.Count(); i++) {
                            var question = viewModel.QuestionsWithResults[i];
                            // Gets the userAnswers having same question id (Id) and same question text (Question) of question of the content.
                            // If missing, means that the current user never answered to that question.
                            var questionWithAnswers = risposteModel.QuestionsWithResults.FirstOrDefault(x => x.Id == question.Id && x.Question == question.Question);
                            if (questionWithAnswers != null) {
                                switch (viewModel.QuestionsWithResults[i].QuestionType) {
                                    case QuestionType.OpenAnswer:
                                        viewModel.QuestionsWithResults[i].OpenAnswerAnswerText = questionWithAnswers.OpenAnswerAnswerText;
                                        break;
                                    case QuestionType.SingleChoice:
                                        viewModel.QuestionsWithResults[i].SingleChoiceAnswer = questionWithAnswers.SingleChoiceAnswer;
                                        break;
                                    case QuestionType.MultiChoice:
                                        for (var j = 0; j < viewModel.QuestionsWithResults[i].AnswersWithResult.Count(); j++) {
                                            var choice = question.AnswersWithResult[j];
                                            // Gets the answers of question having same answer id (Id) and same answer text (userResponse.AnswerText > answer.Answer) of the answer of the content.
                                            // If missing, means that the current user never answered with current option.
                                            var answer = questionWithAnswers.AnswersWithResult.SingleOrDefault(x => x.Id == choice.Id && (questionnaireHasJustBeenSubmitted || x.AnswerText == choice.Answer));
                                            viewModel.QuestionsWithResults[i].AnswersWithResult[j].Answered = answer != null ? answer.Answered : false;
                                        }
                                        break;
                                }
                            }

                        }
                    }
                    else if (hasAcceptedTerms != null) { // l'utente ha appena accettato le condizionoi
                        viewModel.HasAcceptedTerms = (bool)_controllerContextAccessor.Context.Controller.TempData["HasAcceptedTerms"];
                    }
                }
                else {
                    // There's not a WorkContext
                    viewModel = _questServices.BuildViewModelWithResultsForQuestionnairePart(part); //Modello mappato senza risposte
                }
                if (viewModel.UseRecaptcha) { // se è previsto un recaptcha creo l'html e il js del recaptcha
                    viewModel.CaptchaHtmlWidget = _capthcaServices.GenerateCaptcha();
                }
                return ContentShape("Parts_Questionnaire_FrontEnd_Edit",
                 () => shapeHelper.EditorTemplate(TemplateName: "Parts/Questionnaire_FrontEnd_Edit",
                     Model: viewModel,
                     Prefix: Prefix));
            }
            else {
                throw new OrchardSecurityException(T("You have to be logged in, before answering a questionnaire!"));
            }
        }

        protected override DriverResult Editor(QuestionnairePart part, dynamic shapeHelper) {
            Int32 QuestionsLimitsNumber = part.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionsLimitsNumber;
            Int32 QuestionsSortedRandomlyNumber = part.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionsSortedRandomlyNumber;
            bool ShowCorrectResponseFlag = part.Settings.GetModel<QuestionnairesPartSettingVM>().ShowCorrectResponseFlag;
            _controllerContextAccessor.Context.Controller.TempData["ShowCorrectResponseFlag"] = ShowCorrectResponseFlag;
            _controllerContextAccessor.Context.Controller.TempData["QuestionsLimitsNumber"] = QuestionsLimitsNumber;
            _controllerContextAccessor.Context.Controller.TempData["AnswersLimitsNumber"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionsResponseLimitsNumber;
            _controllerContextAccessor.Context.Controller.TempData["EnableQuestionImage"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().EnableQuestionImage;
            _controllerContextAccessor.Context.Controller.TempData["EnableAnswerImage"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().EnableAnswerImage;
            _controllerContextAccessor.Context.Controller.TempData["QuestionImageLimitNumber"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionImageLimitNumber;
            _controllerContextAccessor.Context.Controller.TempData["AnswerImageLimitNumber"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().AnswerImageLimitNumber;
            _controllerContextAccessor.Context.Controller.ViewBag.QuestionnairesPartSettings = part.Settings.GetModel<QuestionnairesPartSettingVM>();
            QuestionnaireEditModel modelForEdit;
            if (_controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"] != null) {
                modelForEdit = (QuestionnaireEditModel)_controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"];
            }
            else {
                modelForEdit = _questServices.BuildEditModelForQuestionnairePart(part);
            }

            _controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"] = null;
            return ContentShape("Parts_Questionnaire_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/Questionnaire_Edit",
                                 Model: modelForEdit,
                                 Prefix: Prefix));
        }

        protected override DriverResult Editor(QuestionnairePart part, IUpdateModel updater, dynamic shapeHelper) {
            QuestionnaireEditModel editModel = new QuestionnaireEditModel();
            editModel = _questServices.BuildEditModelForQuestionnairePart(part);
            try {
                if (updater.TryUpdateModel(editModel, Prefix, null, null)) {
                    if (part.ContentItem.Id != 0) {
                        // se per caso part.Id è diversa dall'Id registrato nei record relazionati, arrivo da una traduzione, quindi devo trattare tutto come se fosse questions e answers nuove
                        foreach (var q in editModel.Questions) {
                            if (part.Id != q.QuestionnairePartRecord_Id) {
                                q.QuestionnairePartRecord_Id = part.Id;
                                q.Id = 0;
                            }

                            foreach (var a in q.Answers) {
                                if (q.Id == 0) {
                                    a.Id = 0;
                                }
                            }
                        }
                        try {
                            _questServices.UpdateForContentItem(
                                part.ContentItem, editModel);
                        }
                        catch (Exception ex) {
                            updater.AddModelError("QuestionnaireUpdateError", T("Cannot update questionnaire. " + ex.Message));
                            _controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"] = editModel;
                        }
                    }

                }
                else {
                    updater.AddModelError("QuestionnaireUpdateError", T("Cannot update questionnaire"));
                    _controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"] = editModel;
                }
            }
            catch (Exception ex) {
                updater.AddModelError("QuestionnaireUpdateError", T("Cannot update questionnaire....... " + ex.Message + ex.StackTrace));
                _controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"] = editModel;
            }
            return Editor(part, shapeHelper);
        }


        #region [ Import/Export ]
        protected override void Exporting(QuestionnairePart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            XElement termsText = new XElement("TermsText");
            root.SetAttributeValue("MustAcceptTerms", part.MustAcceptTerms);
            root.SetAttributeValue("UseRecaptcha", part.UseRecaptcha);
            termsText.SetValue(part.TermsText ?? "");
            root.Add(termsText);
            foreach (var q in part.Questions) {
                XElement question = new XElement("Question");
                question.SetAttributeValue("OriginalId", q.Id);
                question.SetAttributeValue("Position", q.Position);
                question.SetAttributeValue("Published", q.Published);
                question.SetAttributeValue("Question", q.Question);
                question.SetAttributeValue("QuestionType", q.QuestionType);
                question.SetAttributeValue("AnswerType", q.AnswerType);
                question.SetAttributeValue("Section", q.Section);
                question.SetAttributeValue("IsRequired", q.IsRequired);
                question.SetAttributeValue("Condition", q.Condition);
                question.SetAttributeValue("ConditionType", q.ConditionType);
                question.SetAttributeValue("GUIdentifier", q.GUIdentifier);

                ExportMedia(question, q.AllFiles, "AllFiles");
                foreach (var a in q.Answers) {
                    XElement answer = new XElement("Answer");
                    answer.SetAttributeValue("OriginalId", a.Id);
                    answer.SetAttributeValue("Position", a.Position);
                    answer.SetAttributeValue("Published", a.Published);
                    answer.SetAttributeValue("Answer", a.Answer);
                    answer.SetAttributeValue("GUIdentifier", a.GUIdentifier);
                    answer.SetAttributeValue("CorrectResponse", a.CorrectResponse);
                    ExportMedia(answer, a.AllFiles, "AllFiles");
                    question.Add(answer);
                }
                root.Add(question);
            }
        }
        private void ExportMedia(XElement parent, string elencoId, string childsName) {
            var sArrFiles = (elencoId ?? "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var iArrFiles = sArrFiles.Select(x => int.Parse(x));
            foreach (var mediaId in iArrFiles) {
                var ci = _orchardServices.ContentManager.Get(mediaId);
                var mediaIdentity = _orchardServices.ContentManager.GetItemMetadata(ci).Identity.ToString();
                parent.Add(new XElement(childsName, mediaIdentity));
            }
        }
        protected override void Importing(QuestionnairePart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var questions = root.Elements("Question");
            var editModel = _questServices.BuildEditModelForQuestionnairePart(part);
            editModel.MustAcceptTerms = bool.Parse(root.Attribute("MustAcceptTerms").Value);
            editModel.UseRecaptcha = bool.Parse(root.Attribute("UseRecaptcha").Value);
            editModel.TermsText = root.Element("TermsText").Value;

            var questionModelList = new List<QuestionEditModel>();
            foreach (var q in questions) { // recupero le questions
                var answers = q.Elements("Answer");
                var answerModelList = new List<AnswerEditModel>();
                foreach (var a in answers) { // recupero le answers
                    var answerEditModel = new AnswerEditModel {
                        Position = a.Attribute("Position") != null ? int.Parse(a.Attribute("Position").Value) : 0,
                        Published = a.Attribute("Published") != null ? bool.Parse(a.Attribute("Published").Value) : false,
                        Answer = a.Attribute("Answer") != null ? a.Attribute("Answer").Value : "",
                        OriginalId = a.Attribute("OriginalId") != null ? int.Parse(a.Attribute("OriginalId").Value) : 0,
                        CorrectResponse = a.Attribute("CorrectResponse") != null ? bool.Parse(a.Attribute("CorrectResponse").Value) : false,
                        AllFiles = ImportMedia(a, "AllFiles"),
                        GUIdentifier = a.Attribute("GUIdentifier") != null ? a.Attribute("GUIdentifier").Value : "",
                    };
                    answerModelList.Add(answerEditModel);
                }
                var questionEditModel = new QuestionEditModel {
                    Position = q.Attribute("Position") != null ? int.Parse(q.Attribute("Position").Value) : 0,
                    Published = q.Attribute("Published") != null ? bool.Parse(q.Attribute("Published").Value) : false,
                    Question = q.Attribute("Question") != null ? q.Attribute("Question").Value : "",
                    Section = q.Attribute("Section") != null ? q.Attribute("Section").Value : "",
                    QuestionType = q.Attribute("QuestionType") != null ? (QuestionType)Enum.Parse(typeof(QuestionType), q.Attribute("QuestionType").Value) : QuestionType.SingleChoice,
                    AnswerType = q.Attribute("AnswerType") != null ? (AnswerType)Enum.Parse(typeof(AnswerType), q.Attribute("AnswerType").Value) : AnswerType.None,
                    IsRequired = q.Attribute("IsRequired") != null ? bool.Parse(q.Attribute("IsRequired").Value) : false,
                    QuestionnairePartRecord_Id = part.Id,
                    Answers = answerModelList,
                    Condition = q.Attribute("Condition") == null ? null : q.Attribute("Condition").Value,
                    ConditionType = q.Attribute("ConditionType") != null ? (ConditionType)Enum.Parse(typeof(ConditionType), q.Attribute("ConditionType").Value) : ConditionType.Show,
                    OriginalId = q.Attribute("OriginalId") != null ? int.Parse(q.Attribute("OriginalId").Value) : 0,
                    AllFiles = ImportMedia(q, "AllFiles"),
                    GUIdentifier = q.Attribute("GUIdentifier") != null ? q.Attribute("GUIdentifier").Value : ""
                };
                questionModelList.Add(questionEditModel);
            }
            editModel.Questions = questionModelList; // metto tutto nel model 
            _questServices.UpdateForContentItem(
                    part.ContentItem, editModel); //aggiorno
        }
        private string ImportMedia(XElement parent, string childsName) {
            List<int> iArrFiles = new List<int>();
            var mediaElements = parent.Elements("AllFiles");
            if (mediaElements != null) {
                foreach (var element in mediaElements) {
                    var ci = _orchardServices.ContentManager.ResolveIdentity(new ContentIdentity(element.Value));
                    if (ci != null) {
                        iArrFiles.Add(ci.Id);
                    }
                }
            }
            var elencoId = string.Join(",", iArrFiles);
            if (string.IsNullOrWhiteSpace(elencoId)) {
                elencoId = null;
            }
            return elencoId;
        }
        #endregion

        protected override void Cloning(QuestionnairePart originalPart, QuestionnairePart clonePart, CloneContentContext context) {
            var editModel = _questServices.BuildEditModelForQuestionnairePart(clonePart);
            editModel.MustAcceptTerms = originalPart.MustAcceptTerms;
            editModel.TermsText = originalPart.TermsText;
            editModel.UseRecaptcha = originalPart.UseRecaptcha;
            //clone list of questions
            var questionModelList = new List<QuestionEditModel>();
            foreach (var question in originalPart.Questions) {
                //clone answers
                var answerModelList = new List<AnswerEditModel>();
                foreach (var answer in question.Answers) {
                    answerModelList.Add(new AnswerEditModel() {
                        Answer = answer.Answer,
                        Published = answer.Published,
                        Position = answer.Position,
                        OriginalId = answer.Id,
                        CorrectResponse = answer.CorrectResponse,
                        AllFiles = answer.AllFiles
                    });
                }
                questionModelList.Add(new QuestionEditModel() {
                    Question = question.Question,
                    QuestionType = question.QuestionType,
                    AnswerType = question.AnswerType,
                    IsRequired = question.IsRequired,
                    Published = question.Published,
                    Position = question.Position,
                    QuestionnairePartRecord_Id = clonePart.Id,
                    Answers = answerModelList,
                    Section = question.Section,
                    Condition = question.Condition,
                    ConditionType = question.ConditionType,
                    OriginalId = question.Id,
                    AllFiles = question.AllFiles
                });
            }
            editModel.Questions = questionModelList;
            _questServices.UpdateForContentItem(clonePart.ContentItem, editModel);
        }

        public void KeyGenerated(StringBuilder key) {
            if (IsAuthorized) {
                key.Append("SubmitQuestionnaire=true;");
            }
            else {
                key.Append("SubmitQuestionnaire=false;");
            }
        }

    }
}