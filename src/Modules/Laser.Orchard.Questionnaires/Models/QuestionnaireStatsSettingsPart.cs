using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Questionnaires.Models {
    [OrchardFeature("Laser.Orchard.QuestionnaireStatsExport")]
    public class QuestionnaireStatsSettingsPart : ContentPart {
        public bool EnablePdfExport {
            get { return this.Retrieve(x => x.EnablePdfExport); }
            set { this.Store(x => x.EnablePdfExport, value); }
        }

        public bool EnableExcelExport {
            get { return this.Retrieve(x => x.EnableExcelExport); }
            set { this.Store(x => x.EnableExcelExport, value); }
        }
    }
}