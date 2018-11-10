using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.MultiStepAuthentication.Models {
    [OrchardFeature("Laser.Orchard.NonceTemplateEmail")]
    public class NonceTemplateSettingsPart: ContentPart<NonceTemplateSettingsPartRecord> {
        internal LazyField<TemplatePart> SelectedTemplateField = new LazyField<TemplatePart>();

        public TemplatePart SelectedTemplate {
            get { return SelectedTemplateField.Value; }
            set { SelectedTemplateField.Value = value; }
        }

        public CustomTemplatePickerViewModel ct { get; set; }
    }

    public class NonceTemplateSettingsPartRecord : ContentPartRecord {
        public virtual int? TemplateIdSelected { get; set; }
    }

}