using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Newtonsoft.Json;
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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Errors { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Information { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AddressRecord AddressRecord { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AddressRecordType AddressType {
            get { return AddressRecord.AddressType; }
            set { AddressRecord.AddressType = value; }
        }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Honorific {
            get { return AddressRecord.Honorific; }
            set { AddressRecord.Honorific = value; }
        }
        [Required]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName {
            get { return AddressRecord.FirstName; }
            set { AddressRecord.FirstName = value; }
        }
        [Required]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName {
            get { return AddressRecord.LastName; }
            set { AddressRecord.LastName = value; }
        }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Company {
            get { return AddressRecord.Company; }
            set { AddressRecord.Company = value; }
        }
        [Required]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Address1 {
            get { return AddressRecord.Address1; }
            set { AddressRecord.Address1 = value; }
        }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Address2 {
            get { return AddressRecord.Address2; }
            set { AddressRecord.Address2 = value; }
        }
        [Required]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string City {
            get { return AddressRecord.City; }
            set { AddressRecord.City = value; }
        }
        [Required]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Province {
            get { return AddressRecord.Province; }
            set { AddressRecord.Province = value; }
        }
        [Required]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PostalCode {
            get { return AddressRecord.PostalCode; }
            set { AddressRecord.PostalCode = value; }
        }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<SelectListItem> Countries { get; set; }
        // These next two lists are used where in the frontend we allow the choice
        // as to whether the address is shipping or billing.
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<SelectListItem> ShippingCountries { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<SelectListItem> BillingCountries { get; set; }

        public static AddressEditViewModel CreateVM(
            IAddressConfigurationService _addressConfigurationService) {

            return new AddressEditViewModel() {
                Countries = _addressConfigurationService
                    .CountryOptions(),
                ShippingCountries = _addressConfigurationService
                    .CountryOptions(AddressRecordType.ShippingAddress),
                BillingCountries = _addressConfigurationService
                    .CountryOptions(AddressRecordType.BillingAddress)
            };
        }

        public static AddressEditViewModel CreateVM(
            IAddressConfigurationService _addressConfigurationService,
            AddressRecordType addressRecordType) {
            var vm = CreateVM(_addressConfigurationService);
            vm.AddressType = addressRecordType;
            return vm;
        }

        public static AddressEditViewModel CreateVM(
            IAddressConfigurationService _addressConfigurationService,
            AddressRecordType addressRecordType,
            AddressEditViewModel viewModel) {

            if (viewModel == null) {
                viewModel = CreateVM(_addressConfigurationService);
            } else {
                viewModel.Countries = _addressConfigurationService
                    .CountryOptions();
                if (addressRecordType == AddressRecordType.ShippingAddress) {
                    viewModel.ShippingCountries = _addressConfigurationService
                        .CountryOptions(AddressRecordType.ShippingAddress, viewModel.CountryId);
                }
                if (addressRecordType == AddressRecordType.BillingAddress) {
                    viewModel.BillingCountries = _addressConfigurationService
                    .CountryOptions(AddressRecordType.BillingAddress, viewModel.CountryId);
                }
                if (viewModel.ProvinceId <= 0 && !string.IsNullOrWhiteSpace(viewModel.Province)) { viewModel.ProvinceId = -1; }
                if (viewModel.CityId <= 0 && !string.IsNullOrWhiteSpace(viewModel.City)) { viewModel.CityId = -1; }

            }
            viewModel.AddressType = addressRecordType;
            return viewModel;
        }
    }
}