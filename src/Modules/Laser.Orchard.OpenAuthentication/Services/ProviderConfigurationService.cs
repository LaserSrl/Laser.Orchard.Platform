using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard;
using Orchard.Data;
using System;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Orchard.Caching;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IProviderConfigurationService : IDependency {
        IEnumerable<ProviderConfigurationRecord> GetAll();
        ProviderConfigurationRecord Get(string providerName);
        void Delete(int id);
        void Create(ProviderConfigurationCreateParams parameters);
        bool VerifyUnicity(string providerName);
        bool VerifyUnicity(string providerName, int id);
        CreateProviderViewModel Get(Int32 id);
        void Edit(CreateProviderViewModel parameters);
    }

    public class ProviderConfigurationService : IProviderConfigurationService {
        private readonly IRepository<ProviderConfigurationRecord> _repository;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public ProviderConfigurationService(IRepository<ProviderConfigurationRecord> repository, ICacheManager cacheManager, ISignals signals) {
            _repository = repository;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public IEnumerable<ProviderConfigurationRecord> GetProviders() {
            try {
                return _cacheManager.Get(
                    "Laser.Orchard.OpenAuthentication.Providers",
                    ctx => {
                        ctx.Monitor(_signals.When("Laser.Orchard.OpenAuthentication.Providers.Changed"));
                        return _repository.Table.ToList();
                    });
            }
            catch {
                return null;
            }
        }
        private void syncRepositoryStatic() {
            _signals.Trigger("Laser.Orchard.OpenAuthentication.Providers.Changed");
        }

        public IEnumerable<ProviderConfigurationRecord> GetAll() {
            return GetProviders();
        }

        public ProviderConfigurationRecord Get(string providerName) {
            if (providerName == null)
                return null;
            return GetProviders().FirstOrDefault(o => o.ProviderName.Equals(providerName,StringComparison.OrdinalIgnoreCase));
        }

        public void Delete(int id) {
            _repository.Delete(_repository.Get(o => o.Id == id));
            syncRepositoryStatic();
        }

        public bool VerifyUnicity(string providerName) {
            return GetProviders().FirstOrDefault(o => o.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase)) == null;
        }
        public bool VerifyUnicity(string providerName, int id) {
            return GetProviders().FirstOrDefault(o => o.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase) && o.Id != id) == null;
        }

        public void Create(ProviderConfigurationCreateParams parameters) {
            _repository.Create(new ProviderConfigurationRecord {
                DisplayName = parameters.DisplayName,
                ProviderName = parameters.ProviderName,
                ProviderIdentifier = parameters.ProviderIdentifier,
                UserIdentifier = parameters.UserIdentifier,
                ProviderIdKey = parameters.ProviderIdKey,
                ProviderSecret = parameters.ProviderSecret,
                IsEnabled = 1
            });
            syncRepositoryStatic();
        }

        public void Edit(CreateProviderViewModel parameters) {
            var rec = GetProviders().FirstOrDefault(o => o.Id == parameters.Id);
            rec.DisplayName = parameters.DisplayName;
            rec.IsEnabled = parameters.IsEnabled ? 1 : 0;
            rec.ProviderIdentifier = parameters.ProviderIdentifier;
            rec.UserIdentifier = parameters.UserIdentifier;
            rec.ProviderIdKey = parameters.ProviderIdKey;
            rec.ProviderName = parameters.ProviderName;
            rec.ProviderSecret = parameters.ProviderSecret;
            _repository.Update(rec);
            syncRepositoryStatic();
        }


        public CreateProviderViewModel Get(Int32 id) {
            var cpvm = new CreateProviderViewModel();
            var prec = GetProviders().FirstOrDefault(o => o.Id == id);
            cpvm.Id = prec.Id;
            cpvm.DisplayName = prec.DisplayName;
            cpvm.IsEnabled = prec.IsEnabled == 1;
            cpvm.ProviderIdentifier = prec.ProviderIdentifier;
            cpvm.UserIdentifier = prec.UserIdentifier;
            cpvm.ProviderIdKey = prec.ProviderIdKey;
            cpvm.ProviderName = prec.ProviderName;
            cpvm.ProviderSecret = prec.ProviderSecret;
            return cpvm;
        }
    }

    public class ProviderConfigurationCreateParams {
        public string DisplayName { get; set; }
        public string ProviderName { get; set; }
        public string ProviderIdentifier { get; set; }
        public string UserIdentifier { get; set; }
        public string ProviderIdKey { get; set; }
        public string ProviderSecret { get; set; }
    }
}