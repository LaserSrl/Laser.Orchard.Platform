namespace Laser.Orchard.DataProtection.Models {
    public class DataRestrictionsRecord {
        public virtual int Id { get; set; }
        public virtual string Restrictions { get; set; }
        public virtual int DataRestrictionsPartRecord_id { get; set; }
    }
}