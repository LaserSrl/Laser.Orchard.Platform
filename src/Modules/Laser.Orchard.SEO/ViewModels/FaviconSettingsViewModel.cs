using Orchard.Environment.Extensions;

namespace Laser.Orchard.SEO.ViewModels {

  [OrchardFeature("Laser.Orchard.Favicon")]
  public class FaviconSettingsViewModel {
    public string FaviconUrl { get; set; }
  }
}