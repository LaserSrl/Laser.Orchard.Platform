using Laser.Orchard.NwazetIntegration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class AddressEditViewModel {

        public AddressEditViewModel() {
            AddressRecord = new AddressRecord();
        }

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
        public string Province {
            get { return AddressRecord.Province; }
            set { AddressRecord.Province = value; }
        }
        public string PostalCode {
            get { return AddressRecord.PostalCode; }
            set { AddressRecord.PostalCode = value; }
        }
        public string Country {
            get { return AddressRecord.Country; }
            set { AddressRecord.Country = value; }
        }
    }
}