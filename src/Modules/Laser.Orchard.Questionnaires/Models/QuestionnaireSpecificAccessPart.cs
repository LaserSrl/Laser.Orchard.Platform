using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.Questionnaires.Models {
    public class QuestionnaireSpecificAccessPart : ContentPart<QuestionnaireSpecificAccessPartRecord> {
        public IEnumerable<int> UserIds {
            get {
                return Record.UserIds;
            }
            set {
                Record.UserIds = value;
            }
        }
    }
}