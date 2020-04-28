using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Models {
    public class UserAnswerInstanceRecord {
        public virtual int Id { get; set; } // Primary Key
        public virtual int QuestionnairePartRecord_Id { get; set; }
        public virtual int User_Id { get; set; }
        public virtual string SessionID { get; set; }
        public virtual string Context { get; set; }
        public virtual DateTime AnswerDate { get; set; }
        public virtual string AnswerInstance { get; set; }
    }
}