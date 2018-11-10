using System;
using System.Collections.Generic;
using Laser.Orchard.StartupConfig.Fields;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Laser.Orchard.StartupConfig.ColorPicker {
    public class ColorPickerField : ContentField, ICustomField {
        public string Value {
            get {
                return Storage.Get<string>();
            }
            set {
                Storage.Set(value);
            }
        }
        public List<CustomFieldValue> GetFieldValueList() {
            List<CustomFieldValue> result = new List<CustomFieldValue>();
            CustomFieldValue field = new CustomFieldValue {
                ValueName = "",
                Value = Value,
                ValueType = typeof(string)
            };
            result.Add(field);
            return result;
        }
        public void SetFieldValue(string valueName, object value) {
            if (valueName == "") {
                Value = (string)value;
            }
        }
    }
}