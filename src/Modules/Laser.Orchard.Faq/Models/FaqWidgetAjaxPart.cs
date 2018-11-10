using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Faq.Models
{

    public class FaqWidgetAjaxPartRecord : ContentPartRecord
    {

    }

    public class FaqWidgetAjaxPart : ContentPart<FaqWidgetAjaxPartRecord>
    {

    }
}