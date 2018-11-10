using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.MailCommunication.Extensions {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public static class UrlHelperExtensions {

        public static string UnsubscribeMailCommunication(this UrlHelper urlHelper) {
            return urlHelper.Action("Index", "Unsubscribe", new { area = "Laser.Orchard.MailCommunication" });
        }

        public static string ConfirmUnsubscribeMailCommunication(this UrlHelper urlHelper) {
            return urlHelper.Action("ConfirmUnsubscribe", "Unsubscribe", new { area = "Laser.Orchard.MailCommunication" });
        }

    }
}