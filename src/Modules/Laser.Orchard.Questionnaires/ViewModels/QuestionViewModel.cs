using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Questionnaires.Models;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class QuestionViewModel {
        public QuestionViewModel() {
            Answers = new List<AnswerViewModel>();
        }
        public int Id { get; set; }
        public string Question { get; set; }
        public QuestionType QuestionType { get; set; }
        public AnswerType AnswerType { get; set; }
        public bool IsRequired { get; set; }
        public bool Published { get; set; }
        public int Position { get; set; }
        public int QuestionnairePartRecord_Id { get; set; }
        public IList<AnswerViewModel> Answers { get; set; }
        public string Section { get; set; }
        public string Condition { get; set; }
        public ConditionType ConditionType { get; set; }
        public string AllFiles { get; set; }
    }
}