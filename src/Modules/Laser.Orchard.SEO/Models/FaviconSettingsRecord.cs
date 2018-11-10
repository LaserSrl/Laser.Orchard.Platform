using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.SEO.Models {

  [OrchardFeature("Laser.Orchard.Favicon")]
  public class FaviconSettingsRecord : ContentPartRecord {
    public virtual string FaviconUrl { get; set; }
  }
}