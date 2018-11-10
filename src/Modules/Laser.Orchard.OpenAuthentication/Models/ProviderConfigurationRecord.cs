namespace Laser.Orchard.OpenAuthentication.Models {
    public class ProviderConfigurationRecord {
        public virtual int Id { get; set; }
        public virtual int IsEnabled { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string ProviderName { get; set; }
        public virtual string ProviderIdKey { get; set; }
        public virtual string ProviderSecret { get; set; }
        public virtual string ProviderIdentifier { get; set; }
        public virtual string UserIdentifier { get; set; }
       }
}