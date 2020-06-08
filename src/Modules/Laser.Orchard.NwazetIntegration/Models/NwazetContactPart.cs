using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;


namespace Laser.Orchard.NwazetIntegration.Models {

    public class NwazetContactPart : ContentPart<NwazetContactPartRecord> {

        public IList<AddressRecord> NwazetAddressRecord
        {
            get
            {
                return Record.NwazetAddressRecord.Select(s => s).OrderByDescending(o => o.TimeStampUTC).ToList();
            }
        }
    }

    public class NwazetContactPartRecord : ContentPartRecord {
        public NwazetContactPartRecord() {
            NwazetAddressRecord = new List<AddressRecord>();
        }

        public virtual IList<AddressRecord> NwazetAddressRecord { get; set; }
    }

    public enum AddressRecordType { ShippingAddress, BillingAddress }

    public class AddressRecord {
        public AddressRecord() {
            this.TimeStampUTC = DateTime.UtcNow;
        }
        public virtual int Id { get; set; }
        public virtual int NwazetContactPartRecord_Id { get; set; }
        public virtual DateTime TimeStampUTC { get; set; }
        public virtual AddressRecordType AddressType { get; set; }
        public virtual string Honorific { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Company { get; set; }
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string City { get; set; }
        public virtual string Province { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string Country { get; set; }

        // Properties added for the advanced address configuration that 
        // uses territories
        public virtual int CountryId { get; set; }
        public virtual int CityId { get; set; }
        public virtual int ProvinceId { get; set; }

        public override bool Equals(object objAddress) {
            if (!(objAddress is AddressRecord))
                return false;
            AddressRecord obj = objAddress as AddressRecord;
            if (obj == null)
                return false;
            if (this.NwazetContactPartRecord_Id.Equals((int)(obj.NwazetContactPartRecord_Id)) &&
                    object.Equals(this.AddressType, obj.AddressType) &&
                    object.Equals(this.Honorific, obj.Honorific) &&
                    object.Equals(this.FirstName, obj.FirstName) &&
                    object.Equals(this.LastName, obj.LastName) &&
                    object.Equals(this.Company, obj.Company) &&
                    object.Equals(this.Address1, obj.Address1) &&
                    object.Equals(this.Address2, obj.Address2) &&
                    object.Equals(this.City, obj.City) &&
                    object.Equals(this.Province, obj.Province) &&
                    object.Equals(this.PostalCode, obj.PostalCode) &&
                    object.Equals(this.Country, obj.Country)) {
                return true;
            }
            return false;
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}