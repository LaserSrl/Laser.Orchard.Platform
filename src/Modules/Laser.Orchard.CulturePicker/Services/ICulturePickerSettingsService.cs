using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.CulturePicker.Services {
    public interface ICulturePickerSettingsService : IDependency
    {
        IList<Models.ExtendedCultureRecord> CultureList(IEnumerable<string> iEnumerable=null, bool ordered = true);
        void UpdateCulture(Models.ExtendedCultureRecord culture);
        Models.SettingsModel ReadSettings();
        void WriteSettings(Models.SettingsModel settings);
        Models.ExtendedCultureRecord GetExtendedCulture(string culture);
    }

}