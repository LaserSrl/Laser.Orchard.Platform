namespace Laser.Orchard.OpenAuthentication.Models {
    public class ProviderAttributeRecord {
        public virtual int Id { get; set; }
        public virtual int ProviderId { get; set; }
        public virtual string AttributeKey { get; set; }
        public virtual string AttributeValue { get; set; }
    }
}