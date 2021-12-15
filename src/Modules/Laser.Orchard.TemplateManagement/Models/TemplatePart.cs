using Laser.Orchard.TemplateManagement.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using Orchard.Data.Conventions;

namespace Laser.Orchard.TemplateManagement.Models {
    public class TemplatePart : ContentPart<TemplatePartRecord>, ITitleAspect {
        internal LazyField<TemplatePart> LayoutField = new LazyField<TemplatePart>();

        public string Title {
            get { return this.Retrieve(r => r.Title); }
            set { this.Store(r => r.Title, value); }

        }

        public string Subject {
            get { return this.Retrieve(r => r.Subject); }
            set { this.Store(r => r.Subject, value); }

        }

        public string Text {
            get { return this.Retrieve(r => r.Text); }
            set { this.Store(r => r.Text, value); }
        }

        public bool IsLayout {
            get { return this.Retrieve(r => r.IsLayout); }
            set { this.Store(r => r.IsLayout, value); }
        }

        public TemplatePart Layout {
            get { return LayoutField.Value; }
            set { LayoutField.Value = value; }
        }

        public string DefaultParserIdSelected {
            get { return Settings.GetModel<TemplatePartSettings>().DefaultParserIdSelected; }
        }

        public string TemplateCode {
            get { return this.Retrieve(r => r.TemplateCode); }
            set { this.Store(r => r.TemplateCode, value); }
        }
    }

    public class TemplatePartRecord : ContentPartRecord {
        public virtual string Title { get; set; }
        public virtual string Subject { get; set; }

        [StringLengthMax]
        public virtual string Text { get; set; }
        public virtual int? LayoutIdSelected { get; set; }
        public virtual bool IsLayout { get; set; }
        public virtual string TemplateCode { get; set; }
    }
}