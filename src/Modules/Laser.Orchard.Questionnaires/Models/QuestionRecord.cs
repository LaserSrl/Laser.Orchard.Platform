using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Models {
    public class QuestionRecord {

        public QuestionRecord() {
            Answers = new List<AnswerRecord>();
            Published = true;
        }
        public virtual int Id { get; set; }
        public virtual string GUIdentifier { get; set; }
        [MaxLength(500)]
        public virtual string Question { get; set; }
        public virtual QuestionType QuestionType { get; set; }
        public virtual AnswerType AnswerType { get; set; }
        public virtual bool IsRequired { get; set; }
        public virtual bool Published { get; set; }
        public virtual int Position { get; set; }

        [MaxLength(200)]
        public virtual string Section { get; set; }
        public virtual string Condition { get; set; }
        public virtual ConditionType ConditionType { get; set; }
        // foreign models
        public virtual int QuestionnairePartRecord_Id { get; set; }
        public virtual IList<AnswerRecord> Answers { get; set; }
        public virtual string AllFiles { get; set; }


    }
}