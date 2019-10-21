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
    }

    public class ManifestAppFileServices : IManifestAppFileServices {
        private readonly IRepository<ManifestAppFileRecord> _repository;

        public ManifestAppFileServices(IRepository<ManifestAppFileRecord> repository) {
            _repository = repository;
        }

        public ManifestAppFileRecord Get() {
            var robotsFileRecord = _repository.Table.FirstOrDefault();
            //if (robotsFileRecord == null) {
            //    robotsFileRecord = new ManifestAppFileRecord() {
            //        FileContent = ???
            //    };
            //    _repository.Create(robotsFileRecord);
            //}
            return robotsFileRecord;
        }


    }
}