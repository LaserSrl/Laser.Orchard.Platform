using Orchard.Localization;

namespace Laser.Orchard.Questionnaires.Helpers {
    public static class QuestionnaireHelper {
        public static Localizer T = NullLocalizer.Instance;
        public static string SettingsGroupId = T("Questionnaires").Text;
    }
}