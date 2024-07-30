using Orchard.ContentManagement.Records;
using System.Collections.Generic;

namespace Laser.Orchard.Questionnaires.Models {
    public class QuestionnairePartRecord : ContentPartRecord {
        public QuestionnairePartRecord() {
            Questions = new List<QuestionRecord>();
        }
        public virtual bool MustAcceptTerms { get; set; }
        public virtual string TermsText { get; set; }
        public virtual bool UseRecaptcha { get; set; }
        public virtual IList<QuestionRecord> Questions { get; set; }
 
    }
}