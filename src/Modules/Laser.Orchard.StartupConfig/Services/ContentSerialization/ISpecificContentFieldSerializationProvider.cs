using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using System;

namespace Laser.Orchard.StartupConfig.Services.ContentSerialization {
    public interface ISpecificContentFieldSerializationProvider : IDependency {

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
        /// <param name="fieldToSerialize"></param>
        /// <returns></returns>
        bool IsSpecificForField(ContentField fieldToSerialize);

        /// <summary>
        /// Name used to differentiate the Field as a class.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        string ComputeFieldClassName(ContentField field, ContentItem item = null);

        /// <summary>
        /// Actual serialization step
        /// </summary>
        /// <param name="targetFieldObject"></param>
        /// <param name="fieldToSerialize"></param>
        /// <param name="actualLevel"></param>
        /// <param name="itemToSerialize"></param>
        void PopulateJObject(
            ref JObject targetFieldObject,
            ContentField fieldToSerialize,
            int actualLevel,
            ContentItem itemToSerialize = null);
    }
}
