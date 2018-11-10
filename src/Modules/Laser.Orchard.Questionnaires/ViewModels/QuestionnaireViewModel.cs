using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class QuestionnaireViewModel {
        public QuestionnaireViewModel() {
            Questions = new List<QuestionViewModel>();
        }

        public int Id { get; set; }
        public bool MustAcceptTerms { get; set; }
        public string TermsText { get; set; }
        public bool UseRecaptcha { get; set; }
        public IEnumerable<QuestionViewModel> Questions { get; set; }
    }
}
