using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Models {
    public class UserAnswersRecord {
        public UserAnswersRecord() {
            AnswerDate = DateTime.UtcNow;
            AnswerRecord_Id = null;
        }
        public virtual int Id { get; set; }
        public virtual int QuestionRecord_Id { get; set; }
        public virtual int? AnswerRecord_Id { get; set; }
        [MaxLength(500)]
        public virtual string QuestionText { get; set; }
        [MaxLength(1200)]
        public virtual string AnswerText { get; set; }
        public virtual int User_Id { get; set; }
        public virtual DateTime AnswerDate { get; set; }
        [Required]
        public virtual int QuestionnairePartRecord_Id { get; set; }
        [Required]
        public virtual string SessionID { get; set; }
        public virtual string Context { get; set; }
        /// <summary>
        /// Answers to a specific Questionnaire given by a specific user at a given
        /// time will share this value. The goal is to be able identify all the 
        /// UserAnswerRecord that were created "together" when a user submitted their
        /// answers to a questionnaire.
        /// We will generate this string as 
        /// SHA256({user.Id + QuestionnairePartRecord_Id + DateTime.UtcNow})
        /// if a user exists, or
        /// SHA256({sessionId + QuestionnairePartRecord_Id + DateTime.UtcNow})
        /// otherwise
        /// </summary>
        public virtual string AnswerInstance { get; set; }
        public virtual QuestionType QuestionType { get; set; }
    }
}