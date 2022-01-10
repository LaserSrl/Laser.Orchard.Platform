using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;

namespace Laser.Orchard.TemplateManagement.ViewModels {
    public class TemplateViewModel {
        [Required, StringLength(256)]
        public string Title { get; set; }

        [Required, StringLength(256)]
        public string Subject { get; set; }
        public string Text { get; set; }

        [UIHint("TemplateLayoutPicker")]
        public int? LayoutIdSelected { get; set; }
        public bool IsLayout { get; set; }
        public IList<TemplatePart> Layouts { get; set; }
        public IParserEngine ExpectedParser { get; set; }
        public string TemplateCode { get; set; }
    }
}