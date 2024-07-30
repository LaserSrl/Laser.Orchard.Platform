using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Questionnaires.Handlers {

    public class QuestionnaireModuleSettingsHandler : ContentHandler {

        public Localizer T { get; set; }
        public QuestionnaireModuleSettingsHandler() {
            Filters.Add(new ActivatingFilter<QuestionnaireModuleSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<QuestionnaireModuleSettingsPart>((context, part) =>
                context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Questionnaires"))));
        }
    }
}