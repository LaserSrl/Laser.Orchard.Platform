using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Questionnaires.Models;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class AnswerEditModel {
        public AnswerEditModel() {
            Published = true;
        }
        public int Id { get; set; }
        public string Answer { get; set; }
        public bool Published { get; set; }
        public int Position { get; set; }
        public int QuestionRecord_Id { get; set; }
        public bool Delete { get; set; }
        public int OriginalId { get; set; } // For Cloning and Import/Export purpose only
        public  bool CorrectResponse { get; set; }
        public string AllFiles { get; set; }
        public string GUIdentifier { get; set; }
    }
}