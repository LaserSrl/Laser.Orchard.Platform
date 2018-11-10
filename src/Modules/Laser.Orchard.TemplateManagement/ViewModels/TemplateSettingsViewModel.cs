using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.TemplateManagement.ViewModels {
	public class TemplateSettingsViewModel {
		[UIHint("ParserPicker")]
        public string ParserIdSelected { get; set; }
	}
}