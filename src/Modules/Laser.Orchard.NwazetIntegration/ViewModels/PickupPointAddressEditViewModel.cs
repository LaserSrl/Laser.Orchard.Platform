using Laser.Orchard.NwazetIntegration.Models;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointAddressEditViewModel {
        // This address view model is a smaller and simpler version of a "full"
        // AddressEditViewModel, because a PickupPoint only needs the "address"
        // information, rather than all the information we generally require, 
        // such as FirstName and LastName. Moreover, this viewmodel is not backed
        // by an AddressRecord directly, because it generally won't be tied to
        // a ContactRecord.

        public PickupPointAddressEditViewModel() { }
        public PickupPointAddressEditViewModel(PickupPointPart part) {
            Address1 = part.AddressLine1;
            Address2 = part.AddressLine2;
            PostalCode = part.PostalCode;
            City = part.CityName;
            Province = part.ProvinceName;
            Country = part.CountryName;
            CountryId = part.CountryId;
            ProvinceId = part.ProvinceId;
            CityId = part.CityId;
        }

        public List<string> Errors { get; set; }
        public List<string> Information { get; set; }

        [Required]
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Province { get; set; }
        [Required]
        public string PostalCode { get; set; }
        public string Country { get; set; }
        /// <summary>
        /// Id of the TerritoryInternalRecord that matches the country
        /// </summary>
        public int CountryId { get; set; }
        /// <summary>
        /// Id of the TerritoryInternalRecord that matches the city
        /// </summary>
        public int CityId { get; set; }
        /// <summary>
        /// Id of the TerritoryInternalRecord that matches the province
        /// </summary>
        public int ProvinceId { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
    }
}