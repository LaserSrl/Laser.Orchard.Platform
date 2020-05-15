using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Models {
    public class AnswerRecord {

        public AnswerRecord() {
            Published = true;
        }
        public virtual int Id { get; set; }
        public virtual string GUIdentifier { get; set; }
        [MaxLength(1200)]
        public virtual string Answer { get; set; }
        public virtual bool Published { get; set; }
        public virtual int Position { get; set; }
        // foreign models
        public virtual int QuestionRecord_Id { get; set; }

        public virtual bool CorrectResponse { get; set; }
        public virtual string AllFiles { get; set; }


    }
}