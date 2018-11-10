using Orchard.UI.Resources;

namespace Laser.Orchard.Questionnaires {

    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            //Scripts
            //Styles
            manifest.DefineStyle("Questionnaire").SetUrl("orchard.questionnaire.css");
            manifest.DefineStyle("QuestionnaireAdmin").SetUrl("questionnaire-admin.css");

        }
    }
}