using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard;
using Orchard.Data;
using System;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Orchard.Caching;
using Laser.Orchard.OpenAuthentication.Services.Clients;
using Orchard.Logging;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IProviderConfigurationService : IDependency {
        IEnumerable<ProviderConfigurationViewModel> GetAll();
        ProviderConfigurationViewModel Get(string providerName);
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

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        private IEnumerable<ProviderConfigurationViewModel> GetProviders() {
            try {
                return _cacheManager.Get(
                    "Laser.Orchard.OpenAuthentication.Providers",
                    ctx => {
                        ctx.Monitor(_signals.When("Laser.Orchard.OpenAuthentication.Providers.Changed"));
                        var configuration = _repository.Table.ToList()
                        .Select(x => {
                            var cfg = new ProviderConfigurationViewModel();
                            x.ToViewModel(cfg);
                            cfg.Attributes = GetProviderConfigurationAttributes(cfg.Id, cfg.ProviderName);
                            return cfg;
                        });
                        return configuration.ToList();
                    });
            }
            catch (Exception ex) {
                Logger.Error(ex, "An unexpected error occurred in GetProviders method");
                return new List<ProviderConfigurationViewModel>();
            }
        }
        private void syncRepositoryStatic() {
            _signals.Trigger("Laser.Orchard.OpenAuthentication.Providers.Changed");
        }

        public IEnumerable<ProviderConfigurationViewModel> GetAll() {
            return GetProviders();
        }

        public ProviderConfigurationViewModel Get(string providerName) {
            if (providerName == null)
                return null;
            return GetProviders().FirstOrDefault(o => o.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        }

        public void Delete(int id) {
            foreach (var dbAttr in _repositoryAttributes.Fetch(x => x.ProviderId == id)) {
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
            var rec = _repository.Get(o => o.Id == parameters.Id);
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
            cpvm.Attributes = prec.Attributes;
            return cpvm;
        }
        public List<ProviderAttributeViewModel> GetAttributes(int providerId) {
            var result = new List<ProviderAttributeViewModel>();
            var provider = Get(providerId);
            if (provider != null) {
                result = provider.Attributes;
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
            foreach (var viewAttr in attributes) {
                var dbAttr = dbValues.FirstOrDefault(x => x.AttributeKey == viewAttr.AttributeKey);
                // no record on db => create one
                if (dbAttr == null) {
                    _repositoryAttributes.Create(new ProviderAttributeRecord() {
                        ProviderId = providerId,
                        AttributeKey = viewAttr.AttributeKey,
                        AttributeValue = viewAttr.AttributeValue
                    });
                }
            }
            syncRepositoryStatic();
        }
        public bool HasAttributes(string providerName) {
            var client = _authenticationClients.FirstOrDefault(x => x.ProviderName == providerName);
            if (client != null) {
                return client.GetAttributeKeys().Count > 0;
            }
            else {
                return false;
            }
        }

        private List<ProviderAttributeViewModel> GetProviderConfigurationAttributes(int providerId, string providerName) {
            var result = new List<ProviderAttributeViewModel>();
            var client = _authenticationClients.FirstOrDefault(x => x.ProviderName == providerName);
            if (client != null) {
                var original = _repositoryAttributes.Fetch(x => x.ProviderId == providerId).ToList();
                foreach (var item in client.GetAttributeKeys()) {
                    var origValue = original.FirstOrDefault(x => x.AttributeKey == item.Key);
                    result.Add(new ProviderAttributeViewModel() {
                        AttributeKey = item.Key,
                        AttributeValue = origValue != null ? origValue.AttributeValue : "",
                        AttributeDescription = item.Value
                    });
                }
            }
            return result;
        }

    }
    public static class ProviderConfigurationExtensions {
        public static void ToViewModel(this ProviderConfigurationRecord record, ProviderConfigurationViewModel model) {
            model.Id = record.Id;
            model.DisplayName = record.DisplayName;
            model.ProviderIdentifier = record.ProviderIdentifier;
            model.ProviderIdKey = record.ProviderIdKey;
            model.ProviderName = record.ProviderName;
            model.ProviderSecret = record.ProviderSecret;
            model.UserIdentifier = record.UserIdentifier;
            model.IsEnabled = record.IsEnabled;
        }
        public static void ToRecord(this ProviderConfigurationViewModel model, ProviderConfigurationRecord record) {
            if (record == null) return;
            record.DisplayName = model.DisplayName;
            record.ProviderIdentifier = model.ProviderIdentifier;
            record.ProviderIdKey = model.ProviderIdKey;
            record.ProviderName = model.ProviderName;
            record.ProviderSecret = model.ProviderSecret;
            record.UserIdentifier = model.UserIdentifier;
            record.IsEnabled = model.IsEnabled;
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