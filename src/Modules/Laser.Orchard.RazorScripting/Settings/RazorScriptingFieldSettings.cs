namespace Laser.Orchard.RazorScripting.Settings {
    public class RazorScriptingFieldSettings {

        // Razor script that will get executed when buildling the editor for the content
        public string GetEditorScript { get; set; }
        // Razor script that will get executed when saving/publishing the content
        public string PostEditorScript { get; set; }
        // Razor script that will get executed when building the content's display shapes
        public string DisplayScript { get; set; }
    }
}