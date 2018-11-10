using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class PublishExtensionPartDriver : ContentPartDriver<PublishExtensionPart> {
    }
}