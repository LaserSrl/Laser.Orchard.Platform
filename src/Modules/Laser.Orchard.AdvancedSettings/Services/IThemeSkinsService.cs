using Laser.Orchard.AdvancedSettings.ViewModels;
using Orchard;
using Orchard.UI.Resources;
using System.Collections.Generic;

namespace Laser.Orchard.AdvancedSettings.Services {
    public interface IThemeSkinsService : IDependency {
        /// <summary>
        /// Read the name of the customizations configured for the theme.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSkinNames();
        /// <summary>
        /// Read the variables that may be customized for the theme.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ThemeCssVariable> GetSkinVariables();
        /// <summary>
        /// Include the selected skin's resources in the registers.
        /// </summary>
        /// <param name="Style"></param>
        /// <param name="Script"></param>
        /// <param name="settingsName"></param>
        void IncludeSkin(ResourceRegister Style, ResourceRegister Script, string settingsName);
    }
}
