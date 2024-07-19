using Orchard.ContentManagement;

namespace Laser.Orchard.Questionnaires.Models {

    public class QuestionnaireModuleSettingsPart : ContentPart {

        public bool Disposable {
            get { return this.Retrieve(x => x.Disposable,true); }
            set { this.Store(x => x.Disposable, value); }
        }

        public bool EnableCsvExport {
            get { return this.Retrieve(x => x.EnableCsvExport, true); }
            set { this.Store(x => x.EnableCsvExport, value); }
        }
    }
}