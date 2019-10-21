using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization;
using Orchard;
using System.Web.Mvc;
using Laser.Orchard.UsersExtensions.Models;

namespace Laser.Orchard.UsersExtensions.Services {

    public interface IManifestAppFileServices : IDependency {
        ManifestAppFileRecord Get();

        Tuple<bool, IEnumerable<string>> Save(string text);
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

        public Tuple<bool, IEnumerable<string>> Save(string text) {
            var manifestAppFileRecord = Get();
            manifestAppFileRecord.FileContent = text;
            _signals.Trigger("ManifestAppFile.SettingsChanged");
            //var validationResult = Validate(text); //volendo si può inserire un meccanismo di validazione del file (vedi robots.txt)
            return new Tuple<bool, IEnumerable<string>>(true, null);
        }

    }
}