using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ExpressionEvaluator;
using Orchard.Localization;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class ValidateFrontEndQuestionnaireAttribute : ValidationAttribute {
        public ValidateFrontEndQuestionnaireAttribute() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        private IEnumerable<int> _answerIds;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var questionnaire = (QuestionnaireWithResultsViewModel)value;

            if (questionnaire.MustAcceptTerms && !questionnaire.HasAcceptedTerms) {
                return new ValidationResult(T("Please, accept our terms and conditions!").Text);
            }
            else {
                // valido le risposte in base alle condizioni dato che solo in questo oggetto ho la visione di insieme
                var singleChoice = questionnaire.QuestionsWithResults.Where(w => w.SingleChoiceAnswer > 0).Select(s => s.SingleChoiceAnswer);
                var multiChoice = questionnaire.QuestionsWithResults
                    .SelectMany(s => s.AnswersWithResult)
                    .Where(w => w.Answered)
                    .Select(s2 => s2.Id);
                _answerIds = singleChoice.Union(multiChoice);
                var validationErrors = new List<string>();
                var requiredQuestions = questionnaire.QuestionsWithResults.Where(w => String.IsNullOrWhiteSpace(w.Condition) && w.IsRequired);
                foreach (var q in requiredQuestions) {
                    switch (q.QuestionType) {
                        case QuestionType.OpenAnswer:
                            if (string.IsNullOrWhiteSpace(q.OpenAnswerAnswerText))
                                validationErrors.Add(String.Format(T("You have to answer to {0}").Text, q.Question));
                            break;
                        case QuestionType.MultiChoice:
                            if (q.AnswersWithResult.Count(w => w.Answered) == 0) {
                                validationErrors.Add(String.Format(T("You have to answer to {0}").Text, q.Question));
                            }
                            break;
                        case QuestionType.SingleChoice:
                            if (q.SingleChoiceAnswer <= 0) {
                                validationErrors.Add(String.Format(T("You have to answer to {0}").Text, q.Question));
                            }
                            break;
                    }
                }

                var conditionalQuestions = questionnaire.QuestionsWithResults.Where(w => !String.IsNullOrWhiteSpace(w.Condition) && w.IsRequired);

                foreach (var q in conditionalQuestions) {
                    // verifico per ogni question condizionale se tra tutte le risposte date la condizione è soddisfatta
                    var condition = Regex.Replace(q.Condition, "[0-9]+", new MatchEvaluator(HasThisAnswer));
                    condition = condition.Replace("and", "&&").Replace("or", "||");
                    var expression = new CompiledExpression(condition);
                    var conditionResult = expression.Eval();
                    if (((bool)conditionResult && q.ConditionType == ConditionType.Show) || ((!(bool)conditionResult) && q.ConditionType == ConditionType.Hide)) {
                        // Se la condizione è vera allora ho la condizione soddisfatta
                        if (q.IsRequired) {
                            // quindi la domanda è visibile a video e se è required deve avere una risposta pertinente
                            if (q.QuestionType == QuestionType.OpenAnswer) {
                                if (String.IsNullOrWhiteSpace(q.OpenAnswerAnswerText)) {
                                    validationErrors.Add(String.Format(T("You have to answer to {0}").Text, q.Question));
                                }
                                else {
                                    var textValidationError = String.Format(T(ValidateCommons.ValidateAnswerText(q.OpenAnswerAnswerText, q.AnswerType)).Text, q.Question);
                                    if (!String.IsNullOrWhiteSpace(textValidationError)) {
                                        validationErrors.Add(textValidationError);
                                    }
                                }
                            }
                            else if (q.QuestionType == QuestionType.SingleChoice && q.SingleChoiceAnswer <= 0) {
                                validationErrors.Add(String.Format(T("You have to answer to {0}").Text, q.Question));
                            }
                            else if (q.QuestionType == QuestionType.MultiChoice && q.AnswersWithResult.Count(w => w.Answered) == 0) {
                                validationErrors.Add(String.Format(T("You have to answer to {0}").Text, q.Question));
                            }
                        }
                        else {
                            if (q.QuestionType == QuestionType.OpenAnswer && !String.IsNullOrWhiteSpace(q.OpenAnswerAnswerText)) {
                                var textValidationError = String.Format(T(ValidateCommons.ValidateAnswerText(q.OpenAnswerAnswerText, q.AnswerType)).Text, q.Question);
                                if (!String.IsNullOrWhiteSpace(textValidationError)) {
                                    validationErrors.Add(textValidationError);
                                }
                            }
                        }
                    }
                    else if (((bool)conditionResult && q.ConditionType == ConditionType.Hide) || ((!(bool)conditionResult) && q.ConditionType == ConditionType.Show)) {
                        // Se la condizione è vera e deve nascondere oppure la condizione è falsa
                        q.OpenAnswerAnswerText = "";
                        q.SingleChoiceAnswer = 0;
                        for (var i = 0; i < q.AnswersWithResult.Count(); i++) {
                            q.AnswersWithResult[i].Answered = false;
                        }
                    }

                }
                if (validationErrors.Count() > 0) {
                    return new ValidationResult(String.Join("\r\n", validationErrors));
                }
            }
            return ValidationResult.Success;
        }

        private string HasThisAnswer(Match match) {
            int val = 0;
            int.TryParse(match.Value, out val);
            return _answerIds.Contains(val).ToString().ToLower();
        }
    }


    public class ValidateFrontEndQuestionAttribute : ValidationAttribute {
        public ValidateFrontEndQuestionAttribute() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var question = (QuestionWithResultsViewModel)value;
            if (question.IsRequired) { // Required
                if (String.IsNullOrWhiteSpace(question.Condition)) { // Senza condizioni
                    if (question.QuestionType == QuestionType.OpenAnswer) {
                        if (String.IsNullOrWhiteSpace(question.OpenAnswerAnswerText)) {
                            return new ValidationResult(String.Format(T("You have to answer to {0}").Text, question.Question));
                        }
                        else {
                            var textValidationError = String.Format(T(ValidateCommons.ValidateAnswerText(question.OpenAnswerAnswerText, question.AnswerType)).Text, question.Question);
                            if (!String.IsNullOrWhiteSpace(textValidationError)) {
                                return new ValidationResult(textValidationError);
                            }
                        }
                    }
                    else if (question.QuestionType == QuestionType.SingleChoice && question.SingleChoiceAnswer <= 0) {
                        return new ValidationResult(String.Format(T("You have to answer to {0}").Text, question.Question));
                    }
                    else if (question.QuestionType == QuestionType.MultiChoice && question.AnswersWithResult.Count(w => w.Answered) == 0) {
                        return new ValidationResult(String.Format(T("You have to answer to {0}").Text, question.Question));
                    }
                }
                else {
                    // Se ho delle condizioni Si occupa il validator del questionnaire (che ha la visione d'insieme di tutte le domanda/risposte
                }
            }
            else { // NotRequired
                if (question.QuestionType == QuestionType.OpenAnswer && !String.IsNullOrWhiteSpace(question.OpenAnswerAnswerText)) {
                    var textValidationError = String.Format(T(ValidateCommons.ValidateAnswerText(question.OpenAnswerAnswerText, question.AnswerType)).Text, question.Question); if (!String.IsNullOrWhiteSpace(textValidationError)) {
                        return new ValidationResult(textValidationError);
                    }
                }
            }
            return ValidationResult.Success;
        }


    }

    public static class ValidateCommons {

        public static string ValidateAnswerText(string answerText, AnswerType answerType) {

            answerText = (String.IsNullOrWhiteSpace(answerText) ? "" : answerText);
            if (answerType == AnswerType.Email) {
                if (!Regex.IsMatch(answerText, "^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$")) {
                    return "Your answer to {0} must be a valid email address";
                }
            }
            else if (answerType == AnswerType.Url) {
                if (!Regex.IsMatch(answerText, @"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?")) {
                    return "Your answer to {0} must be a valid internet address";
                }
            }
            else if (answerType == AnswerType.Date) {
                DateTime date = new DateTime();
                if (!DateTime.TryParse(answerText, out date)) {
                    return "Your answer to {0} must be a valid date";
                }
            }
            else if (answerType == AnswerType.Datetime) {
                DateTime date = new DateTime();
                if (!DateTime.TryParse(answerText, out date)) {
                    return "Your answer to {0} must be a valid date and time";
                }
            }
            else if (answerType == AnswerType.Number) {
                decimal number = new Decimal();
                if (!decimal.TryParse(answerText, out number)) {
                    return "Your answer to {0} must be a valid number";
                }
            }

            return "";
        }

    }
}
