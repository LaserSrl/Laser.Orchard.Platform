using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Laser.Orchard.Questionnaires.Models;

namespace Laser.Orchard.Questionnaires.ViewModels {

    [ValidateFrontEndQuestion]
    public class QuestionWithResultsViewModel {
        public QuestionWithResultsViewModel() {
            AnswersWithResult = new List<AnswerWithResultViewModel>();
        }
        public int Id { get; set; }
        public string Question { get; set; }
        public QuestionType QuestionType { get; set; }
        public AnswerType AnswerType { get; set; }
        public bool IsRequired { get; set; }
        public bool Published { get; set; }
        public int Position { get; set; }
        public int SingleChoiceAnswer { get; set; }
        [MaxLength(1200)]
        public string OpenAnswerAnswerText { get; set; }
        public int QuestionnairePartRecord_Id { get; set; }
        public string Section { get; set; }

        public IList<AnswerWithResultViewModel> AnswersWithResult { get; set; }
        public string Condition { get; set; }
        public ConditionType ConditionType { get; set; }
        public string AllFiles { get; set; }
    }
}