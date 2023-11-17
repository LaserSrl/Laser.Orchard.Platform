using Laser.Orchard.RazorScripting.Models;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.RazorScripting.Drivers {
    public class RazorScriptingFieldDriver : ContentFieldDriver<RazorScriptingField> {
        // I need to have a driver, even if it does nothing specific, to have a field
        // being processed, otherwise it won't be found by the handler.

    }
}