using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Fields {
    public class HashedStringField : ContentField {
        public string Value {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value ?? string.Empty); }
        }

        public string HashAlgorithm {
            get { return Storage.Get<string>("HashAlgorithm"); }
            set { Storage.Set("HashAlgorithm", value ?? string.Empty); }
        }

        public string Salt {
            get { return Storage.Get<string>("Salt"); }
            set { Storage.Set("Salt", value ?? string.Empty); }
        }
    }
}