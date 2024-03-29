﻿using Laser.Orchard.NwazetIntegration.Aspects;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nwazet.Commerce.Aspects;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Models {
    public class AddressOrderPart
        : ContentPart<AddressOrderPartRecord>,
        ITerritoryAddressAspect,
        IOrderExtensionAspect {

        public string ShippingCountryName {
            get { return Retrieve(r => r.ShippingCountryName); }
            set { Store(r => r.ShippingCountryName, value); }
        }
        public int ShippingCountryId {
            get { return Retrieve(r => r.ShippingCountryId); }
            set { Store(r => r.ShippingCountryId, value); }
        }
        public string ShippingCityName {
            get { return Retrieve(r => r.ShippingCityName); }
            set { Store(r => r.ShippingCityName, value); }
        }
        public int ShippingCityId {
            get { return Retrieve(r => r.ShippingCityId); }
            set { Store(r => r.ShippingCityId, value); }
        }
        public string ShippingProvinceName {
            get { return Retrieve(r => r.ShippingProvinceName); }
            set { Store(r => r.ShippingProvinceName, value); }
        }
        public int ShippingProvinceId {
            get { return Retrieve(r => r.ShippingProvinceId); }
            set { Store(r => r.ShippingProvinceId, value); }
        }

        public bool ShippingAddressIsOptional {
            get { return Retrieve(r => r.ShippingAddressIsOptional); }
            set { Store(r => r.ShippingAddressIsOptional, value); }
        }

        public string BillingCountryName {
            get { return Retrieve(r => r.BillingCountryName); }
            set { Store(r => r.BillingCountryName, value); }
        }
        public int BillingCountryId {
            get { return Retrieve(r => r.BillingCountryId); }
            set { Store(r => r.BillingCountryId, value); }
        }
        public string BillingCityName {
            get { return Retrieve(r => r.BillingCityName); }
            set { Store(r => r.BillingCityName, value); }
        }
        public int BillingCityId {
            get { return Retrieve(r => r.BillingCityId); }
            set { Store(r => r.BillingCityId, value); }
        }
        public string BillingProvinceName {
            get { return Retrieve(r => r.BillingProvinceName); }
            set { Store(r => r.BillingProvinceName, value); }
        }
        public int BillingProvinceId {
            get { return Retrieve(r => r.BillingProvinceId); }
            set { Store(r => r.BillingProvinceId, value); }
        }

        public string BillingFiscalCode {
            get { return Retrieve(r => r.BillingFiscalCode); }
            set { Store(r => r.BillingFiscalCode, value); }
        }
        public string BillingVATNumber {
            get { return Retrieve(r => r.BillingVATNumber); }
            set { Store(r => r.BillingVATNumber, value); }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public CustomerTypeOptions BillingCustomerType {
            get { return Retrieve(r => r.BillingCustomerType); }
            set { Store(r => r.BillingCustomerType, value); }
        }
        
        public bool BillingInvoiceRequest {
            get { return Retrieve(r => r.BillingInvoiceRequest); }
            set { Store(r => r.BillingInvoiceRequest, value); }
        }

        public IEnumerable<int> TerritoriesIds =>
            new int[] { ShippingCityId, ShippingProvinceId, ShippingCountryId,
                BillingCityId, BillingProvinceId, BillingCountryId };

        public int CountryId => ShippingCountryId;

        public int ProvinceId => ShippingProvinceId;

        public int CityId => ShippingCityId;

        public void ExtendCreation(CheckoutViewModel cvm) {
            // Verify address information in the AddressOrderPart
            if (cvm.ShippingRequired) {
                // may not have a shipping address if shipping isn't required
                ShippingCountryName = cvm.ShippingAddress.Country;
                ShippingCountryId = cvm.SelectedShippingAddressProvider
                    .GetShippingCountryId(cvm);
                ShippingCityName = cvm.ShippingAddress.City;
                ShippingCityId = cvm.SelectedShippingAddressProvider
                    .GetShippingCityId(cvm);
                ShippingProvinceName = cvm.ShippingAddress.Province;
                ShippingProvinceId = cvm.SelectedShippingAddressProvider
                    .GetShippingProvinceId(cvm);
                // added information to manage saving in bo
                ShippingAddressIsOptional = false;
            }
            else {
                ShippingAddressIsOptional = true;
            }
            // Billing address
            BillingCountryName = cvm.BillingAddressVM.Country;
            BillingCountryId = cvm.BillingAddressVM.CountryId;
            BillingCityName = cvm.BillingAddressVM.City;
            BillingCityId = cvm.BillingAddressVM.CityId;
            BillingProvinceName = cvm.BillingAddressVM.Province;
            BillingProvinceId = cvm.BillingAddressVM.ProvinceId;
            BillingInvoiceRequest = cvm.BillingAddressVM.InvoiceRequest;
            BillingVATNumber = cvm.BillingAddressVM.VATNumber;
            BillingFiscalCode = cvm.BillingAddressVM.FiscalCode;
            BillingCustomerType = cvm.BillingAddressVM.CustomerType;
        }
    }
}