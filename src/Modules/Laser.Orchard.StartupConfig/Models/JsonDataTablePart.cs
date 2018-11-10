using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Helpers;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTablePart : ContentPart<JsonDataTablePartRecord> {
        public string TableData {
            get {
                var aux = Retrieve(r => r.TableData);
                if (string.IsNullOrWhiteSpace(aux)) {
                    aux = "[]";
                }
                return aux;
            }
            set {
                if(string.IsNullOrWhiteSpace(value) == false) {
                    Store(r => r.TableData, value);
                }
            }
        }
        public DynamicJsonObject DataList {
            get {
                var arr = new List<object>();
                var aux = JArray.Parse(TableData);
                foreach (var el in aux) {
                    var a1 = el.ToObject<Dictionary<string, object>>();
                    arr.Add(new DynamicJsonObject(a1));
                }
                var jsonObj = new Dictionary<string, object>();
                jsonObj.Add(string.Format("{0}Rows", ContentItem.ContentType), arr.ToArray());
                return new DynamicJsonObject(jsonObj);
            }
        }
    }
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTablePartRecord : ContentPartVersionRecord {
        private string _tableData = "[]"; // default value
        public virtual string TableData {
            get {
                return _tableData;
            }
            set {
                if(string.IsNullOrWhiteSpace(value) == false) {
                    _tableData = value;
                }
            }
        }
    }
}