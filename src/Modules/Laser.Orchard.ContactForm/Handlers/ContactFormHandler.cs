using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Laser.Orchard.ContactForm.Models;

namespace Laser.Orchard.ContactForm.Handlers
{
    /// <summary>
    /// Defines the behavior of the ContactForm part.
    /// </summary>
    public class ContactFormHandler : ContentHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactFormHandler"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public ContactFormHandler(IRepository<ContactFormRecord> repository)
        {
            // Tell this handler to use ContactFormRecord for storage.
            Filters.Add(StorageFilter.For(repository));
        }
    }
}