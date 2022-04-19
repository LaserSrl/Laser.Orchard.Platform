﻿using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class TerritoryISO3166CodePartRecord : ContentPartRecord {
        public virtual string ISO3166Code { get; set; }
        public virtual TerritoryInternalRecord TerritoryInternalRecord { get; set; }
    }
}