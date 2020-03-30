using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Drivers {
    /// <summary>
    /// Do not remove. if missing the EnsureAsFrontEndInvisiblePart is threated as a ContentPart and it can't be used as a visibility discriminant
    /// </summary>
    public class EnsureAsFrontEndInvisiblePartDriver :ContentPartDriver<EnsureAsFrontEndInvisiblePart>{
    }
}