using Nwazet.Commerce.Models;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class TerritoryISO3166CodePartRecord : ContentPartRecord {
        /*
         * https://en.wikipedia.org/wiki/ISO_3166
         * Initially we are going to use this for countries only, so it may make sense
         * to have more fields here, to store the specific codes used in ISO 3166-1 to
         * represent countries. On the other hand, ISO 3166-2 only has one alphanumeric
         * code to represent subdivisions.
         * Since the ContentPart for this record can be attached to any Territory, and
         * those may not represent a country, it may make sense that the ISO code used
         * to represent it is not tied to the definitions used for countries.
         * This leaves the complexity that its interpretation is up to the downstream
         * modules reading from it.
         * 
         * Worse comes to worse, adding three strings here to cover the entire standard
         * won't be too hard.
         */
        public virtual string ISO3166Code { get; set; }
        public virtual TerritoryInternalRecord TerritoryInternalRecord { get; set; }
    }
}