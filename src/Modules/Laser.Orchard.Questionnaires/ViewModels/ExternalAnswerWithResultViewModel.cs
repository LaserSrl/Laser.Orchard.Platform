using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class ExternalAnswerWithResultViewModel {
        public List<AnswerWithResultViewModel> Answers { get; set; }
        public string QuestionnaireContext { get; set; }
    }
}