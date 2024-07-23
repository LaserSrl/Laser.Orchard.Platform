using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.Questionnaires.Models {
    public class QuestionnaireSpecificAccessPart : ContentPart<QuestionnaireSpecificAccessPartRecord> {
        public IEnumerable<int> UserIds {
            get {
                if (Record == null) {
                    Record = new QuestionnaireSpecificAccessPartRecord();
                }
                return Record.DecodeIds(Record.SerializedUserIds);
            }
            set {
                Record.SerializedUserIds = Record.EncodeIds(value);
            }
        }
    }
}