using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.Questionnaires.Handlers {
    [OrchardFeature("Laser.Orchard.QuestionnaireStatsExport")]
    public class QuestionnaireStatsSettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }
        public QuestionnaireStatsSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<QuestionnaireStatsSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<QuestionnaireStatsSettingsPart>((context, part) =>
                context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Questionnaires"))));
        }
    }
}