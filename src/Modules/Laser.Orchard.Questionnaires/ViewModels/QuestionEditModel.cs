using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Questionnaires.Models;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class QuestionEditModel {
        public QuestionEditModel() {
            Answers = new List<AnswerEditModel>();
            Published = true;
        }
        public int Id { get; set; }
        public string Question { get; set; }
        public QuestionType QuestionType { get; set; }
        public AnswerType AnswerType { get; set; }
        public bool IsRequired { get; set; }
        public bool Published { get; set; }
        public int Position { get; set; }
        public int QuestionnairePartRecord_Id { get; set; }
        public IList<AnswerEditModel> Answers { get; set; }
        public string Section { get; set; }
        public bool Delete { get; set; }
        public string Condition { get; set; }
        public ConditionType ConditionType { get; set; }
        public int OriginalId { get; set; } // For Cloning and Import/Export purpose only
        public string AllFiles { get; set; }
        public string GUIdentifier { get; set; }
    }
}