using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Services.ContentSerialization {
    public class SerializationSettings {

        public string[] SkipAlwaysProperties { get; set; }
        public string SkipAlwaysPropertiesEndWith { get; set; }
        public string[] SkipFieldProperties { get; set; }
        public string[] SkipFieldTypes { get; set; }
        public string[] SkipPartNames { get; set; }
        public string[] SkipPartProperties { get; set; }
        public string[] SkipPartTypes { get; set; }
        public Type[] BasicTypes { get; set; }
        public int MaxLevel { get; set; }


        // Back references to the calling service and its methods to allow recursing into them
        public IContentSerializationServices ContentSerializationService { get; set; }
        // This way we can use the same serializer configuration as the service
        public Func<JsonSerializer> SerializerFactory { get; set; }
        // This allows recursive invocations
        public Func<object, int, int, string[], JProperty> 
            ObjectSerializerMethod { get; set; }


        // Methods
        public bool IsBasicType(Type type) {
            return BasicTypes.Contains(type) || type.IsEnum;
        }

        public static void FormatValue(ref object val) {
            if (val != null && val.GetType().IsEnum) {
                val = val.ToString();
            }
        }

    }
}