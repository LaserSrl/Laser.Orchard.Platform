using Orchard.ContentManagement;

namespace Laser.Orchard.Questionnaires.Models {

    public class QuestionnaireModuleSettingsPart : ContentPart {

        public bool Disposable {
            get { return this.Retrieve(x => x.Disposable,true); }
            set { this.Store(x => x.Disposable, value); }
        }
    }
}