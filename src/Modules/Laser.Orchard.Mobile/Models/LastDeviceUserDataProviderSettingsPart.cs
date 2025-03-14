using Orchard.ContentManagement;

namespace Laser.Orchard.Mobile.Models {
    public class LastDeviceUserDataProviderSettingsPart : ContentPart {

        /// <summary>
        /// This flag controls the behaviour of LastDeviceUserDataProvider.
        /// If the setting is true, we register the UUID from the SignIn call for a user
        /// in the UserData dictionary as well as in a record. When we receive a call and have
        /// validate the User, we compare the call's UUID, the registered UUID and the UUID
        /// from the UserData we received, and only validate if they all match.
        /// </summary>
        public bool AuthenticateOnlyLatestUUID {
            get { return this.Retrieve(x => x.AuthenticateOnlyLatestUUID); }
            set { this.Store(x => x.AuthenticateOnlyLatestUUID, value); }
        }

    }
}