using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;

namespace Laser.Orchard.TemplateManagement.Models {
    public class CustomTemplatePickerPart : ContentPart<CustomTemplatePickerPartRecord> {

        internal LazyField<TemplatePart> SelectedTemplateField = new LazyField<TemplatePart>();

        public TemplatePart SelectedTemplate {
            get { return SelectedTemplateField.Value; }
            set { SelectedTemplateField.Value = value; }
        }
    }

    public class CustomTemplatePickerPartRecord : ContentPartRecord {
        public virtual int? TemplateIdSelected { get; set; }
    }

}