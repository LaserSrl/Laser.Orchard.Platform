using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Questionnaires.Models {
    public class QuestionnairePart : ContentPart<QuestionnairePartRecord> {

        public bool MustAcceptTerms { 
            get { return Record.MustAcceptTerms; }
            set { Record.MustAcceptTerms = value; } 
        }
        public string TermsText {
            get { return Record.TermsText; }
            set { Record.TermsText = value; }
        }
        public bool UseRecaptcha {
            get { return Record.UseRecaptcha; }
            set { Record.UseRecaptcha = value; }
        }

        public IList<QuestionRecord> Questions {
            get {
                if (this.QuestionsToDisplay != null)
                    return QuestionsToDisplay;
                else {
                    IList<QuestionRecord> a = Record.Questions.Select(s => s).OrderBy(o => o.Position).ToList();
                    return a;
                }
            }
        }

        public IList<QuestionRecord> QuestionsToDisplay { get; set; }
       

    }
}