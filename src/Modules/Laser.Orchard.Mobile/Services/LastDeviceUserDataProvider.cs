using Laser.Orchard.Mobile.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Security;
using Orchard.Security.Providers;
using Orchard.Settings;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Mobile.Services {
    public class LastDeviceUserDataProvider : BaseUserDataProvider {

        private readonly ISiteService _siteService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRepository<LatestUUIDForUserRecord> _latestUUIDForUserRepository;

        public LastDeviceUserDataProvider(
            ISiteService siteService,
            IWorkContextAccessor workContextAccessor,
            IRepository<LatestUUIDForUserRecord> latestUUIDForUserRepository) 
            : base(true) {

            // base(true) sets base.DefaultValid = true, but we are going to override the
            // IsValid behaviour anyway, as well as the DefaultValid value

            _siteService = siteService;
            _workContextAccessor = workContextAccessor;
            _latestUUIDForUserRepository = latestUUIDForUserRepository;
        }

        protected override bool DefaultValid {
            get {
                return !AuthenticateOnlyLatestUUID;
            }
        }

        private bool AuthenticateOnlyLatestUUID {
            get {
                return _siteService
                  .GetSiteSettings()
                  .As<LastDeviceUserDataProviderSettingsPart>()
                  .AuthenticateOnlyLatestUUID;
            }
        }

        protected override string Value(IUser user) {
            // This will be used to provide the value to put in the UserData dictionary
            // upon user's SignIn

            var callUUID = GetCallUUID();

            if (!string.IsNullOrWhiteSpace(callUUID)) {
                if (TryUpdateLatestRecord(user, callUUID)) {
                    return callUUID;
                }
            }
            // TODO: decide what should be done here
            return null;
            // null values are not inserted in the UserDataDictionary
        }

        public override bool IsValid(IUser user, IDictionary<string, string> userData) {
            if (!AuthenticateOnlyLatestUUID) {
                return true; // TODO: expand here using further settings
            }

            // Get the UUID from all possible sources
            var latestUUID = GetLatestUUID(user); // from storage
            var callUUID = GetCallUUID(); //from request
            var userDataUUID = GetUserDataUUID(userData); //from authentication cookie

            // Depending on which one of the UUID is not null, we have 8 different cases
            var condition = latestUUID != null ? 1 : 0;
            condition += callUUID != null ? 2 : 0;
            condition += userDataUUID != null ? 4 : 0;
            // TODO: I don't like switch statements. There may be a way to order the terms 
            // in condition to make this cleaner, and possibly remove the switch entirely
            switch (condition) {
                case 0: // UUIDs are all null
                    return OldSignIn(); 
                case 1:
                    return OtherSignIn();
                case 2:
                    return NeverRegisteredLogin();
                case 3:
                    return CookieTamperedWith();
                case 4:
                    return CookieTamperedWith();
                case 5:
                    return CookieTamperedWith();
                case 6:
                    return NeverRegisteredLogin();
                case 7:
                    if (!callUUID.Equals(latestUUID, StringComparison.Ordinal)) {
                        return WrongDevice();
                    }
                    if (!callUUID.Equals(userDataUUID, StringComparison.Ordinal)) {
                        return CookieTamperedWith();
                    }
                    return Valid(); // UUIDs all match each other
            }
            
            return DefaultValid; //Should never fall here
        }

        /// <summary>
        /// Fetch the UUID the user used in its latest SignIn
        /// </summary>
        /// <param name="user">The user we are considering</param>
        /// <returns>The UUID, or null if it does not exist.</returns>
        private string GetLatestUUID(IUser user) {

            var latestRecord = GetLatestRecord(user);
            if (latestRecord == null) {
                // we never registered a sign in for this user
                return null;
            }

            return latestRecord.UUID;
        }

        /// <summary>
        /// Fetch the record where we stored the UUID for the user in their most recent
        /// SignIn.
        /// </summary>
        /// <param name="user">The user we are considering</param>
        /// <returns>The record, or null if it does not exist</returns>
        private LatestUUIDForUserRecord GetLatestRecord(IUser user) {
            var userPart = user.As<UserPart>();
            if (userPart == null) {
                return null;
            }

            var record = userPart.Record;
            if (record == null) { // sanity check
                return null;
            }

            var latestRecord = _latestUUIDForUserRepository
                .Fetch(luu => luu.UserPartRecord == record)
                .FirstOrDefault();

            return latestRecord;
        }

        private bool TryUpdateLatestRecord(IUser user, string uuid) {
            var latestRecord = GetLatestRecord(user);
            if (latestRecord == null) {
                // create
                var userPart = user.As<UserPart>();
                if (userPart == null) { // sanity check
                    return false;
                }

                var record = userPart.Record;
                if (record == null) { // sanity check
                    return false;
                }
                latestRecord = new LatestUUIDForUserRecord {
                    UserPartRecord = record,
                    UUID = uuid
                };
                _latestUUIDForUserRepository.Create(latestRecord);
            } else {
                // update
                latestRecord.UUID = uuid;
                _latestUUIDForUserRepository.Update(latestRecord);
            }
            return true;
        }

        /// <summary>
        /// Fetch the UUID recorded in the UserData dictionary of the authentication cookie.
        /// </summary>
        /// <param name="userData">The UserData dictionary from the authentication cookie.</param>
        /// <returns>The UUID, or null if it does not exist.</returns>
        private string GetUserDataUUID(IDictionary<string, string> userData) {
            if (!userData.ContainsKey(Key)) {
                return null;
            }
            return userData[Key];
        }

        /// <summary>
        /// Fetch the UUID passed with the current request
        /// </summary>
        /// <returns>The UUID, or null if it does not exist.</returns>
        private string GetCallUUID() {
            var request = _workContextAccessor.GetContext()
                .HttpContext
                .Request;

            if (request.Headers["x-uuid"] != null) {
                return request.Headers["x-uuid"].ToString();
            }

            // if the UUID is not in the header, see if it is in query string 
            if (request.QueryString["x-uuid"] != null) {
                return request.QueryString["x-uuid"].ToString();
            }

            return null;
        }

        #region Response templates for IsValid()
        // Using these rather than just returning the boolean allows us to differentiate the cases, and
        // and eventually trigger different behaviors for the different situations, even though the IsValid()
        // response may be the same. e.g. we may want to log things differently.

        /// <summary>
        /// The cookie we received has been tampered with or changed (e.g. used form a different client),
        /// so the values in the UserData dictionary don't belong.
        /// </summary>
        /// <returns></returns>
        private bool CookieTamperedWith() {
            return false;
        }
        /// <summary>
        /// The user had signed in at a time this system was not yet in place, so it's impossible to verify
        /// that everything is correct. However, we don't want to lock them out.
        /// </summary>
        /// <returns></returns>
        private bool OldSignIn() {
            // TODO: figure out how to handle this properly
            return true;
        }
        /// <summary>
        /// The request comes form a differnt client that one of our configured mobile applications, so it's
        /// not necessarily required to respect our constraints. Additional settings should tell how to manage
        /// this situation.
        /// </summary>
        /// <returns></returns>
        private bool OtherSignIn() {
            // TODO: add a setting to manage this case (user accessing not from our app, but e.g. from a browser)
            return true;
        }
        /// <summary>
        /// The request comes from a different device than the one used for the latest SignIn for the user,
        /// so we refuse it.
        /// </summary>
        /// <returns></returns>
        private bool WrongDevice() {
            return false;
        }
        /// <summary>
        /// We never recorded a valid SignIn from any device, and now we are receiving a call from one, so we
        /// block it.
        /// </summary>
        /// <returns></returns>
        private bool NeverRegisteredLogin() {
            return false;
        }
        /// <summary>
        /// From the information we had, everything looked to be fine.
        /// </summary>
        /// <returns></returns>
        private bool Valid() {
            return true;
        }
        #endregion
    }
}