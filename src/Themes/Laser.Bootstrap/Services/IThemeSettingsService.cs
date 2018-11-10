using System.Collections.Generic;
using Laser.Bootstrap.Models;
using Laser.Bootstrap.ViewModels;
using Orchard;

namespace Laser.Bootstrap.Services {
    public interface IThemeSettingsService : IDependency {
        ThemeSettingsRecord GetSettings();
        IList<ThemeInfoViewModel> GetThemes();
    }
}
