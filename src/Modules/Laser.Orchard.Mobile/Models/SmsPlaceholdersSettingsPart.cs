using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Laser.Orchard.Mobile.Models {

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