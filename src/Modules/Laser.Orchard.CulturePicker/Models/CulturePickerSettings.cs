using Orchard.ContentManagement.Records;
using System.Collections.Generic;
using System.ComponentModel;

namespace Laser.Orchard.CulturePicker.Models {
    public class ExtendedCultureRecord {
        public virtual int Id { get; set; }
        public virtual string CultureCode { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual int Priority { get; set; }
    }

    public class SettingsModel {
        public virtual SettingsRecord Settings { get; set; }
        public virtual IList<ExtendedCultureRecord> ExtendedCulturesList { get; set; }
    }

    public class SettingsRecord {
        public SettingsRecord() {
            Id = 0;
            ShowLabel = true;
            ShowOnlyPertinentCultures = false;
        }
        public virtual int Id { get; set; }
        public virtual bool ShowLabel { get; set; }
        public virtual bool ShowOnlyPertinentCultures { get; set; }

    }

}