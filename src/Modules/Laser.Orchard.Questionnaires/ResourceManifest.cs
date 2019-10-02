using Orchard.UI.Resources;

namespace Laser.Orchard.Questionnaires {

    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            //Scripts
            //Styles
            manifest.DefineStyle("Questionnaire").SetUrl("orchard.questionnaire.css?v=1.0");
            manifest.DefineStyle("QuestionnaireAdmin").SetUrl("questionnaire-admin.css?v=1.0");

        }
    }
}