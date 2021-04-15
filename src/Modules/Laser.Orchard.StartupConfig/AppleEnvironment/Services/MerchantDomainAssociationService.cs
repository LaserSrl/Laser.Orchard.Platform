using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Data;
using Orchard;
using Laser.Orchard.StartupConfig.AppleEnvironment.Models;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Laser.Orchard.StartupConfig.AppleEnvironment.ViewModels;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.AppleEnvironment.Services {

    public interface IMerchantDomainAssociationService : IDependency {
        MerchantDomainAssociationRecord Get();

        void Save(MerchantDomainAssociationViewModel viewModel);
    }

    [OrchardFeature("Laser.Orchard.ApplePay.DomainAssociation")]
    public class MerchantDomainAssociationService : IMerchantDomainAssociationService {
        private readonly IRepository<MerchantDomainAssociationRecord> _repository;
        private readonly ISignals _signals;

        public MerchantDomainAssociationService(IRepository<MerchantDomainAssociationRecord> repository, ISignals signals) {
            _repository = repository;
            _signals = signals;
        }

        public MerchantDomainAssociationRecord Get() {
            var merchantDomainAssociationFileRecord = _repository.Table.FirstOrDefault();
            if (merchantDomainAssociationFileRecord == null) {
                merchantDomainAssociationFileRecord = new MerchantDomainAssociationRecord() {
                    FileContent = "",
                    Enable = false,
                };
                _repository.Create(merchantDomainAssociationFileRecord);
            }
            return merchantDomainAssociationFileRecord;
        }

        public void Save(MerchantDomainAssociationViewModel viewModel) {
            var merchantDomainAssociationFileRecord = Get();
            merchantDomainAssociationFileRecord.FileContent = viewModel.Text;
            merchantDomainAssociationFileRecord.Enable = viewModel.Enable;
            _signals.Trigger("MerchantDomainAssociation.SettingsChanged");
        }
    }
}