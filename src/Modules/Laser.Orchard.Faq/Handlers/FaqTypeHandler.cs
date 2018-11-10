using Laser.Orchard.Faq.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Faq.Handlers
{
    public class FaqTypeHandler : ContentHandler
    {
        public FaqTypeHandler(IRepository<FaqTypePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
            OnRemoved<FaqTypePart>((context, part) => repository.Delete(part.Record));
            OnIndexing<FaqTypePart>((context, contactPart) => context.DocumentIndex.Add("faqtype_title", contactPart.Title).Analyze().Store());

        }
    }
}