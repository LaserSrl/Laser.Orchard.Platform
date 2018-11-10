using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class NwazetContactPartHandler : ContentHandler {
        private readonly IRepository<AddressRecord> _addressRepository;
        public NwazetContactPartHandler(IRepository<NwazetContactPartRecord> repository, IRepository<AddressRecord> addressRepository) {
            _addressRepository = addressRepository;
            Filters.Add(StorageFilter.For(repository));
        }
    }
}