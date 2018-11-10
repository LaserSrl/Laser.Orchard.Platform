using Orchard.ContentManagement;

namespace Laser.Orchard.DynamicNavigation.Models {

  public class DynamicMenuPart : ContentPart<DynamicMenuRecord> {

    public int MenuId {
      get { return Record.MenuId; }
      set { Record.MenuId = value; }
    }

    public int LevelsToShow {
      get { return Record.LevelsToShow; }
      set { Record.LevelsToShow = value; }
    }
    public bool ShowFirstLevelBrothers {
        get { return Record.ShowFirstLevelBrothers; }
        set { Record.ShowFirstLevelBrothers = value; }
    }

  }
}