using Laser.Orchard.ExternalContent.Fields;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.Services.ContentSerialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Helpers;

namespace Laser.Orchard.ExternalContent.Services {
    public class ExternalFieldSerializationProvider : ISpecificContentFieldSerializationProvider {
        public int Specificity => 10;

        private SerializationSettings CurrentSerializationSettings { get; set; }
        private const string SerializedPropertyName = "ContentObject";

        public string ComputeFieldClassName(ContentField field, ContentItem item = null) {
            var fieldExternal = (FieldExternal)field;
            if (fieldExternal == null) {
                return field.FieldDefinition.Name;
            }
            // {ContentType}.{ContentPart.PartDefinition.Name}.{field.FieldDefinition.Name}.{field.Name};
            var classNameElements = new List<string>();
            if (item != null) {
                classNameElements.Add(item.ContentType);
                // find the part the field is in
                var parts = item.Parts
                    .Where(pa => pa.Fields
                        .Any(fi => fi.Name.Equals(field.Name) 
                            && fi is FieldExternal));
                if (parts.Any()) { // sanity check
                    var part = (ContentPart)null;
                    if (parts.Count() == 1) {
                        // "normal" healthy case
                        part = parts.FirstOrDefault();
                    } else {
                        // FieldExternal with the same name in different ContentParts.
                        // Check other properties of the field to compare with the ones from
                        // the parts.
                        part = parts.FirstOrDefault(pa => {
                            var candidateField = pa.Fields
                                .FirstOrDefault(fi => fi is FieldExternal
                                    && fi.Name.Equals(field.Name)) as FieldExternal;
                            return string.Equals(fieldExternal.DisplayName, candidateField.DisplayName)
                                && string.Equals(fieldExternal.ExternalUrl, candidateField.ExternalUrl)
                                && string.Equals(fieldExternal.HttpVerbCode, candidateField.HttpVerbCode)
                                && string.Equals(fieldExternal.HttpDataTypeCode, candidateField.HttpDataTypeCode)
                                && string.Equals(fieldExternal.BodyRequest, candidateField.BodyRequest)
                                && string.Equals(fieldExternal.AdditionalHeadersText, candidateField.AdditionalHeadersText);
                            });
                    }
                    if (part != null) {
                        // we've managed to identify the ContentPart where the ContentField is in
                        classNameElements.Add(part.PartDefinition.Name);
                    }
                }
            }
            classNameElements.Add(field.FieldDefinition.Name); // This is "FieldExternal"
            classNameElements.Add(field.Name); // Technical Name of the ContentField
            return string.Join("", classNameElements);
        }

        public void Configure(SerializationSettings serializationSettings) {
            CurrentSerializationSettings = serializationSettings;
        }

        public bool IsSpecificForField(ContentField fieldToSerialize) {
            return fieldToSerialize is FieldExternal;
        }

