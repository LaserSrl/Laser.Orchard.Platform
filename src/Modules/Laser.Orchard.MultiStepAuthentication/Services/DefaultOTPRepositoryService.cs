using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard.Security;
using Orchard.Data;
using System.Linq.Expressions;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.MultiStepAuthentication")]
    public class DefaultOTPRepositoryService : IOTPRepositoryService {

        private readonly IRepository<OTPRecord> _repository;

        public DefaultOTPRepositoryService(
            IRepository<OTPRecord> repository) {

            _repository = repository;
        }

        public OTPRecord AddOTP(OTPRecord otp) {
            var userId = otp.UserRecord.Id;
            // delete all expired records for the user
            DeleteExpired(userId, otp.PasswordType);
            // create the new record in the db
            _repository.Create(otp);
            return otp;
        }

        public void Delete(OTPRecord otp) {
            if (otp != null) {
                Delete(otp.Id); // This prevents exceptions from weird data.
            }
        }

        private void Delete(int id) {
            var otp = _repository.Get(id);
            if (otp != null) {
                _repository.Delete(otp);
            }
        }

        public void Delete(IUser user, string OTPType = null) {
            var records = Get(user, OTPType);
            foreach (var otp in records) {
                Delete(otp.Id);
            }
        }

        public void DeleteExpired(string OTPType = null) {
            Expression<Func<OTPRecord, bool>> query;
            var utcNow = DateTime.UtcNow;
            if (OTPType == null) {
                query = or => or.ExpirationUTCDate < utcNow;
            } else {
                query = or => or.PasswordType == OTPType && or.ExpirationUTCDate < utcNow;
            }
            var records = _repository.Fetch(query);
            foreach (var otp in records) {
                Delete(otp.Id);
            }
        }

        public void DeleteExpired(IUser user, string OTPType = null) {
            DeleteExpired(user.Id, OTPType);
        }

        private void DeleteExpired(int userId, string OTPType = null) {
            Expression<Func<OTPRecord, bool>> query;
            var utcNow = DateTime.UtcNow;
            if (OTPType == null) {
                query = or => or.UserRecord.Id == userId && or.ExpirationUTCDate < utcNow;
            } else {
                query = or => or.UserRecord.Id == userId && or.PasswordType == OTPType && or.ExpirationUTCDate < utcNow;
            }
            var records = _repository.Fetch(query);
            foreach (var otp in records) {
                Delete(otp.Id);
            }
        }

        public OTPRecord Get(string password, string OTPType = null) {
            var query = PasswordQuery(password, OTPType);
            return _repository.Fetch(query).FirstOrDefault();
        }

        public IEnumerable<OTPRecord> Get(IUser user, string OTPType = null) {
            var query = UserQuery(user, OTPType);
            return _repository.Fetch(query);
        }

        private Expression<Func<OTPRecord, bool>> UserQuery(IUser user, string OTPType = null) {
            return UserQuery(user.Id, OTPType);
        }

        private Expression<Func<OTPRecord, bool>> UserQuery(int userId, string OTPType = null) {
            if (OTPType == null) {
                return or => or.UserRecord.Id == userId;
            } else {
                return or => or.UserRecord.Id == userId && or.PasswordType == OTPType;
            }
        }

        private Expression<Func<OTPRecord, bool>> PasswordQuery(string password, string OTPType = null) {
            if (OTPType == null) {
                return or => password == or.Password;
            } else {
                return or => password == or.Password && or.PasswordType == OTPType;
            }
        }
    }
}