using Laser.Orchard.HiddenFields.Settings;
using Laser.Orchard.StartupConfig.Fields;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Fields {
    public class HiddenStringField : ContentField, ICustomField {
        public string Value {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value); }
        }

        public List<CustomFieldValue> GetFieldValueList() {
            List<CustomFieldValue> result = new List<CustomFieldValue>();
            CustomFieldValue field = new CustomFieldValue {
                ValueName = "",
                Value = this.Value,
                ValueType = typeof(string)
            };
            result.Add(field);
            return result;
        }
        public void SetFieldValue(string valueName, object value) {
            if (valueName == "") {
                this.Value = (string)value;
            }
        }
    }
}