using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System.ComponentModel;
using Orchard.Localization;
using System;

namespace Laser.Orchard.Maps.Models
{
    #region Obsolete code
    //[Obsolete("Replaced with 'MapVersionRecord' to enable versioning of this content")]
    //public class MapRecord : ContentPartRecord
    //{
    //    public virtual float Latitude { get; set; }
    //    public virtual float Longitude { get; set; }
    //    public virtual string LocationInfo { get; set; }
    //    public virtual string LocationAddress { get; set; }
    //}
    #endregion

    public class MapVersionRecord : ContentPartVersionRecord {
        public virtual float Latitude { get; set; }
        public virtual float Longitude { get; set; }
        public virtual string LocationInfo { get; set; }
        public virtual string LocationAddress { get; set; }
    }

    public class MapPart : ContentPart<MapVersionRecord>
    {
        [Required]
        [DisplayName("Latitude")]
        public float Latitude
        {
            get { return this.Retrieve(r => r.Latitude); }
            set { this.Store(r => r.Latitude, value); }
        }

        [Required]
        [DisplayName("Longitude")]
        public float Longitude
        {
            get { return this.Retrieve(r => r.Longitude); }
            set { this.Store(r => r.Longitude, value); }
        }

        [DisplayName("Location title")]
        public string LocationInfo
        {
            get { return this.Retrieve(r => r.LocationInfo); }
            set { this.Store(r => r.LocationInfo, value); }
        }

        [DisplayName("Location address")]
        public string LocationAddress
        {
            get { return this.Retrieve(r => r.LocationAddress); }
            set { this.Store(r => r.LocationAddress, value); }
        }
    }

    //public class MapPart : ContentPart<MapRecord>
    //{
    //    [Required]
    //    [DisplayName("Latitude")]
    //    public float Latitude
    //    {
    //        get { return Record.Latitude; }
    //        set { Record.Latitude = value; }
    //    }

    //    [Required]
    //    [DisplayName("Longitude")]
    //    public float Longitude
    //    {
    //        get { return Record.Longitude; }
    //        set { Record.Longitude = value; }
    //    }
    //    [DisplayName("Location title")]
    //    public string LocationInfo {
    //        get { return Record.LocationInfo; }
    //        set { Record.LocationInfo = value; }
    //    }
    //    [DisplayName("Location address")]
    //    public string LocationAddress {
    //        get { return Record.LocationAddress; }
    //        set { Record.LocationAddress = value; }
    //    }
    //}
}