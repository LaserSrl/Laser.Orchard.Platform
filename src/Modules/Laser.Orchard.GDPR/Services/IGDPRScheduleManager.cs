using Laser.Orchard.GDPR.Settings;
using Orchard;

namespace Laser.Orchard.GDPR.Services {
    public interface IGDPRScheduleManager : IDependency {

        /// <summary>
        /// Schedules or unschedules the task to process ContentItems of the given type. The details on the
        /// processes that shall be performed are in the settings object.
        /// </summary>
        /// <param name="typeName">The name of the ContentType the items of which will be processed.</param>
        /// <param name="settings">The object containing the configuration for the specific processing.</param>
        /// <remakrs>This method should be called before actually updating the settings for the type. As a
        /// consequence, the settings object may be different from the one we would get by looking at the
        /// settings for the type: we want this so that we may chekc for any change that might have happened.</remakrs>
        void UpdateSchedule(string typeName, GDPRPartTypeSchedulingSettings settings);
        
    }
}
