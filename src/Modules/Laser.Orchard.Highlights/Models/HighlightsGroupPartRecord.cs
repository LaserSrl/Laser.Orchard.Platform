using Laser.Orchard.Highlights.Enums;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Highlights.Models {

  public class HighlightsGroupPartRecord : ContentPartRecord {
    public virtual string DisplayPlugin { get; set; }
    public virtual DisplayTemplate DisplayTemplate { get; set; }
    public virtual ItemsSourceTypes  ItemsSourceType {get;set;}
    public virtual int Query_Id { get; set; }
  }
}