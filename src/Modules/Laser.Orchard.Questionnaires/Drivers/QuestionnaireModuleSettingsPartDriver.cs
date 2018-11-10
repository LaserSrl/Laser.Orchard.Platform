using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.Questionnaires.Drivers {

    public class QuestionnaireModuleSettingsPartDriver : ContentPartDriver<QuestionnaireModuleSettingsPart> {
        private const string TemplateName = "Parts/QuestionnaireModuleSettings";

        protected override string Prefix { get { return "QuestionnaireModuleSettingsPartDriver"; } }

        protected override DriverResult Editor(QuestionnaireModuleSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_QuestionnaireModuleSettings_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(QuestionnaireModuleSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}