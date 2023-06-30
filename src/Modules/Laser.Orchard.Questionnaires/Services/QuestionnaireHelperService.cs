using System;
using Orchard.Localization;

namespace Laser.Orchard.Questionnaires.Services {
    public class QuestionnaireHelperServices : IQuestionnaireHelperServices {
        public QuestionnaireHelperServices() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string GetValidationString(ValidationAnswerType answerType) {
            switch (answerType) {
                case ValidationAnswerType.AcceptTerms:
                    return T("Please, accept our terms and conditions!").Text;

                case ValidationAnswerType.RequiredAnswer:
                    return T("You have to answer to {0}").Text;

                case ValidationAnswerType.ImpossibleValidation:
                    return T("Impossible to validate conditions.").Text;

                case ValidationAnswerType.InvalidEmailAddress:
                    return T("Your answer to {0} must be a valid email address").Text;

                case ValidationAnswerType.InvalidInternetAddress:
                    return T("Your answer to {0} must be a valid internet address").Text;

                case ValidationAnswerType.InvalidDate:
                    return T("Your answer to {0} must be a valid date").Text;

                case ValidationAnswerType.InvalidDateTime:
                    return T("Your answer to {0} must be a valid date and time").Text;

                case ValidationAnswerType.InvalidNumber:
                    return T("Your answer to {0} must be a valid number").Text;

                default:
                    return "";
            }
        }
    }
}