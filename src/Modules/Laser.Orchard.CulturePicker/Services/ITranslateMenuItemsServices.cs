using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using System.Collections.Generic;
using Laser.Orchard.CulturePicker.Models;
using Orchard;

namespace Laser.Orchard.CulturePicker.Services {
    public interface ITranslateMenuItemsServices : IDependency {
        bool TryTranslateAllSubmenus(TranslateMenuItemsPart part);
    }
}