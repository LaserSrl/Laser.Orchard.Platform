using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Fields.Fields;
using System;

namespace Laser.Orchard.StartupConfig.Services.ContentSerialization {
    public class EnumerationFieldSerializationProvider : ISpecificContentFieldSerializationProvider {

        public EnumerationFieldSerializationProvider() { }

        public int Specificity => 10;

        public string ComputeFieldClassName(ContentField field, 
            ContentPart part = null, ContentItem item = null) {
            return field.FieldDefinition.Name;
        }

        public  void Configure(SerializationSettings serializationSettings) { }

        public  bool IsSpecificForField(ContentField fieldToSerialize) {
            return fieldToSerialize is EnumerationField;
        }

        public  void PopulateJObject(
            ref JObject targetFieldObject,
            ContentField fieldToSerialize,
            int actualLevel,
            ContentItem itemToSerialize = null) {

            var enumField = (EnumerationField)fieldToSerialize;
            string[] selected = enumField.SelectedValues;
            string[] options = enumField.PartFieldDefinition
                .Settings["EnumerationFieldSettings.Options"]
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            targetFieldObject.Add("Options", JToken.FromObject(options));
            targetFieldObject.Add("SelectedValues", JToken.FromObject(selected));
        }
    }
}