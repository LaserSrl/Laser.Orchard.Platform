using System.Collections.Generic;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard;
using Orchard.Data;
using Orchard.Security;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IUserProviderServices : IDependency {
        UserProviderRecord Get(string providerName, string providerUserId);
        void Create(string providerName, string providerUserId, IUser user, string providerUserData = null);
        void Update(string providerName, string providerUserId, IUser user, string providerUserData = null);
        IEnumerable<UserProviderRecord> Get(IUser user);
        IEnumerable<UserProviderRecord> Get(int userId);
    }

    public class UserProviderServices : IUserProviderServices {
        private readonly IRepository<UserProviderRecord> _repository;

        public UserProviderServices(IRepository<UserProviderRecord> repository) {
            _repository = repository;
        }

        public UserProviderRecord Get(string providerName, string providerUserId) {
            return _repository.Get(o => o.ProviderName == providerName && o.ProviderUserId == providerUserId);
        }

        public IEnumerable<UserProviderRecord> Get(IUser user) {
            return Get(user.Id);
        }

        public IEnumerable<UserProviderRecord> Get(int userId) {
            return _repository.Fetch(o => o.UserId == userId);
        }

        public void Create(string providerName, string providerUserId, IUser user, string providerUserData = null) {
            var record = new UserProviderRecord {
                UserId = user.Id,
                ProviderName = providerName,
                ProviderUserId = providerUserId,
                ProviderUserData = providerUserData
            };

            _repository.Create(record);
        }

        public void Update(string providerName, string providerUserId, IUser user, string providerUserData = null) {
            var record = Get(providerName, providerUserId);

            record.UserId = user.Id;
            if (string.IsNullOrEmpty(record.ProviderUserData)) {
                record.ProviderUserData = providerUserData;
            }

            _repository.Update(record);
        }
    }
}