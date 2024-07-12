using Laser.Orchard.Questionnaires.Helpers;
using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Questionnaires.Drivers {
    [OrchardFeature("Laser.Orchard.QuestionnaireStatsExport")]
    public class QuestionnaireStatsSettingsPartDriver : ContentPartDriver<QuestionnaireStatsSettingsPart> {
        public const string TemplateName = "Parts/QuestionnaireStatsSettings";
        
        protected override string Prefix { get { return "QuestionnaireStatsSettingsPartDriver"; } }

        protected override DriverResult Editor(QuestionnaireStatsSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_QuestionnaireStatsSettings_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: part,
                    Prefix: Prefix))
                .OnGroup(QuestionnaireHelper.SettingsGroupId);
        }

        protected override DriverResult Editor(QuestionnaireStatsSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}