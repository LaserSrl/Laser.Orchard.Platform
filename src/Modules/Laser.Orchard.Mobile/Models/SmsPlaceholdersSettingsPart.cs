using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Models {

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsPlaceholdersSettingsPart : ContentPart {
        private SmsPleaceholdersList _placeholdersList;
        public SmsPleaceholdersList PlaceholdersList {
            get {
                if (_placeholdersList == null) {
                    var json = Retrieve<string>("Placeholders");
                    if (json == null) return new SmsPleaceholdersList();
                    _placeholdersList = new JavaScriptSerializer().Deserialize<SmsPleaceholdersList>(json);
                }
                return _placeholdersList;
            }
            set {
                var json = new JavaScriptSerializer().Serialize(value);
                _placeholdersList = value;
                this.Store("Placeholders", json);
            }
        }
    }

    public class SmsPleaceholdersList {
        public SmsPleaceholdersList() {
            Placeholders = new List<SmsPlaceholder>();
        }

        public IEnumerable<SmsPlaceholder> Placeholders { get; set; }
    }

    public class SmsPlaceholder {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public bool Delete { get; set; }
    }
}