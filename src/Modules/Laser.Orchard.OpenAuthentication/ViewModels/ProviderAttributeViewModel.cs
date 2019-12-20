using Orchard.Localization;

namespace Laser.Orchard.OpenAuthentication.ViewModels {
    public class ProviderAttributeViewModel {
        public string AttributeKey { get; set; }
        public string AttributeValue { get; set; }
        public LocalizedString AttributeDescription { get; set; }
    }
}