        public void PopulateJObject(
            ref JObject targetFieldObject,
            ContentField fieldToSerialize,
            int actualLevel,
            ContentItem itemToSerialize = null) {

            var field = (FieldExternal)fieldToSerialize;
            // The only property we should serialize here is ContentObject.
            // Either the call or the management of its response may have failed:
            if (field.ContentObject is Exception) {
                return;
            }

            // If we serialized it using default methods (i.e. using reflection), the result would be:
            /*
             * "ContentObject": {
	         *   "ToRemove": {
		     *     "Type": [
			 *       {
			 *	       "prop": "val",
			 *	       ...
			 *       },
			 *       {
			 *	       "prop": "val",
			 *	       ...
			 *       }
		     *     ]
	         *   }
             * }
             * */
            // There may be several nested "ToRemove" levels, depending on the details of how the
            // results from the call to the external service are parsed.

            // Downstream clients of the ContentSerialization services expect the results in a different
            // format:
            /*
             * "ContentObject": [
             *   {
             *     "Type": {
             *       "prop": "val",
             *       ...
             *     }
             *   },
             *   {
             *     "Type": {
             *       "prop": "val",
             *       ...
             *     }
             *   }
             * ]
             * */
            // To begin with, we should remove all the "ToRemove" levels".
            var contentObject = CleanContentObject(field.ContentObject);
            if (contentObject == null) {
                return; // sanity check
            }
            var transformedObject = new List<dynamic>();
            // Then we should wrap each object in the array, so it carries information on its type.
            var dynamicMemberNames = ((DynamicJsonObject)contentObject).GetDynamicMemberNames();
            foreach (var memberName in dynamicMemberNames) {
                if (string.IsNullOrWhiteSpace(memberName) // sanity check
                    || string.Equals(memberName, "ToRemove", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                var arrayMember = contentObject[memberName];
                if (contentObject[memberName] == null) {
                    continue;
                }
                if (contentObject[memberName].GetType().Equals(typeof(DynamicJsonArray))) {
                    // this member of the dynamic object is one of those arrays. The memberName
                    // is "Type" from the comments/examples above.
                    // The elements in the array may be of a basic type.
                    if (CurrentSerializationSettings.IsBasicType(arrayMember[0].GetType())) {
                        // e.g. the result from the call to the external service is a list of numbers
                        // or some other basic type like that.
                        foreach (var item in arrayMember) {
                            transformedObject.Add(item);
                        }
                    } else {
                        // this is the case from the comments/examples above.
                        // We need to "wrap" each object here in its "Type".
                        foreach (var item in arrayMember) {
                            var itemModel = new Dictionary<string, object>();
                            itemModel.Add(memberName, item);
                            transformedObject.Add((dynamic)itemModel);
                        }
                    }
                }
            }

            // Then we should serialize whatever that is, iteratively/recursively.

            // Finally, we add that result to the serialization we are building
            //targetFieldObject.Add("ContentObject", JToken.FromObject(transformedObject));
            PopulateJObject(
                ref targetFieldObject,
                transformedObject,
                actualLevel,
                itemToSerialize?.Id ?? 0);
        }

        private dynamic CleanContentObject(dynamic objec) {
            if (objec != null)
                if (objec.ToRemove != null) {
                    return CleanContentObject(objec.ToRemove);
                }
            return objec;
        }
        
        private void PopulateJObject(
            ref JObject jObject,
            object val,
            int actualLevel,
            int parentContentId) {
            // this replicates the PopulateJObject from ContentSerializationServices
            JObject propertiesObject;
            var serializer = CurrentSerializationSettings.SerializerFactory();
            if (val is Array || val.GetType().IsGenericType) {
                JArray array = new JArray();
                foreach (var itemArray in (IEnumerable)val) {

                    if (!CurrentSerializationSettings.IsBasicType(itemArray.GetType())) {
                        var aux = CurrentSerializationSettings.ObjectSerializerMethod
                            (itemArray, actualLevel, parentContentId, CurrentSerializationSettings.SkipFieldProperties);
                        array.Add(new JObject(aux));
                    } else {
                        var valItem = itemArray;
                        SerializationSettings.FormatValue(ref valItem);
                        array.Add(valItem);
                    }
                }
                jObject.Add(new JProperty(SerializedPropertyName, array));

            } else if (val is ContentItem) {
                var contentProperty = CurrentSerializationSettings.ObjectSerializerMethod
                    (val, actualLevel, parentContentId, null);
                jObject.Add(new JProperty(SerializedPropertyName, new JObject(contentProperty)));
            }
            if (!CurrentSerializationSettings.IsBasicType(val.GetType())) {
                try {
                    propertiesObject = JObject.FromObject(val, serializer);
                    foreach (var skip in CurrentSerializationSettings.SkipFieldProperties) {
                        propertiesObject.Remove(skip);
                    }
                    jObject.Add(SerializedPropertyName, propertiesObject);
                } catch {
                    jObject.Add(new JProperty(SerializedPropertyName, val.GetType().FullName));
                }
            } else {
                SerializationSettings.FormatValue(ref val);
                jObject.Add(new JProperty(SerializedPropertyName, val));
            }
        }
    }
}