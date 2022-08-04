using Nwazet.Commerce.Aspects;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Models {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointPart
        : ContentPart<PickupPointPartRecord>, ITerritoryAddressAspect {

        public const string DefaultContentTypeName = "PickupPoint";

        public IEnumerable<int> TerritoriesIds => 
            new int[] { CityId, ProvinceId, CountryId };
        
        public string CountryName {
            get { return Retrieve(r => r.CountryName); }
            set { Store(r => r.CountryName, value); }
        }

        public int CountryId {
            get { return Retrieve(r => r.CountryId); }
            set { Store(r => r.CountryId, value); }
        }

        public string ProvinceName {
            get { return Retrieve(r => r.ProvinceName); }
            set { Store(r => r.ProvinceName, value); }
        }

        public int ProvinceId {
            get { return Retrieve(r => r.ProvinceId); }
            set { Store(r => r.ProvinceId, value); }
        }

        public string CityName {
            get { return Retrieve(r => r.CityName); }
            set { Store(r => r.CityName, value); }
        }

        public int CityId {
            get { return Retrieve(r => r.CityId); }
            set { Store(r => r.CityId, value); }
        }

        public string AddressLine1 {
            get { return Retrieve(r => r.AddressLine1); }
            set { Store(r => r.AddressLine1, value); }
        }
        public string AddressLine2 {
            get { return Retrieve(r => r.AddressLine2); }
            set { Store(r => r.AddressLine2, value); }
        }
        public string PostalCode {
            get { return Retrieve(r => r.PostalCode); }
            set { Store(r => r.PostalCode, value); }
        }

        public string AddressText(bool withCountry = true) {
            var strElements = new List<string>();
            if (!string.IsNullOrWhiteSpace(AddressLine1)) {
                strElements.Add(AddressLine1);
            }
            if (!string.IsNullOrWhiteSpace(AddressLine2)) {
                strElements.Add(AddressLine2);
            }
            if (!string.IsNullOrWhiteSpace(CityName)) {
                strElements.Add(CityName);
            }

            if (!string.IsNullOrWhiteSpace(ProvinceName)) {
                if (withCountry && !string.IsNullOrWhiteSpace(CountryName)) {
                    strElements.Add($"{ProvinceName} ({CountryName})");
                } else {
                    strElements.Add(ProvinceName);
                }
            } else if (withCountry && !string.IsNullOrWhiteSpace(CountryName)) {
                // edit the last element of the list so it has the country to it
                // to prevent a comma separator where it wouldn't be required
                var last = strElements.Last();
                strElements.RemoveAt(strElements.Count - 1);
                last = $"{last} ({CountryName})";
                strElements.Add(last);
            }


            if (!string.IsNullOrWhiteSpace(PostalCode)) {
                strElements.Add(PostalCode);
            }

            return string.Join(", ", strElements);
        }
    }
}