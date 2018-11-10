using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.TemplateManagement.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.TemplateManagement.ViewModels {
    public class CustomTemplatePickerViewModel {
        public int? TemplateIdSelected { get; set; }
        public IEnumerable<TemplatePart>  TemplatesList{ get; set; }

    }


}