using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.Services.ContentSerialization;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.StartupConfig.Providers {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTableFieldSerializationProvider : ISpecificContentFieldSerializationProvider {
        private readonly IJsonDataTableService _jsonDataTableService;

        public JsonDataTableFieldSerializationProvider(IJsonDataTableService jsonDataTableService) {
            _jsonDataTableService = jsonDataTableService;
        }

        public int Specificity => 10;

        public string ComputeFieldClassName(ContentField field, ContentPart part = null, ContentItem item = null) {
            return field.FieldDefinition.Name;
        }

        public void Configure(SerializationSettings serializationSettings) {
            
        }

        public bool IsSpecificForField(ContentField fieldToSerialize) {
            //return false;
            return fieldToSerialize is JsonDataTableField;
        }

        public void PopulateJObject(ref JObject targetFieldObject, ContentField fieldToSerialize, int actualLevel, ContentItem itemToSerialize = null) {
            var jdtField = (JsonDataTableField)fieldToSerialize;
            targetFieldObject.Add("TableData", JToken.FromObject(jdtField.TableData));
            targetFieldObject.Add("DataList", JToken.FromObject(jdtField.DataList));
            targetFieldObject.Add("Rows", _jsonDataTableService.SerializeData(jdtField));
        }
    }
}