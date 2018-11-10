using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Settings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Events;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.HiddenFields.Services {
    public enum HiddenStringFieldUpdateProcessVariant {
        None,
        All,
        Empty
    }
    public interface IHiddenStringFieldUpdateProcessor : IEventHandler {
        /// <summary>
        /// Runs the processing for the fields.
        /// </summary>
        /// <param name="contentItemIds">The Ids of the affected content items.</param>
        /// <param name="partName">The name of the part that contains the fields to process.</param>
        /// <param name="fieldName">The name of the field to be updated</param>
        /// <param name="settings">The settings for the fields being updated. The same settings will be used for all fields processed.</param>
        void Process(IEnumerable<int> contentItemIds, string partName, string fieldName, HiddenStringFieldSettings settings);
        /// <summary>
        /// Gets a IEnumerable of the variants for processing in a form to be used for dropdown in a razor view.
        /// </summary>
        /// <returns>An IEnumerable of SelectListItem objects representing the variants</returns>
        IEnumerable<SelectListItem> GetVariants();
        /// <summary>
        /// Schedules the processing task given by the variant and using the information from the definition builder
        /// </summary>
        /// <param name="variant">The processing variant selected</param>
        /// <param name="builder">The definition builder for the settings that have changed</param>
        void AddTask(HiddenStringFieldUpdateProcessVariant variant, HiddenStringFieldSettings settings, ContentPartFieldDefinitionBuilder builder);
    }
}
