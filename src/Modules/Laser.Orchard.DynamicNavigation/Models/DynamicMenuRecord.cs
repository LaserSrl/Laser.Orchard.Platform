using Orchard.ContentManagement.Records;

namespace Laser.Orchard.DynamicNavigation.Models {

  public class DynamicMenuRecord : ContentPartRecord {
    public virtual int MenuId { get; set; }
    public virtual int LevelsToShow { get; set; }
    public virtual bool ShowFirstLevelBrothers { get; set; }
  }
}