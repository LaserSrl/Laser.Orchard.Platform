using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Laser.Orchard.SecureData.Fields {
    public class EncryptedStringField : ContentField {
        public string Value {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value ?? string.Empty); }
        }
    }
}