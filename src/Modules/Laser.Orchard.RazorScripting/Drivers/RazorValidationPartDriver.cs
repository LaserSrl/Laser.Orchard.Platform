using Laser.Orchard.RazorScripting.Models;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.RazorScripting.Drivers {

    public class RazorValidationPartDriver : ContentPartDriver<RazorValidationPart> {
        // Moved logic to the handler to make sure it gets executed after the other drivers
        // for fields and parts. The driver is still required for the part to be properly
        // attached.
    }
}