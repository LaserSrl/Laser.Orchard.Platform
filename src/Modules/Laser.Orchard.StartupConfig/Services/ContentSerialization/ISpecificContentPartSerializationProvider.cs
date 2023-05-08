using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.StartupConfig.Services.ContentSerialization {
    public interface ISpecificContentPartSerializationProvider : IDependency {

        /// <summary>
        /// How specific the implementation is. Used for ordering.
        /// </summary>
        int Specificity { get; }

        /// <summary>
        /// Get parameters from the ContentSerializationService and save those
        /// we'll need for the specific serialization this provider will do. This should
        /// be invoked once, when initializing the ContentSerializationService.
        /// </summary>
        void Configure(SerializationSettings serializationSettings);

        /// <summary>
        /// Allow runtime selection of which providers to use
        /// </summary>
        /// <param name="partToSerialize"></param>
        /// <returns></returns>
        bool IsSpecificForPart(ContentPart partToSerialize);


        void PopulateJObject(
            ref JObject targetPartObject,
            ContentPart partToSerialize,
            IEnumerable<PropertyInfo> propertiesToSerialize,
            Action<JObject, PropertyInfo, object> defaultSerialization
            );

    }
}
