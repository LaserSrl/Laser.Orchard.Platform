using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard;
using Orchard.Data;
using System;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Orchard.Caching;
using Laser.Orchard.OpenAuthentication.Services.Clients;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IProviderConfigurationService : IDependency {
        IEnumerable<ProviderConfigurationRecord> GetAll();
        ProviderConfigurationRecord Get(string providerName);
        void Delete(int id);
        int Create(ProviderConfigurationCreateParams parameters);
        bool VerifyUnicity(string providerName);
        bool VerifyUnicity(string providerName, int id);
        CreateProviderViewModel Get(Int32 id);
        void Edit(CreateProviderViewModel parameters);
        List<ProviderAttributeViewModel> GetAttributes(int providerId);
        void SaveAttributes(int providerId, List<ProviderAttributeViewModel> viewModel);
        bool HasAttributes(string providerName);
    }

    public class ProviderConfigurationService : IProviderConfigurationService {
        private readonly IRepository<ProviderConfigurationRecord> _repository;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IRepository<ProviderAttributeRecord> _repositoryAttributes;
        private readonly IEnumerable<IExternalAuthenticationClient> _authenticationClients;

        public ProviderConfigurationService(
            IRepository<ProviderConfigurationRecord> repository, 
            ICacheManager cacheManager, 
            ISignals signals,
            IRepository<ProviderAttributeRecord> repositoryAttributes,
            IEnumerable<IExternalAuthenticationClient> authenticationClients) {
            _repository = repository;
            _cacheManager = cacheManager;
            _signals = signals;
            _repositoryAttributes = repositoryAttributes;
            _authenticationClients = authenticationClients;
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
            foreach(var dbAttr in _repositoryAttributes.Fetch(x => x.ProviderId == id)) {
                _repositoryAttributes.Delete(dbAttr);
            }
            _repository.Delete(_repository.Get(o => o.Id == id));
            syncRepositoryStatic();
        }

        public bool VerifyUnicity(string providerName) {
            return GetProviders().FirstOrDefault(o => o.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase)) == null;
        }
        public bool VerifyUnicity(string providerName, int id) {
            return GetProviders().FirstOrDefault(o => o.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase) && o.Id != id) == null;
        }

        public int Create(ProviderConfigurationCreateParams parameters) {
            var provider = new ProviderConfigurationRecord {
                DisplayName = parameters.DisplayName,
                ProviderName = parameters.ProviderName,
                ProviderIdentifier = parameters.ProviderIdentifier,
                UserIdentifier = parameters.UserIdentifier,
                ProviderIdKey = parameters.ProviderIdKey,
                ProviderSecret = parameters.ProviderSecret,
                IsEnabled = 1
            };
            _repository.Create(provider);
            syncRepositoryStatic();
            return provider.Id;
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
        public List<ProviderAttributeViewModel> GetAttributes(int providerId) {
            var result = new List<ProviderAttributeViewModel>();
            var provider = _repository.Get(providerId);
            if(provider != null) {
                var client = _authenticationClients.FirstOrDefault(x => x.ProviderName == provider.ProviderName);
                if(client != null) {
                    var original = _repositoryAttributes.Fetch(x => x.ProviderId == providerId).ToList();
                    foreach(var item in client.GetAttributeKeys()) {
                        var origValue = original.FirstOrDefault(x => x.AttributeKey == item.Key);
                        result.Add(new ProviderAttributeViewModel() {
                            AttributeKey = item.Key,
                            AttributeValue = origValue != null ? origValue.AttributeValue : "",
                            AttributeDescription = item.Value
                        });
                    }
                }
            }
            return result;
        }
        public void SaveAttributes(int providerId, List<ProviderAttributeViewModel> attributes) {
            var dbValues = _repositoryAttributes.Fetch(x => x.ProviderId == providerId);
            //update or delete attributes that are already on db 
            foreach (var dbAttr in dbValues) {
                var viewAttr = attributes.FirstOrDefault(x => x.AttributeKey == dbAttr.AttributeKey);
                if (viewAttr != null) {
                    dbAttr.AttributeValue = viewAttr.AttributeValue;
                    _repositoryAttributes.Update(dbAttr);
                }
                else {
                    _repositoryAttributes.Delete(dbAttr);
                }
            }
            //insert new attributes
            foreach(var viewAttr in attributes) {
                var dbAttr = dbValues.FirstOrDefault(x => x.AttributeKey == viewAttr.AttributeKey);
                // no record on db => create one
                if(dbAttr == null) {
                    _repositoryAttributes.Create(new ProviderAttributeRecord() {
                        ProviderId = providerId,
                        AttributeKey = viewAttr.AttributeKey,
                        AttributeValue = viewAttr.AttributeValue
                    });
                }
            }
        }
        public bool HasAttributes(string providerName) {
            var client = _authenticationClients.FirstOrDefault(x => x.ProviderName == providerName);
            if(client != null) {
                return client.GetAttributeKeys().Count > 0;
            }
            else {
                return false;
            }
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