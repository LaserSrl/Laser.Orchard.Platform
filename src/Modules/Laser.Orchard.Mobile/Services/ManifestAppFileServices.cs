using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Data;
using Orchard;
using Laser.Orchard.Mobile.Models;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Laser.Orchard.Mobile.ViewModels;

namespace Laser.Orchard.Mobile.Services {

    public interface IManifestAppFileServices : IDependency {
        ManifestAppFileRecord Get();

        Tuple<bool, IEnumerable<string>> Save(ManifestAppFileViewModel vieModel);
    }

    public class ManifestAppFileServices : IManifestAppFileServices {
        private readonly IRepository<ManifestAppFileRecord> _repository;
        private readonly ISignals _signals;

        public ManifestAppFileServices(IRepository<ManifestAppFileRecord> repository, ISignals signals) {
            _repository = repository;
            _signals = signals;
        }

        public ManifestAppFileRecord Get() {
            var manifestAppFileRecord = _repository.Table.FirstOrDefault();
            if (manifestAppFileRecord == null) {
                manifestAppFileRecord = new ManifestAppFileRecord() {
                    FileContent = "",
                    Enable = false,
                    DeveloperDomainText = "",
                    EnableDeveloperDomain = false
                };
                _repository.Create(manifestAppFileRecord);
            }
            return manifestAppFileRecord;
        }

        public Tuple<bool, IEnumerable<string>> Save(ManifestAppFileViewModel vieModel) {
            var validationResult = Validate(vieModel);
            if (validationResult.Item1) {
                var manifestAppFileRecord = Get();
                manifestAppFileRecord.FileContent = vieModel.Text;
                manifestAppFileRecord.Enable = vieModel.Enable;
                manifestAppFileRecord.DeveloperDomainText = vieModel.DeveloperDomainText;
                manifestAppFileRecord.EnableDeveloperDomain = vieModel.EnableDeveloperDomain;
                _signals.Trigger("ManifestAppFile.SettingsChanged");
            }
            return validationResult;
        }

        private Tuple<bool, IEnumerable<string>> Validate(ManifestAppFileViewModel vieModel) {

            //string schemaJson = @"{
            //        'definition': {
            //            'webcredentials': {
            //                'type':'object',
            //             'properties': {
            //              'apps': {
            //                   'type':'array'
            //              }
            //                 },
            //                'required': [ 'apps' ]
            //            }
            //        }
            //    }";

            if (String.IsNullOrEmpty(vieModel.Text)) {
                return new Tuple<bool, IEnumerable<string>>(true, new List<string>() {});
            }

            string schemaJson = @"{ }";

            JSchema schema = JSchema.Parse(schemaJson);
            try {
                JObject jText = JObject.Parse(vieModel.Text);
                IList<string> message;
                bool valid = jText.IsValid(schema, out message);
                return new Tuple<bool, IEnumerable<string>>(valid, message);
            }
            catch (JsonReaderException ex) {
                return new Tuple<bool, IEnumerable<string>>(false, new List<string>(){ex.Message});
            }                    
        }

    }
}