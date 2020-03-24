using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services.JsonConverters {
    public class PriceFormatConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(decimal);
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            // won't be called because can read is false
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteRawValue($"\"{((decimal)value).ToString("0.00", CultureInfo.InvariantCulture)}\"");
        }
    }
}