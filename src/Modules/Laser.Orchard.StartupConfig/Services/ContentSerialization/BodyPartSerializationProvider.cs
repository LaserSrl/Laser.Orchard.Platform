using Laser.Orchard.StartupConfig.Models;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls.WebParts;

namespace Laser.Orchard.StartupConfig.Services.ContentSerialization {
    public class BodyPartSerializationProvider :
        ISpecificContentPartSerializationProvider {

        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        private SerializationSettings CurrentSerializationSettings { get; set; }

        public BodyPartSerializationProvider(
            IEnumerable<IHtmlFilter> htmlFilters) {

            _htmlFilters = htmlFilters;
        }

        public int Specificity => 10;

        public void Configure(SerializationSettings serializationSettings) {
            CurrentSerializationSettings = serializationSettings;
        }

        public bool IsSpecificForPart(ContentPart partToSerialize) {
            return partToSerialize is BodyPart;
        }

        public void PopulateJObject(
            ref JObject targetPartObject,
            ContentPart partToSerialize,
            IEnumerable<PropertyInfo> propertiesToSerialize,
            Action<JObject, PropertyInfo, object> defaultSerialization
            ) {

            foreach (var property in propertiesToSerialize) {
                object val = property
                    .GetValue(partToSerialize, BindingFlags.GetProperty, null, null, null);
                if (property.Name.Equals("Text", StringComparison.InvariantCultureIgnoreCase)) {
                    var flavor = GetFlavor(partToSerialize as BodyPart);
                    val = _htmlFilters.Aggregate(
                        val.ToString(), 
                        (text, filter) => filter.ProcessContent(text, flavor));
                }
                defaultSerialization(targetPartObject, property, val);
            }
        }
        private static string GetFlavor(BodyPart part) {
            var typePartSettings = part.Settings.GetModel<BodyTypePartSettings>();
            return (typePartSettings != null && !string.IsNullOrWhiteSpace(typePartSettings.Flavor))
                       ? typePartSettings.Flavor
                       : part.PartDefinition.Settings.GetModel<BodyPartSettings>().FlavorDefault;
        }
    }
}