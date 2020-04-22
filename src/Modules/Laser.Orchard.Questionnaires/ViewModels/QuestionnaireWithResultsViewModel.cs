using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {

    [ValidateFrontEndQuestionnaire]
    public class QuestionnaireWithResultsViewModel {
        public QuestionnaireWithResultsViewModel() {
            QuestionsWithResults = new List<QuestionWithResultsViewModel>();
            Context = "";
        }
        public int Id { get; set; }
        public bool MustAcceptTerms { get; set; }
        public string TermsText { get; set; }
        public bool UseRecaptcha { get; set; }
        public bool HasAcceptedTerms { get; set; }
        public string CaptchaHtmlWidget { get; set; }
        public IList<QuestionWithResultsViewModel> QuestionsWithResults { get; set; }
        public string Context { get; set; }

        public string AnswersInstance { get; set; }
    }
}
