using Laser.Orchard.StartupConfig.Fields;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTableField : ContentField {
        public string TableData {
            get {
                var aux = Storage.Get<string>("TableData");
                if (string.IsNullOrWhiteSpace(aux)) {
                    aux = "[]";
                }
                return aux;
            }
            set {
                if (string.IsNullOrWhiteSpace(value) == false) {
                    Storage.Set("TableData", value);
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
                jsonObj.Add(string.Format("{0}Rows", this.PartFieldDefinition.Name + "_" + Name), arr.ToArray());
                return new DynamicJsonObject(jsonObj);
            }
        }
    }
}