using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization;
using Orchard;
using System.Web.Mvc;
using Laser.Orchard.Mobile.Models;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Laser.Orchard.Mobile.Services {

    public interface IManifestAppFileServices : IDependency {
        ManifestAppFileRecord Get();

        Tuple<bool, IEnumerable<string>> Save(string text, bool enable);
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
                    FileContent = ""
                };
                _repository.Create(manifestAppFileRecord);
            }
            return manifestAppFileRecord;
        }

        public Tuple<bool, IEnumerable<string>> Save(string text, bool enable) {


            var validationResult = Validate(text);
            if (validationResult.Item1) {
                var manifestAppFileRecord = Get();
                manifestAppFileRecord.FileContent = text;
                manifestAppFileRecord.Enable = enable;
                _signals.Trigger("ManifestAppFile.SettingsChanged");
            }
            return validationResult;
        }

        private Tuple<bool, IEnumerable<string>> Validate(string text) {

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

            if (String.IsNullOrEmpty(text)) {
                return new Tuple<bool, IEnumerable<string>>(true, new List<string>() {});
            }

            string schemaJson = @"{ }";

            JSchema schema = JSchema.Parse(schemaJson);
            try {
                JObject jText = JObject.Parse(text);
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