using Orchard;
using Orchard.UI.Resources;
using System.Collections.Generic;

namespace Laser.Orchard.AdvancedSettings.Services {
    public interface IThemeSkinsService : IDependency {
        /// <summary>
        /// Read the /Styles/Skins folders for the current theme. Find the
        /// css files to be used as skins.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSkinNames();
        /// <summary>
        /// Include the selected skin's resources in the registers.
        /// </summary>
        /// <param name="Style"></param>
        /// <param name="Script"></param>
        /// <param name="settingsName"></param>
        void IncludeSkin(ResourceRegister Style, ResourceRegister Script, string settingsName);
    }
}
