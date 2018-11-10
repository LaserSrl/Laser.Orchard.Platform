using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Exceptions {
    public class RedirectRuleDuplicateException : Exception {
        public RedirectRuleDuplicateException() :
            base("Rules with same SourceURL are not valid.") { }
        public RedirectRuleDuplicateException(LocalizedString message) : base(message.Text) { }
        public RedirectRuleDuplicateException(string message) : base(message) { }
        public RedirectRuleDuplicateException(string message, Exception innerException) : base (message, innerException) { }
    }
}