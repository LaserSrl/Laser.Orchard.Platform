using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.TemplateManagement.Settings {

    public class TemplatePartSettings {
        [UIHint("ParserPicker")]
        public string DefaultParserIdSelected { get; set; }
    }
}
