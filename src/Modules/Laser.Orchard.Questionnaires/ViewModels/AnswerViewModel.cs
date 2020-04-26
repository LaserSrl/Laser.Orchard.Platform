using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Questionnaires.Models;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class AnswerViewModel {
        // AnswerRecord.Id
        public int Id { get; set; }
        public string Answer { get; set; }
        public bool Published { get; set; }
        public int Position { get; set; }
        public int QuestionRecord_Id { get; set; }
        public string AllFiles { get; set; }
    }

}