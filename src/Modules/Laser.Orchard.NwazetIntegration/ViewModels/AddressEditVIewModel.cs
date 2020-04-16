using Laser.Orchard.NwazetIntegration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class AddressEditViewModel {

        public AddressEditViewModel() {
            AddressRecord = new AddressRecord();

            Errors = new List<string>();
            Information = new List<string>();
        }
        public AddressEditViewModel(AddressRecord address) : this() {
            AddressRecord = address;
        }
        public AddressEditViewModel(int id) : this() {
            AddressRecord = new AddressRecord() { Id = id };
        }

        public List<string> Errors { get; set; }
        public List<string> Information { get; set; }

        public AddressRecord AddressRecord { get; set; }

        public AddressRecordType AddressType {
            get { return AddressRecord.AddressType; }
            set { AddressRecord.AddressType = value; }
        }
        public string Honorific {
            get { return AddressRecord.Honorific; }
            set { AddressRecord.Honorific = value; }
        }
        [Required]
        public string FirstName {
            get { return AddressRecord.FirstName; }
            set { AddressRecord.FirstName = value; }
        }
        [Required]
        public string LastName {
            get { return AddressRecord.LastName; }
            set { AddressRecord.LastName = value; }
        }
        public string Company {
            get { return AddressRecord.Company; }
            set { AddressRecord.Company = value; }
        }
        [Required]
        public string Address1 {
            get { return AddressRecord.Address1; }
            set { AddressRecord.Address1 = value; }
        }
        public string Address2 {
            get { return AddressRecord.Address2; }
            set { AddressRecord.Address2 = value; }
        }
        [Required]
        public string City {
            get { return AddressRecord.City; }
            set { AddressRecord.City = value; }
        }
        [Required]
        public string Province {
            get { return AddressRecord.Province; }
            set { AddressRecord.Province = value; }
        }
        [Required]
        public string PostalCode {
            get { return AddressRecord.PostalCode; }
            set { AddressRecord.PostalCode = value; }
        }
        public string Country {
            get { return AddressRecord.Country; }
            set { AddressRecord.Country = value; }
        }

        /// <summary>
        /// Id of the TerritoryInternalRecord that matches the country
        /// </summary>
        public int CountryId {
            get { return AddressRecord.CountryId; }
            set { AddressRecord.CountryId = value; }
        }
        /// <summary>
        /// Id of the TerritoryInternalRecord that matches the city
        /// </summary>
        public int CityId {
            get { return AddressRecord.CityId; }
            set { AddressRecord.CityId = value; }
        }
        /// <summary>
        /// Id of the TerritoryInternalRecord that matches the province
        /// </summary>
        public int ProvinceId {
            get { return AddressRecord.ProvinceId; }
            set { AddressRecord.ProvinceId = value; }
        }

        public IEnumerable<SelectListItem> Countries { get; set; }
        // These next two lists are used where in the frontend we allow the choice
        // as to whether the address is shipping or billing.
        public IEnumerable<SelectListItem> ShippingCountries { get; set; }
        public IEnumerable<SelectListItem> BillingCountries { get; set; }
    }
}