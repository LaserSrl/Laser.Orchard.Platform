using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Laser.Orchard.ContactForm.Models;
using System.Linq;
using Orchard.Core.Common.Models;

namespace Laser.Orchard.ContactForm.Handlers {
    /// <summary>
    /// Defines the behavior of the ContactForm part.
    /// </summary>
    public class ContactFormHandler : ContentHandler {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactFormHandler"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public ContactFormHandler(IRepository<ContactFormRecord> repository) {
            // Tell this handler to use ContactFormRecord for storage.
            Filters.Add(StorageFilter.For(repository));
        }
        protected override void Activating(ActivatingContentContext context) {
            // attach the IdentityPart wherever we have a ContactFormPart
            if (context.Definition.Parts.Any(ctpd => ctpd.PartDefinition.Name.Equals("ContactFormPart"))) {
                context.Builder.Weld<IdentityPart>();
            }
            base.Activating(context);
        }
    }
}