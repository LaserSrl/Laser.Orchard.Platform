using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.Questionnaires.Handlers {

    public class QuestionnaireModuleSettingsHandler : ContentHandler {

        public QuestionnaireModuleSettingsHandler() {
            Filters.Add(new ActivatingFilter<QuestionnaireModuleSettingsPart>("Site"));
        }
    }
}