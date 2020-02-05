using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Users.Models;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Data;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.CommunicationGateway.Models;
using Orchard.Logging;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.StartupConfig.Handlers;

namespace Laser.Orchard.Mobile.Handlers {
    public class UserDeviceHandler : IUserEventHandler {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<UserDeviceRecord> _userDeviceRecord;
        private readonly IRepository<PushNotificationRecord> _pushNotificationRecord;
        private readonly IRepository<CommunicationContactPartRecord> _communicationContactPartRecord;
        private readonly IContactRelatedEventHandler _contactEventHandler;
        private readonly IPushNotificationService _pushNotificationService;
        public ILogger Logger { get; set; }

        public UserDeviceHandler(
            IHttpContextAccessor httpContextAccessor,
            IRepository<UserDeviceRecord> userDeviceRecord,
            IRepository<PushNotificationRecord> pushNotificationRecord,
            IRepository<CommunicationContactPartRecord> communicationContactPartRecord,
            IContactRelatedEventHandler contactEventHandler,
            IPushNotificationService pushNotificationService
            ) {
            _httpContextAccessor = httpContextAccessor;
            _userDeviceRecord = userDeviceRecord;
            _pushNotificationRecord = pushNotificationRecord;
            _communicationContactPartRecord = communicationContactPartRecord;
            _contactEventHandler = contactEventHandler;
            _pushNotificationService = pushNotificationService;
            Logger = NullLogger.Instance;
        }
        public void AccessDenied(IUser user) {
            //  throw new NotImplementedException();
        }

        public void Approved(IUser user) {
            //  throw new NotImplementedException();
        }
        public void Moderate(IUser user) {
            //  throw new NotImplementedException();
        }

        public void ChangedPassword(IUser user) {
            //   throw new NotImplementedException();
        }

        public void ConfirmedEmail(IUser user) {
            //   throw new NotImplementedException();
        }

        public void Created(UserContext context) {
            //   throw new NotImplementedException();
        }

        public void Creating(UserContext context) {
            //   throw new NotImplementedException();
        }
        private string GetRequestKey(string name) {
            var result = "";
            var req = _httpContextAccessor.Current().Request;
            result = req.Headers["x-" + name];
            if (string.IsNullOrWhiteSpace(result)) {
                result = req.QueryString[name];
            }
            return result;
        }
        public void LoggedIn(IUser user) {
            // crea o aggiorna il contatto
            _contactEventHandler.Synchronize(user);
            // imposta UUID per il device
            var UUIdentifier = GetRequestKey("UUID");
            if (!string.IsNullOrWhiteSpace(UUIdentifier)) {
                var record = _userDeviceRecord.Fetch(x => x.UUIdentifier == UUIdentifier).FirstOrDefault();
                if (record == null) {
                    UserDeviceRecord newUD = new UserDeviceRecord();
                    newUD.UUIdentifier = UUIdentifier;
                    newUD.UserPartRecord = ((dynamic)user).Record;
                    _userDeviceRecord.Create(newUD);
                    _userDeviceRecord.Flush();
                }
                else {
                    if (record.UserPartRecord.Id != user.Id) {
                        record.UserPartRecord = ((dynamic)user).Record;
                        _userDeviceRecord.Update(record);
                        _userDeviceRecord.Flush();
                    }
                }

                // aggiorna il collegamento del device con il contact, se il device è registrato
                _pushNotificationService.UpdateDevice(UUIdentifier);
            }
        }

        public void LoggedOut(IUser user) {
            //   throw new NotImplementedException();
        }

        public void SentChallengeEmail(IUser user) {
            //   throw new NotImplementedException();
        }

        public void LoggingIn(string userNameOrEmail, string password) {
            //throw new NotImplementedException();
        }

        public void LogInFailed(string userNameOrEmail, string password) {
            //throw new NotImplementedException();
        }
    }
}