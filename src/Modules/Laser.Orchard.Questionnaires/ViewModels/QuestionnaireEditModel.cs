using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class QuestionnaireEditModel {
        public QuestionnaireEditModel() {
            Questions = new List<QuestionEditModel>();
        }
        public int Id { get; set; }
        public bool MustAcceptTerms { get; set; }
        public string TermsText { get; set; }
        public bool UseRecaptcha { get; set; }
        public IEnumerable<QuestionEditModel> Questions { get; set; }
  
    }
}
