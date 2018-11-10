using Laser.Orchard.UserProfiler.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.UserProfiler.Handlers {

    public class UserProfilingSettingHandler : ContentHandler {

        public UserProfilingSettingHandler() {
            Filters.Add(new ActivatingFilter<UserProfilingSettingPart>("Site"));
        }
    }
}